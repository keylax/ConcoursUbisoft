using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PongBall : MonoBehaviour
{
    private const float HammerSpeedModifier = 2.5f;
    private const float BallHitPlayerSpeedModifier = 0.75f;
    [SerializeField] private float PlayerHitBallSpeedModifier = 1.1f;
    [SerializeField] private float WeaponSpeedModifier = 1.1f;
    [SerializeField] private float maxSpeed = 15;
    private const float BaseSpeed = 8.5f;
    [SerializeField] private float _minimumHorizontal = 0.5f;

    private Vector2 _direction;
    private bool _active;
    private Player _topLeftPlayer;
    private UnitMovement _topLeftPlayerMovement;
    private Player _topRightPlayer;
    private UnitMovement _topRightPlayerMovement;
    private Player _bottomLeftPlayer;
    private UnitMovement _bottomLeftPlayerMovement;
    private Player _bottomRightPlayer;
    private UnitMovement _bottomRightPlayerMovement;
    private Transform _leftLimit;
    private Transform _rightLimit;
    private Transform _topLimit;
    private Transform _bottomLimit;
    private MinigamePong _mgPong;
    private PongBallRotate _meshRotation;
    private float _currentSpeed;
    private float _targetSpeed;
    private Vector2 _resetPosition;
    private Vector3 _basePosition;
    private Player _lastHit;
    private float _timeLastHit;
    private List<Player> _players;
    [SerializeField] GameObject _mesh;
    [SerializeField] private SphereCollider _collider;
    [SerializeField] private bool _areBodyHitsActive;

    private void Update()
    {
        if (!_active)
            return;
        AddSpeed();
        _meshRotation.UpdateRotation();
        // Debug.Log("Current speed = " + _currentSpeed + " | target speed = " + _targetSpeed);
        float actualSpeed = _currentSpeed;
        if (Math.Abs(_currentSpeed - _targetSpeed) > 0.2)
        {
            if (_currentSpeed > _targetSpeed)
                _currentSpeed -= 0.2f;
            else
                _currentSpeed += 0.2f;
        }
        transform.Translate(_direction * actualSpeed * Time.deltaTime);
        Player tempLastHit = _lastHit;
        
        if (transform.position.y <= _bottomLimit.position.y + _collider.radius && _direction.y < 0)
        {
            ReflectDirection(_bottomLimit.up);
        }
        else if (transform.position.y >= _topLimit.position.y - _collider.radius && _direction.y > 0)
        {
            ReflectDirection(-_topLimit.up);
        }
        if (transform.position.x <= _leftLimit.position.x + _collider.radius)
        {
            if (transform.position.y >= _mgPong.horizontalSeparation.transform.position.y)
            {
                if (_topLeftPlayer && _lastHit && !_topLeftPlayer.Equals(_lastHit))
                    _mgPong.IncrementScore(_lastHit);
                if (tempLastHit == null || _players.Count > 2)
                    _mgPong.IncrementAllScoreExcept(_topLeftPlayer);
            }
            else//  if (transform.position.y <= _mgPong.horizontalSeparation.transform.position.z - _collider.radius)
            {
                if (_lastHit && !_bottomLeftPlayer.Equals(_lastHit))
                    _mgPong.IncrementScore(_lastHit);
                if (tempLastHit == null || _players.Count > 2)
                    _mgPong.IncrementAllScoreExcept(_bottomLeftPlayer);
            }
            StartCoroutine(Reset());
        }
        else if (transform.position.x > _rightLimit.position.x - _collider.radius)
        {
            if (_topRightPlayer && transform.position.y >= _mgPong.horizontalSeparation.transform.position.y)
            {
                if (_lastHit && !_topRightPlayer.Equals(_lastHit))
                    _mgPong.IncrementScore(_lastHit);
                if (tempLastHit == null || _players.Count > 2)
                    _mgPong.IncrementAllScoreExcept(_topRightPlayer);
            }
            else
            {
                if (_lastHit && !_bottomRightPlayer.Equals(_lastHit))
                    _mgPong.IncrementScore(_lastHit);
                if (tempLastHit == null || _players.Count > 2)
                    _mgPong.IncrementAllScoreExcept(_bottomRightPlayer);
            }
            StartCoroutine(Reset());
        }
    }

    public void Initialise(Transform leftLimit,
        Transform rightLimit,
        Transform topLimit,
        Transform bottomLimit,
        MinigamePong mgPong,
        Vector2 resetPosition,
        Player bottomLeftPlayer,
        Player bottomRightPlayer,
        Player topLeftPlayer,
        Player topRightPlayer)
    {
        _leftLimit = leftLimit;
        _rightLimit = rightLimit;
        _topLimit = topLimit;
        _bottomLimit = bottomLimit;
        _mgPong = mgPong;
        _resetPosition = resetPosition;
        _bottomLeftPlayer = bottomLeftPlayer;
        _bottomLeftPlayerMovement = _bottomLeftPlayer.GetComponent<UnitMovement>();
        _bottomRightPlayer = bottomRightPlayer;
        _bottomRightPlayerMovement = _bottomRightPlayer.GetComponent<UnitMovement>();
        _players = StaticObjects.GameController.GetPlayers();

        if (topLeftPlayer)
        {
            _topLeftPlayer = topLeftPlayer;
            _topLeftPlayerMovement = _topLeftPlayer.GetComponent<UnitMovement>();
        }
        if (topRightPlayer)
        {
            _topRightPlayer = topRightPlayer;
            _topRightPlayerMovement = _topRightPlayer.GetComponent<UnitMovement>();
        }
        _meshRotation = _mesh.GetComponent<PongBallRotate>();
        _currentSpeed = BaseSpeed;
    }

    public IEnumerator Reset()         
    {                                    
        _active = false;
        _lastHit = null;
        transform.position = _resetPosition;
        _direction = GetRandomDirection().normalized;
        _currentSpeed = BaseSpeed;
        _targetSpeed = _currentSpeed;
        yield return new WaitForSeconds(3);
        _active = true;
    }
    
    private Vector2 GetRandomDirection()
    {
        float[] xValues = new float[2];
        float[] yValues = new float[2];
        xValues[0] = Random.Range(-0.7f, -0.3f);
        xValues[1] = Random.Range(0.3f, 0.7f);
        yValues[0] = Random.Range(-0.7f, -0.3f);
        yValues[1] = Random.Range(0.3f, 0.7f);
        return new Vector2(xValues[Random.Range(0,2)], yValues[Random.Range(0, 2)]);
    }

    private void OnTriggerEnter(Collider collider)
    {
        Player player;
        if (collider.CompareTag("Weapon"))
            player = collider.GetComponentInParent<Player>();
        else if (collider.CompareTag("Player"))
            player = collider.GetComponent<Player>();
        else
            return;
        if (player.Equals(_lastHit) && Time.time - _timeLastHit < 0.5f)
            return;
        Vector3 direction = transform.right;
        direction.y += (player.transform.position.y < _basePosition.y ? 0.1f : -0.1f);
        ReflectDirection(direction);
        if ((player.transform.position.x < StaticObjects.MainCameraTransform.position.x && _direction.x < 0) ||
            (player.transform.position.x > StaticObjects.MainCameraTransform.position.x && _direction.x > 0))
            _direction.x *= -1;
        // StartCoroutine(AffectSpeed(collider.CompareTag("Player") ? PlayerHitBallSpeedModifier : PlayerHitBallSpeedModifier * 1.5f, collider.CompareTag("Player") ? 1 : 1.5f));
        _targetSpeed = _currentSpeed * (collider.CompareTag("Player")
            ? PlayerHitBallSpeedModifier
            : PlayerHitBallSpeedModifier * WeaponSpeedModifier);
        /*
        if (_targetSpeed > maxSpeed)
            _targetSpeed = maxSpeed;
            */
        _lastHit = player;
        _timeLastHit = Time.time;
    }

    private IEnumerator AffectSpeed(float modifier, float duration)
    {
        float tempSpeed = _currentSpeed;
        _targetSpeed = _currentSpeed * modifier;
        yield return new WaitForSeconds(duration);
        _targetSpeed = tempSpeed;
    }

    private void ReflectDirection(Vector3 normal)
    { 
        _direction = Vector2.Reflect(_direction, normal);
        if (Mathf.Abs(_direction.x) < _minimumHorizontal)
            _direction = new Vector2(_minimumHorizontal * Mathf.Sign(_direction.x), _direction.y);
    }

    private void AddSpeed()
    {
        if (_currentSpeed < BaseSpeed * 3f)
        {
            _currentSpeed += Time.deltaTime * 0.50f;
        }
    }
}
