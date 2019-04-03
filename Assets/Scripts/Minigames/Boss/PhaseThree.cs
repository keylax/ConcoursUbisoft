using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PhaseThree : MonoBehaviour
{
    private const float _shotCooldown = 1f;
    private const int NbrOfShotsPerCycle = 4;
    private const float MoveToVulnPosSpeed = 12f;
    private const float Maxlife2Players = 12f;
    private const float Maxlife4Players = 24f;
    private int _nbrOfShots;
    private bool _phaseThreeReady;
    private bool _cycleReady;
    private bool _isMoving;
    private bool _isGoingToVulnerabilityPos;
    private bool _isReturning;
    private bool _isVulnerable;
    private bool _isLifeDepleted;
    private bool _isReactorDestroyed;
    private bool _isShootingMovingTarget;
    private bool _canBeHit;
    private LifeBar _lifeBar;
    private List<Player> _players;
    private Movement _currentMovement;
    private Camera _mainCamera;
    private ParticleSystem _explosionSystem;
    private BossSounds _bossSounds;

    [Header("Components")]
    [SerializeField] private Reactor _reactor;
    [SerializeField] private Collider _reactorTrigger;
    [SerializeField] private Slider _lifeBarSlider;

    [Header("Important map positions")]
    [SerializeField] private Transform _vulnerableStatePosition;
    [SerializeField] private Transform _defaultPosition;

    [Header("Particle effect gameobjects")]
    [SerializeField] private GameObject _explosionObject;

    private void Start()
    {
        _mainCamera = StaticObjects.CameraController.Camera;
        _nbrOfShots = 0;
        _phaseThreeReady = false;
        _cycleReady = false;
        _isMoving = false;
        _isGoingToVulnerabilityPos = false;
        _isReturning = false;
        _isVulnerable = false;
        _isLifeDepleted = false;
        _isReactorDestroyed = false;
        _players = StaticObjects.GameController.GetPlayers();
        float maxLife = _players.Count <= 2 ? Maxlife2Players : Maxlife4Players;
        _lifeBar = new LifeBar(maxLife, _mainCamera, _lifeBarSlider, -2.5f, 5f, -4f);
        _lifeBar.Hide();
        _explosionSystem = _explosionObject.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        PhaseThreeCycle();
        if (_isMoving)
        {
            DoMovement(_currentMovement.moveInitialPos, _currentMovement.moveTargetPos);
        }

        if (_isShootingMovingTarget)
        {
            if (!_reactor.IsShootingMovingTarget())
            {
                _isShootingMovingTarget = false;
                StartCoroutine(PrepareToBecomeVulnerable());
            }
        }

        if (_isVulnerable)
        {
            if (_lifeBar.IsLifeDepleted(_reactor.transform) && !_isLifeDepleted)
            {
                LifeDepleted();
            }
        }
        else
        {
            _lifeBar.Hide();
        }
    }

    private void LifeDepleted()
    {
        _isLifeDepleted = true;
        _reactorTrigger.enabled = false;
        _explosionObject.SetActive(true);
        _explosionSystem.Play();
        _bossSounds.StopKeyboard();
        _bossSounds.PlayDeathSound();
        StartCoroutine(Destroyed());
    }

    public void InitPhase(Transform leftWallPos,
        Transform rightWallPos,
        Transform floorPos,
        Transform ceilingPos,
        BossSounds bossSounds)
    {
        _reactor.InitReactor(leftWallPos, rightWallPos, floorPos, ceilingPos, bossSounds);
        _bossSounds = bossSounds;
    }

    public void SetPhaseThreeReady(bool isPhaseReady)
    {
        _phaseThreeReady = isPhaseReady;
        _cycleReady = true;
    }

    public bool IsReactorDestroyed()
    {
        return _isReactorDestroyed;
    }

    private IEnumerator PrepareToBecomeVulnerable()
    {
        _bossSounds.PlayShortCircuit();
        yield return new WaitForSeconds(1f);
        MoveToVulnerabilityPosition();
    }

    private void MoveToVulnerabilityPosition()
    {
        SetCurrentMovement(_vulnerableStatePosition.position);
        _isGoingToVulnerabilityPos = true;
        _currentMovement.moveSpeed = MoveToVulnPosSpeed;
        _isMoving = true;
    }

    private void SetCurrentMovement(Vector2 targetPosition)
    {
        _currentMovement.moveInitialPos = transform.position;
        _currentMovement.moveStartTime = Time.time;
        _currentMovement.moveTargetPos = targetPosition;
        _currentMovement.moveDistance = Vector3.Distance(_currentMovement.moveTargetPos, _currentMovement.moveInitialPos);
    }

    private void DoMovement(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        float distanceCovered = (Time.time - _currentMovement.moveStartTime) * _currentMovement.moveSpeed;
        float fractionOfDistance = distanceCovered / distance;
        if (from != to)
            transform.position = Vector3.Lerp(from, to, fractionOfDistance);

        if (transform.position == to)
        {
            if (_isGoingToVulnerabilityPos)
            {
                _isGoingToVulnerabilityPos = false;
                StartCoroutine(Vulnerability());
            }
            else if (_isReturning)
            {
                StartCoroutine(CycleReady());
                _isReturning = false;
            }
            _isMoving = false;
        }
    }

    public void TakeAHit(float damage)
    {
        _lifeBar.TakeDamage(damage);
    }

    private IEnumerator Destroyed()
    {
        yield return new WaitForSeconds(1f);
        _isReactorDestroyed = true;
    }

    private IEnumerator CycleReady()
    {
        yield return new WaitForSeconds(1f);
        _cycleReady = true;
    }

    private void PhaseThreeCycle()
    {
        if (_phaseThreeReady && _cycleReady)
        {
            _cycleReady = false;
            StartCoroutine(ShootingCycle());
        }
    }

    private IEnumerator Vulnerability()
    {
        StaticObjects.SpeakerLinesManager.PlayBossPanicLine();
        _bossSounds.PlayKeyboard();
        _isVulnerable = true;
        _reactorTrigger.enabled = true;
        _reactor.SetVulnerable(true);
        yield return new WaitForSeconds(5f);
        _bossSounds.StopKeyboard();
        _isVulnerable = false;
        _reactorTrigger.enabled = false;
        _reactor.SetVulnerable(false);

        if (!_isLifeDepleted)
            StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        yield return new WaitForSeconds(1f);
        SetCurrentMovement(_defaultPosition.position);
        _currentMovement.moveSpeed = MoveToVulnPosSpeed;
        _isReturning = true;
        _isMoving = true;
    }

    private IEnumerator ShootingCycle()
    {
        for (int i = 0; i < NbrOfShotsPerCycle; i++)
        {
            _reactor.ShootLaser(_players[_nbrOfShots % _players.Count].transform);
            _nbrOfShots++;
            yield return new WaitForSeconds(3f);
        }
        _isShootingMovingTarget = true;
        _reactor.ShootMovingTarget();
    }

    private IEnumerator ReturnToOriginalPos()
    {
        yield return new WaitForSeconds(1f);
        _isReturning = true;
        _isMoving = true;
    }
}
