using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

struct Armthrow
{
    public Vector3 target;
    public float throwStartTime;
    public float throwDistance;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public Vector3 returnPosition;
}

public class Arm : MonoBehaviour
{
    private const float ArmRotationSpeed = 3f;
    private const float HitDamage = 1f;
    private const float VulnerabilityTime = 4f;
    private const float Maxlife2Players = 4f;
    private const float Maxlife4Players = 8f;
    private const float HitCooldown = 0.7f;
    private const float PlayerDamageOnBoss = 150;
    private const float BossDamageOnPlayer = 10f;
    private bool _isShooting;
    private bool _isReturning;
    private bool _isTraveling;
    private bool _isDestroyed;
    private bool _isVulnerable;
    private bool _isRotating;
    private bool _isReady;
    private bool[] _canBeHit;
    private Vector3 _defaultPosition;
    private Quaternion _defaultRotation;
    private float _baseArmThrowSpeed = 5.0F;
    private float _maxArmThrowSpeed = 20f;
    private float _currentArmThrowSpeed;
    private Armthrow _currentArmThrow;
    private Camera _mainCamera;
    private ParticleSystem _smokeSystem;
    private ParticleSystem _explosionSystem;
    private ParticleSystem _shockwaveSystem;
    private Player _lastPlayerHit;
    private Vector3 _deathPosition;
    private LifeBar _lifeBar;
    private List<Player> _players;
    private BossSounds _bossSounds;

    public bool isRightArm;
    public Vector3 test = new Vector3(-29f, 10f, 0f);
    [Header("Components")]
    [SerializeField] private RectTransform _lifeBarTransform;
    [SerializeField] private Slider _lifeBarSlider;
    [SerializeField] private Transform _fist;
    [SerializeField] private List<Collider> _colliders;

    [Header("Particle effects")]
    [SerializeField] private GameObject _smokeObject;
    [SerializeField] private GameObject _explosionObject;
    [SerializeField] private GameObject _shockwaveObject;

    private void Start()
    {
        _mainCamera = StaticObjects.CameraController.Camera;
        _smokeObject.SetActive(false);
        _explosionObject.SetActive(false);
        _shockwaveObject.SetActive(false);
        _smokeSystem = _smokeObject.GetComponent<ParticleSystem>();
        _explosionSystem = _explosionObject.GetComponent<ParticleSystem>();
        _shockwaveSystem = _shockwaveObject.GetComponent<ParticleSystem>();
        _isDestroyed = false;
        _isVulnerable = false;
        _isReady = true;
        _players = StaticObjects.GameController.GetPlayers();
        _canBeHit = Enumerable.Repeat(true, _players.Count).ToArray();
        float maxLife = StaticObjects.GameController.GetPlayers().Count <= 2 ? Maxlife2Players : Maxlife4Players;
        _lifeBar = new LifeBar(maxLife, _mainCamera, _lifeBarSlider, 0f, 1.5f, -2f);
        _defaultPosition = transform.position;
        _defaultRotation = transform.rotation;
    }

    private void Update()
    {

        if (_isRotating)
        {
            UpdateRotation();
        }
        else if (_isTraveling)
        {
            Travel(_currentArmThrow.initialPosition, _currentArmThrow.target);
        }

        if (_isVulnerable)
        {
            if (_lifeBar.IsLifeDepleted(transform))
            {
                Destroyed();
            }
        }
        else
        {
            _lifeBar.Hide();
        }
    }

    private void LateUpdate()
    {
        if (_isDestroyed)
        {
            transform.position = _deathPosition;
            _smokeObject.transform.position = _deathPosition;
        }
            
    }

    public void Shoot(Vector3 position)
    {
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (_isDestroyed || _isTraveling || _isVulnerable || _isReturning || transform.position == position)
            return;

        _isReturning = false;
        _isTraveling = false;
        _isReady = false;
        _isShooting = true;
        _currentArmThrow.initialRotation = transform.rotation;
        _currentArmThrow.returnPosition = transform.position;
        SetCollidersActive(true);
        SetCurrentArmTransition(position);
        _bossSounds.PlayArmSound();
        _isRotating = true;
    }

    public void ReturnArm()
    {
        if (_isDestroyed || transform.position == _defaultPosition)
            return;

        _isShooting = false;
        _isReturning = true;
        _currentArmThrow.throwStartTime = Time.time;
        SetCollidersActive(false);
        SetCurrentArmTransition(_currentArmThrow.returnPosition);
        _isTraveling = true;
    }

    private void SetCollidersActive(bool isActive)
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            _colliders[i].enabled = isActive;
        }
    }

    private void SetCurrentArmTransition(Vector3 targetPosition)
    {
        _currentArmThrowSpeed = _baseArmThrowSpeed;
        _currentArmThrow.throwDistance = Vector3.Distance(transform.position, targetPosition);
        _currentArmThrow.target = targetPosition;
        _currentArmThrow.initialPosition = transform.position;
    }

    private void Travel(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to) - Vector3.Distance(_fist.position, to);
        float distanceCovered = (Time.time - _currentArmThrow.throwStartTime) * _currentArmThrowSpeed;
        float fractionOfDistance = distanceCovered / _currentArmThrow.throwDistance;
        if (_isShooting)
        {
            transform.position = Vector3.Lerp(from, to - (_fist.position - transform.position), fractionOfDistance);
        }
        else if (_isReturning)
        {
            transform.position = Vector3.Lerp(from, _currentArmThrow.returnPosition, fractionOfDistance);
        }

        if (_currentArmThrowSpeed <= _maxArmThrowSpeed)
        {
            _currentArmThrowSpeed += Time.deltaTime * 5f;
        }

        Vector3 position = transform.position;
        if (_isShooting)
        {
            position = _fist.position;
        }

        if ((to - position).sqrMagnitude < 0.5f * 0.5f)
        {
            if (_isShooting)
            {
                HitWall();
            }
            if (_isReturning)
            {
                _bossSounds.PlayArmSound();
                _isRotating = true;
            }

            _isTraveling = false;
            _isShooting = false;
        }
    }

    private void UpdateRotation()
    {
        Quaternion rotBeforeUpdate = transform.rotation;
        Quaternion lookRotation = transform.rotation;
        Vector3 direction;
        if (_isReturning)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _currentArmThrow.initialRotation, (1 - Mathf.Exp(-ArmRotationSpeed * Time.deltaTime)));
        }
        else
        {
            direction = (_currentArmThrow.target - transform.position).normalized;
            lookRotation = XLookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, (1 - Mathf.Exp(-ArmRotationSpeed * Time.deltaTime)));
        }

        if (rotBeforeUpdate == transform.rotation)
        {
            _isRotating = false;
            if (_isShooting)
            {
                _currentArmThrow.throwStartTime = Time.time;
                _isTraveling = true;
            }

            if (_isReturning)
            {
                _isReturning = false;
                _isReady = true;
            }
        }
    }

    private Quaternion XLookRotation(Vector3 direction)
    {

        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        if (isRightArm)
        {
            rightToForward = Quaternion.Euler(0f, 90f, 0f);
        }
        Quaternion forwardToTarget = Quaternion.LookRotation(direction, transform.up);

        return forwardToTarget * rightToForward;
    }

    private IEnumerator Vulnerability()
    {
        _isVulnerable = true;
        yield return new WaitForSeconds(VulnerabilityTime);
        _isVulnerable = false;
        ReturnArm();
    }

    private void TakeAHit(float damage)
    {
        _lifeBar.TakeDamage(damage);
    }

    private void Destroyed()
    {
        SetCollidersActive(false);
        _deathPosition = transform.position;
        _isTraveling = false;
        _isVulnerable = false;
        _isShooting = false;
        _isReturning = false;
        if (!_isDestroyed)
        {
            _smokeObject.transform.position = transform.position;
            _explosionObject.transform.position = transform.position;
            _smokeObject.SetActive(true);
            _explosionObject.SetActive(true);
            _smokeSystem.Play();
            _explosionSystem.Play();
            _bossSounds.PlayArmExplosion();
            _isDestroyed = true;
        }
    }

    private void QuakeSurroundings(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, 10);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && rb.CompareTag("Player"))
            {
                rb.AddExplosionForce(3000, position, 20, 0f);
                rb.GetComponent<Player>().TakeAHitFromBoss(BossDamageOnPlayer);
            }
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (_isVulnerable || !collision.gameObject.CompareTag("Player") || _isDestroyed || _isReturning)
            return;

        Player playerHit = collision.gameObject.GetComponent<Player>();
        if (_lastPlayerHit != playerHit)
        {
            _lastPlayerHit = collision.gameObject.GetComponent<Player>();
            _lastPlayerHit.TakeAHitFromBoss(35);
            StartCoroutine(ResetLastPlayerHit());
        }
    }*/

    private void OnTriggerEnter(Collider collider)
    {
        if (!_isVulnerable || !collider.gameObject.CompareTag("Weapon"))
            return;

        int indexOfPlayer = _players.IndexOf(collider.transform.parent.GetComponent<Player>()); ;
        if (indexOfPlayer == -1 || !_canBeHit[indexOfPlayer])
            return;

        TakeAHit(HitDamage);
        _players[indexOfPlayer].TouchAHitOnBoss(PlayerDamageOnBoss);
        _players[indexOfPlayer].MeleeAttack.PlayAttackEffect();
        StartCoroutine(TakeHitCooldown(indexOfPlayer));
    }

    public bool IsDestroyed()
    {
        return _isDestroyed;
    }

    private IEnumerator TakeHitCooldown(int index)
    {
        _canBeHit[index] = false;
        yield return new WaitForSeconds(HitCooldown);
        _canBeHit[index] = true;
    }

    private IEnumerator ResetLastPlayerHit()
    {
        yield return new WaitForSeconds(1f);
        _lastPlayerHit = null;
    }

    private void HitWall()
    {
        StartCoroutine(StaticObjects.CameraController.CameraShake(0.2f, 0.75f));
        QuakeSurroundings(_currentArmThrow.target);
        _shockwaveObject.transform.position = _currentArmThrow.target;
        _shockwaveObject.SetActive(true);
        _bossSounds.PlayImpact();
        if (!_shockwaveSystem.isPlaying)
        {
            _shockwaveSystem.Play();
        }
        StartCoroutine(Vulnerability());
    }

    public bool IsReady()
    {
        return _isReady;
    }

    public void SetBossSounds(BossSounds bossSounds)
    {
        _bossSounds = bossSounds;
    }
}
