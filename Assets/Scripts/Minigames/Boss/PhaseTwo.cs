using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

struct Movement
{
    public Vector3 moveTargetPos;
    public float moveStartTime;
    public float moveDistance;
    public Vector3 moveInitialPos;
    public float moveSpeed;
}

public class PhaseTwo : MonoBehaviour
{
    private const float PhaseTwoCycleLength = 19f;
    private const float TimeBetweenMoves = 1f;
    private const float RowTransitionSpeed = 10f;
    private const float BodySlamSpeed = 25f;
    private const float FallToGroundSpeed = 12f;
    private const float ReturnSpeed = 25f;
    private const float HitPlayerKnockbackForce = 50f;
    private const float VulnerabilityTime = 5f;
    private const float HitDamage = 1f;
    private const float Maxlife2Players = 12f;
    private const float Maxlife4Players = 24f;
    private const float HitCooldown = 0.7f;
    private const float HitPlayerCooldown = 1f;
    private const float PlayerDamageOnBoss = 150;
    private const float BossDamageOnPlayer = 10f;
    private const int TopRowIndex = 0;
    private const int MiddleRowIndex = 1;
    private const int BottomRowIndex = 2;
    private bool _phaseTwoReady;
    private bool _isBodyDestroyed;
    private bool _isMovingBetweenRows;
    private bool _isSlamming;
    private bool _isMoving;
    private bool _isReturning;
    private bool _isFallingToGround;
    private bool _phaseTwoCycleReady;
    private bool _isVulnerable;
    private bool _isReturningToOriginalPos;
    private bool[] _canBeHit;
    private bool _isLifeDepleted;
    private bool _knockbackActive;
    private int _currentRow;
    private Camera _mainCamera;
    private Vector3 _transitionInitialPos;
    private Transform _leftWallPos;
    private Transform _rightWallPos;
    private Transform _floorPos;
    private Transform _ceilingWallPos;
    private List<Player> _players;
    private List<Vector3> _phaseTwoRows;
    private Movement _currentMovement;
    private LifeBar _lifeBar;
    private float oneCycleTotalTime;
    private BossSounds _bossSounds;
    [Header("Components")]
    [SerializeField] private List<Collider> _bodyColliders;
    [SerializeField] private Collider _bodyTrigger;
    [SerializeField] private Transform _phaseTwoTopPos;
    [SerializeField] private Transform _phaseTwoMiddlePos;
    [SerializeField] private Transform _phaseTwoBottomPos;
    [SerializeField] private Slider _lifeBarSlider;

    [Header("Important map positions")]
    [SerializeField] private Transform _vulnerableStatePosition;
    [SerializeField] private Transform _defaultPosition;

    //Particle Effects
    private ParticleSystem _smoke;
    private ParticleSystem _fuelLeak1;
    private ParticleSystem _fuelLeak2;
    private ParticleSystem _sparks;
    private ParticleSystem _shockWave;
    private ParticleSystem _explosion;

    [Header("Particle system gameobjects")]
    [SerializeField] private GameObject _smokeObj;
    [SerializeField] private GameObject _fuelLeak1Obj;
    [SerializeField] private GameObject _fuelLeak2Obj;
    [SerializeField] private GameObject _sparksObj;
    [SerializeField] private GameObject _explosionObj;
    [SerializeField] private GameObject _shockWaveObj;

    private void Start()
    {
        //Quand le boss va être intégré à la scène avec la camera
        //_mainCamera = StaticObjects.CameraController.Camera;
        _mainCamera = Camera.main;
        _isMovingBetweenRows = false;
        _isMoving = false;
        _isSlamming = false;
        _isReturning = false;
        _isReturningToOriginalPos = false;
        _isVulnerable = false;
        _isLifeDepleted = false;
        _bodyTrigger.enabled = false;
        _knockbackActive = true;
        _smokeObj.SetActive(false);
        _fuelLeak1Obj.SetActive(false);
        _fuelLeak2Obj.SetActive(false);
        _sparksObj.SetActive(false);
        _shockWaveObj.SetActive(false);
        _explosionObj.SetActive(false);
        _explosion = _explosionObj.GetComponent<ParticleSystem>();
        _smoke = _smokeObj.GetComponent<ParticleSystem>();
        _fuelLeak1 = _fuelLeak1Obj.GetComponent<ParticleSystem>();
        _fuelLeak2 = _fuelLeak2Obj.GetComponent<ParticleSystem>();
        _sparks = _sparksObj.GetComponent<ParticleSystem>();
        _shockWave = _shockWaveObj.GetComponent<ParticleSystem>();
        _phaseTwoRows = new List<Vector3>();
        _phaseTwoRows.Add(_phaseTwoTopPos.position);
        _phaseTwoRows.Add(_phaseTwoMiddlePos.position);
        _phaseTwoRows.Add(_phaseTwoBottomPos.position);
        _players = StaticObjects.GameController.GetPlayers();
        _canBeHit = Enumerable.Repeat(true, _players.Count).ToArray();
        float maxLife = _players.Count <= 2 ? Maxlife2Players : Maxlife4Players;
        _lifeBar = new LifeBar(maxLife, _mainCamera, _lifeBarSlider, 0f, 5f, -2f);
        _lifeBar.Hide();
    }

    public void Update()
    {
        PhaseTwoCycle();

        if (_isMoving)
        {
            DoMovement(_currentMovement.moveInitialPos, _currentMovement.moveTargetPos);
        }

        if (_isVulnerable)
        {
            if (_lifeBar.IsLifeDepleted(transform) && !_isLifeDepleted)
            {
                LifeDepleted();
            }
        }
        else
        {
            _lifeBar.Hide();
        }

        if (!_isBodyDestroyed)
        {
            UpdateParticles();
        }
    }

    public void InitPhase(Transform leftWallPos,
        Transform rightWallPos,
        Transform floorPos,
        Transform ceilingPos,
        BossSounds bossSounds)
    {
        _leftWallPos = leftWallPos;
        _rightWallPos = rightWallPos;
        _floorPos = floorPos;
        _ceilingWallPos = ceilingPos;
        _bossSounds = bossSounds;
    }

    private void DoMovement(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        float distanceCovered = (Time.time - _currentMovement.moveStartTime) * _currentMovement.moveSpeed;
        float fractionOfDistance = distanceCovered / distance;
        if(from != to)
            transform.position = Vector3.Lerp(from, to, fractionOfDistance);

        if (transform.position == to)
        {
            if (_isMovingBetweenRows)
            {
                _isMovingBetweenRows = false;
                StartCoroutine(PrepareToCharge());
            }
            else if (_isSlamming)
            {
                _isSlamming = false;
                ToggleColliders(false);
                QuakeSurroundings(to);
                StartCoroutine(PrepareToReturn());
            }
            else if (_isReturning)
            {
                _isReturning = false;
                ToggleColliders(true);
                _bossSounds.PlayShortCircuit();
                StartCoroutine(PrepareToBecomeVulnerable());
            } else if (_isFallingToGround)
            {
                _isFallingToGround = false;
                StartCoroutine(Vulnerability());
            } else if (_isReturningToOriginalPos)
            {
                _isReturningToOriginalPos = false;
                ToggleColliders(true);
                if (_isLifeDepleted)
                    StartCoroutine(Destroyed());
            }
            _isMoving = false;
        }
    }

    private IEnumerator Destroyed()
    {
        yield return new WaitForSeconds(1f);
        _knockbackActive = false;
        _isBodyDestroyed = true;
    }

    private void LifeDepleted()
    {
        _isLifeDepleted = true;
        _smokeObj.SetActive(true);
        _smoke.Play();
        _explosionObj.SetActive(true);
        _bossSounds.StopKeyboard();
        _bossSounds.PlayPhase2Dead();
        StaticObjects.SpeakerLinesManager.PlayBossLaserPhase();
        if (!_explosion.isPlaying)
            _explosion.Play();
        StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator PrepareToCharge()
    {
        yield return new WaitForSeconds(TimeBetweenMoves);
        Charge();
    }

    private IEnumerator PrepareToReturn()
    {
        yield return new WaitForSeconds(TimeBetweenMoves);
        ReturnAfterCharge();
    }

    private IEnumerator PrepareToBecomeVulnerable()
    {
        yield return new WaitForSeconds(0);
        MoveToVulnerabilityPosition();
    }

    private IEnumerator Vulnerability()
    {
        StaticObjects.SpeakerLinesManager.PlayBossPanicLine();
        _bossSounds.PlayKeyboard();
        _isVulnerable = true;
        _bodyTrigger.enabled = true;
        yield return new WaitForSeconds(VulnerabilityTime);
        _bossSounds.StopKeyboard();
        _isVulnerable = false;
        _bodyTrigger.enabled = false;
        ToggleColliders(false);
        StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        yield return new WaitForSeconds(2f);
        SetCurrentMovement(_defaultPosition.position);
        _currentMovement.moveSpeed = RowTransitionSpeed;
        _isReturningToOriginalPos = true;
        _isMoving = true;
    }

    private void MoveToVulnerabilityPosition()
    {
        SetCurrentMovement(_vulnerableStatePosition.position);
        _isFallingToGround = true;
        _currentMovement.moveSpeed = FallToGroundSpeed;
        _isMoving = true;
    }

    private void DoBodySlam(int row)
    {
        oneCycleTotalTime = Time.time;
        ToggleColliders(true);
        TransitionToRow(row);
    }

    private void ReturnAfterCharge()
    {
        SetCurrentMovement(new Vector2(_vulnerableStatePosition.position.x, _phaseTwoRows[_currentRow].y));
        ToggleColliders(false);
        _currentMovement.moveSpeed = ReturnSpeed;
        _isMoving = true;
        _isReturning = true;
    }

    private void Charge()
    {
        SetCurrentMovement(new Vector3(_leftWallPos.position.x, _phaseTwoRows[_currentRow].y, 0));
        _currentMovement.moveSpeed = BodySlamSpeed;
        _isMoving = true;
        _isSlamming = true;
    }

    private void TransitionToRow(int row)
    {
        _currentRow = row;
        SetCurrentMovement(_phaseTwoRows[row]);
        _currentMovement.moveSpeed = RowTransitionSpeed;
        _isMovingBetweenRows = true;
        _isMoving = true;
        _bossSounds.PlaySlamSound();
    }

    private void SetCurrentMovement(Vector2 targetPosition)
    {
        _currentMovement.moveInitialPos = transform.position;
        _currentMovement.moveStartTime = Time.time;
        _currentMovement.moveTargetPos = targetPosition;
        _currentMovement.moveDistance = Vector3.Distance(_currentMovement.moveTargetPos, _currentMovement.moveInitialPos);
    }

    private void ToggleColliders(bool enabled)
    {
        for (int i = 0; i < _bodyColliders.Count; i++)
        {
            _bodyColliders[i].enabled = enabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _isVulnerable || _isFallingToGround || !_knockbackActive)
            return;

        Vector2 direction = (_currentMovement.moveTargetPos - transform.position).normalized;
        //collision.rigidbody.AddForce(new Vector2(HitPlayerKnockbackForce * direction.x, (HitPlayerKnockbackForce / 4)), ForceMode.Impulse);
    }

    private void QuakeSurroundings(Vector3 position)
    {
        _shockWaveObj.transform.position = _currentMovement.moveTargetPos;
        _shockWaveObj.SetActive(true);
        _shockWave.Play();
        _bossSounds.StopSlamSound();
        _bossSounds.PlayImpact();
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, 15);
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

    private void OnTriggerEnter(Collider collider)
    {
        if (!_isVulnerable || !collider.gameObject.CompareTag("Weapon"))
            return;

        int indexOfPlayer = _players.IndexOf(collider.transform.parent.GetComponent<Player>());
        if (indexOfPlayer == -1 || !_canBeHit[indexOfPlayer])
            return;

        TakeAHit(HitDamage);
        _players[indexOfPlayer].TouchAHitOnBoss(PlayerDamageOnBoss);
        _players[indexOfPlayer].MeleeAttack.PlayAttackEffect();
        StartCoroutine(TakeHitCooldown(indexOfPlayer));
    }

    private void TakeAHit(float damage)
    {
        _lifeBar.TakeDamage(damage);
    }

    private IEnumerator TakeHitCooldown(int index)
    {
        _canBeHit[index] = false;
        yield return new WaitForSeconds(HitCooldown);
        _canBeHit[index] = true;
    }

    public void SetPhaseTwoReady(bool isPhaseReady)
    {
        _phaseTwoReady = isPhaseReady;
    }

    private void PhaseTwoCycle()
    {
        if (_phaseTwoReady)
        {
            DoBodySlam(GetRowWithPlayer());
            StartCoroutine(PhaseTwoCycleCooldown());
        }
    }

    public bool IsBodyDead()
    {
        return _isBodyDestroyed;
    }

    private IEnumerator PhaseTwoCycleCooldown()
    {
        _phaseTwoReady = false;
        yield return new WaitForSeconds(PhaseTwoCycleLength);
        _phaseTwoReady = true;
    }

    private int GetRowWithPlayer()
    {
        List<int> rowsWithPlayers = new List<int>();
        float topRowLimit = (_phaseTwoRows[TopRowIndex].y + _phaseTwoRows[MiddleRowIndex].y) / 2;
        float middleRowLimit = (_phaseTwoRows[MiddleRowIndex].y + _phaseTwoRows[BottomRowIndex].y) / 2;
        for (int i = 0; i < _players.Count; i++)
        {
            float playerY = _players[i].transform.position.y;
            if (playerY >= topRowLimit)
            {
                if (!rowsWithPlayers.Contains(TopRowIndex))
                    rowsWithPlayers.Add(TopRowIndex);
            } else if (playerY >= middleRowLimit)
            {
                if (!rowsWithPlayers.Contains(MiddleRowIndex))
                    rowsWithPlayers.Add(MiddleRowIndex);
            } else if (playerY >= _floorPos.position.y)
            {
                if (!rowsWithPlayers.Contains(BottomRowIndex))
                    rowsWithPlayers.Add(BottomRowIndex);
            }
        }
        return rowsWithPlayers[Random.Range(0, rowsWithPlayers.Count)];
    }

    private void UpdateParticles()
    {
        float currentLife = _lifeBar.GetCurrentLife();
        float maxLife = _lifeBar.GetMaxLife();
        if (currentLife < maxLife && currentLife < maxLife * 0.75f && !_fuelLeak1.isPlaying)
        {
            _fuelLeak1Obj.SetActive(true);
            _fuelLeak1.Play();
        }

        if (currentLife < maxLife && currentLife < maxLife * 0.50f && !_sparks.isPlaying)
        {
            _sparksObj.SetActive(true);
            _sparks.Play();
        }

        if (currentLife < maxLife && currentLife < maxLife * 0.25f && !_fuelLeak2.isPlaying)
        {
            _fuelLeak2Obj.SetActive(true);
            _fuelLeak2.Play();
        }
    }
}
