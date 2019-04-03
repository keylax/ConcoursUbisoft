using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Reactor : MonoBehaviour
{
    private const float AimingLightSaberFactor = 1f;
    private const float LaserShotKnockBack = 50f;
    private const float MovingLaserKockback = 10f;
    private const float AimingWidth = 0.5f;
    private const float ShootingLightSaberFactor = 0.85f;
    private const float ShootingWidth = 1f;
    private const float AimingTime = 1.5f;
    private const float HitCooldown = 0.7f;
    private const float HitDamage = 1f;
    private const float LaserShotDamage = 10;
    private const float MovingLaserDamage = 1f;
    private const float PlayerDamageOnBoss = 150;
    private bool[] _canBeHit;
    private Plane _floorPlane;
    private Plane _ceilingPlane;
    private Plane _leftWallPlane;
    private Plane _rightWallPlane;
    private Transform _leftWallPos;
    private Transform _rightWallPos;
    private Transform _floorPos;
    private Transform _ceilingWallPos;
    private Transform _currentTarget;
    private Vector3 _shootTarget;
    private bool _isAiming;
    private bool _isReactorDead;
    private bool _isVulnerable;
    private bool _isShootingMovingTarget;
    private Material _laserMaterial;
    private List<Player> _players;
    private BossSounds _bossSounds;

    [Header("Scripts")]
    [SerializeField] private MovingLaserTarget _movingTarget;
    [SerializeField] private VolumetricLineBehavior _laser;
    private PhaseThree _phaseThree;

    private void Start()
    {
        _players = StaticObjects.GameController.GetPlayers();
        _canBeHit = Enumerable.Repeat(true, _players.Count).ToArray();
        _phaseThree = GetComponentInParent<PhaseThree>();
        _laserMaterial = _laser.gameObject.GetComponent<Renderer>().material;
    }

    private void Update()
    {
        _laser.transform.localPosition = Vector3.zero; ;
        _laser.StartPos = transform.position - transform.parent.parent.transform.position;
        if (_isAiming)
        {
            _laser.EndPos = _currentTarget.position - transform.parent.parent.transform.position;
        } else if (_isShootingMovingTarget)
        {
            if (_movingTarget.IsCycleComplete())
            {
                _bossSounds.StopMovingLaserSound();
                _isShootingMovingTarget = false;
                _laser.gameObject.SetActive(false);
            }
                
            _laser.EndPos = _movingTarget.transform.position - transform.parent.parent.transform.position;
            Vector2 rayDirection = _movingTarget.transform.position - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, LayerMask.GetMask("Units")))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    hit.rigidbody.AddForce(rayDirection.normalized * MovingLaserKockback, ForceMode.Impulse);
                    hit.transform.GetComponent<Player>().TakeAHitFromBoss(MovingLaserDamage);
                }
            }
        }
    }

    public void InitReactor(Transform leftWallPos,
        Transform rightWallPos,
        Transform floorPos,
        Transform ceilingPos,
        BossSounds bossSounds)
    {
        _leftWallPos = leftWallPos;
        _rightWallPos = rightWallPos;
        _floorPos = floorPos;
        _ceilingWallPos = ceilingPos;
        _floorPlane = new Plane(floorPos.up, floorPos.position);
        _ceilingPlane = new Plane(-ceilingPos.up, ceilingPos.position);
        _leftWallPlane = new Plane(leftWallPos.right, leftWallPos.position);
        _rightWallPlane = new Plane(-rightWallPos.right, rightWallPos.position);
        _bossSounds = bossSounds;
    }

    public void ShootLaser(Transform target)
    {
        _currentTarget = target;
        StartCoroutine(Aim());
    }

    public IEnumerator Aim()
    {
        _laserMaterial.SetFloat("_LightSaberFactor", AimingLightSaberFactor);
        _laserMaterial.SetFloat("_LineWidth", AimingWidth);
        _laser.EndPos = _currentTarget.position - transform.parent.parent.transform.position;
        _isAiming = true;
        _laser.gameObject.SetActive(true);

        yield return new WaitForSeconds(AimingTime);

        _isAiming = false;
        _laser.gameObject.SetActive(false);
        try
        {
            _shootTarget = ComputeShootTargetPosition(_currentTarget.position);
        }
        catch (UnityException exception)
        {
            Debug.Log("Something went wrong, not shooting the laser.");
        }
        StartCoroutine(PrepareToShoot(_shootTarget));
    }

    public void ShootMovingTarget()
    {
        _laserMaterial.SetFloat("_LineWidth", ShootingWidth);
        _laserMaterial.SetFloat("_LightSaberFactor", ShootingLightSaberFactor);
        _movingTarget.Move(transform.position.x);
        _bossSounds.PlayMovingLaser();
        _isShootingMovingTarget = true;
        _laser.EndPos = _movingTarget.transform.position - transform.parent.parent.transform.position;
        _laser.gameObject.SetActive(true);
    }

    public bool IsShootingMovingTarget()
    {
        return _isShootingMovingTarget;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!_isVulnerable || !collider.gameObject.CompareTag("Weapon"))
            return;

        int indexOfPlayer = _players.IndexOf(collider.transform.parent.GetComponent<Player>());
        if (indexOfPlayer == -1 || !_canBeHit[indexOfPlayer])
            return;

        _phaseThree.TakeAHit(HitDamage);
        _players[indexOfPlayer].TouchAHitOnBoss(PlayerDamageOnBoss);
        _players[indexOfPlayer].MeleeAttack.PlayAttackEffect();
        StartCoroutine(TakeHitCooldown(indexOfPlayer));
    }

    private IEnumerator TakeHitCooldown(int index)
    {
        _canBeHit[index] = false;
        yield return new WaitForSeconds(HitCooldown);
        _canBeHit[index] = true;
    }

    public void SetVulnerable(bool isVulnerable)
    {
        _isVulnerable = isVulnerable;
    }

    public IEnumerator PrepareToShoot(Vector3 target)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(Shoot(target));
    }

    public IEnumerator Shoot(Vector3 target)
    {
        _laserMaterial.SetFloat("_LineWidth", ShootingWidth);
        _laserMaterial.SetFloat("_LightSaberFactor", ShootingLightSaberFactor);
        _laser.SetStartAndEndPoints(transform.position - transform.parent.parent.transform.position, target - transform.parent.parent.transform.position);
        _laser.EndPos = target - transform.parent.parent.transform.position;
        _laser.gameObject.SetActive(true);
        _bossSounds.PlayLaserSound();
        Vector2 rayDirection = _shootTarget - transform.position;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 1f, rayDirection, out hit, Mathf.Infinity, LayerMask.GetMask("Units")))
        {
            if (hit.transform.CompareTag("Player"))
            {
                hit.rigidbody.AddForce(rayDirection.normalized * LaserShotKnockBack, ForceMode.Impulse);
                hit.transform.GetComponent<Player>().TakeAHitFromBoss(LaserShotDamage);
            }
        }

        yield return new WaitForSeconds(0.1f);

        _laser.gameObject.SetActive(false);
    }

    private Vector3 ComputeShootTargetPosition(Vector3 playerPosition)
    {
        Vector3 shootTarget = Vector3.zero;
        Vector3 direction = (playerPosition - transform.position).normalized;
        Ray ray = new Ray(transform.position, direction);
        float enter = 0.0f;

        if (_floorPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.x >= _leftWallPos.position.x -1 && hitPoint.x <= _rightWallPos.position.x + 1)
            {
                return new Vector2(hitPoint.x, hitPoint.y - 1);
            }
        }

        if (_ceilingPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.x >= _leftWallPos.position.x + 1 && hitPoint.x <= _rightWallPos.position.x + 1)
            {
                return hitPoint;
            }
        }

        if (_leftWallPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.y >= _floorPos.position.y -1 && hitPoint.y <= _ceilingWallPos.position.y + 1)
            {
                return hitPoint;
            }
        }

        if (_rightWallPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.y >= _floorPos.position.y + 1 && hitPoint.y <= _ceilingWallPos.position.y + 1)
            {
                return hitPoint;
            }
        }
        throw new UnityException();
    }

    public bool IsDestroyed()
    {
        return _isReactorDead;
    }
}
