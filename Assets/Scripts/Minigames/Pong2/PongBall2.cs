using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBall2 : MonoBehaviour
{
    private const float HammerSpeedModifier = 2.5f;
    private const float BallHitPlayerSpeedModifier = 0.75f;
    private const float PlayerHitBallSpeedModifier = 1.5f;
    private const float BaseSpeed = 5f;

    private Vector2 _direction;
    private bool _active;
    private bool _leftPlayerScoredLast;
    private bool _isScoreNull = true;
    private Player _leftPlayer;
    private UnitMovement _leftPlayerMovement;
    private Player _rightPlayer;
    private UnitMovement _rightPlayerMovement;
    private Transform _leftLimit;
    private Transform _rightLimit;
    private Transform _topLimit;
    private Transform _bottomLimit;
    private MinigamePong2 _mgPong;
    private PongBallRotate2 _meshRotation;
    private float _currentSpeed;
    private Vector2 _resetPosition;
    [SerializeField] GameObject _mesh;
    [SerializeField] private SphereCollider _collider;
    [SerializeField] private bool _areBodyHitsActive;

    private void Update()
    {
        if (!_active)
            return;
        AddSpeed();
        _meshRotation.UpdateRotation();
        transform.Translate(_direction * _currentSpeed * Time.deltaTime);

        if (transform.position.y < _bottomLimit.position.y + _collider.radius && _direction.y < 0)
        {
            ReflectDirection(_bottomLimit.up);
        }

        if (transform.position.y > _topLimit.position.y - _collider.radius && _direction.y > 0)
        {
            ReflectDirection(-_topLimit.up);
        }

        if (transform.position.x < _leftLimit.position.x + _collider.radius && _direction.x < 0)
        {
            _leftPlayerScoredLast = false;
            _isScoreNull = false;
            _mgPong.IncrementScore(_rightPlayer);
            _currentSpeed = BaseSpeed;
        }

        if (transform.position.x > _rightLimit.position.x - _collider.radius && _direction.x > 0)
        {
            _leftPlayerScoredLast = true;
            _isScoreNull = false;
            _mgPong.IncrementScore(_leftPlayer);
            _currentSpeed = BaseSpeed;
        }
    }

    public void Initialise(Transform leftLimit,
        Transform rightLimit,
        Transform topLimit,
        Transform bottomLimit,
        MinigamePong2 mgPong,
        Vector2 resetPosition,
        Player leftPlayer,
        Player rightPlayer)
    {
        _leftLimit = leftLimit;
        _rightLimit = rightLimit;
        _topLimit = topLimit;
        _bottomLimit = bottomLimit;
        _mgPong = mgPong;
        _resetPosition = resetPosition;
        _leftPlayer = leftPlayer;
        _rightPlayer = rightPlayer;
        _leftPlayerMovement = _leftPlayer.GetComponent<UnitMovement>();
        _rightPlayerMovement = _rightPlayer.GetComponent<UnitMovement>();
        _meshRotation = _mesh.GetComponent<PongBallRotate2>();
        _currentSpeed = BaseSpeed;
    }

    public IEnumerator Reset()         
    {                                    
        _active = false;
        transform.position = _resetPosition;
        _direction = GetRandomDirection().normalized;
        yield return new WaitForSeconds(3);
        _active = true;
    }
    
    private Vector2 GetRandomDirection()
    {
        float[] xValues = new float[2];
        float[] yValues = new float[2];
        xValues[0] = Random.Range(-1f, -0.3f);
        xValues[1] = Random.Range(0.3f, 1f);
        yValues[0] = Random.Range(-1f, -0.3f);
        yValues[1] = Random.Range(0.3f, 1f);
        Vector2 direction = new Vector2(xValues[Random.Range(0, 2)], yValues[Random.Range(0, 2)]);

        if (_isScoreNull)
        {
            return direction;
        } else if (_leftPlayerScoredLast)
        {
            direction = new Vector2(xValues[1], yValues[1]);
        } else
        {
            direction = new Vector2(xValues[0], yValues[0]);
        }

        return direction;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Transform player = collider.transform.parent;
        if (collider.CompareTag("Weapon"))
        {
            if (LeftPlayerHitBall(collider))
            {
                ReflectDirection(_leftPlayer.transform.right);
                if (_leftPlayer.MeleeAttackHitBox.isHammerTime)
                {
                    StartCoroutine(AffectSpeed(HammerSpeedModifier, 1));
                } else
                {
                    StartCoroutine(AffectSpeed(PlayerHitBallSpeedModifier, 1));
                }
            }
            else if (RightPlayerHitBall(collider))
            {
                ReflectDirection(-_rightPlayer.transform.right);
                if (_rightPlayer.MeleeAttackHitBox.isHammerTime)
                {
                    StartCoroutine(AffectSpeed(HammerSpeedModifier, 1));
                } else
                {
                    StartCoroutine(AffectSpeed(PlayerHitBallSpeedModifier, 1));
                }
            }
        } else if (collider.CompareTag("Player"))
        {
            if (!_areBodyHitsActive) return;
            if (DidBallHitLeftPlayerBody(collider))
            {
                ReflectDirection(_leftPlayer.transform.right);
                StartCoroutine(AffectSpeed(BallHitPlayerSpeedModifier, 1.5f));
            } else if (DidBallHitRightPlayerBody(collider))
            {
                ReflectDirection(-_rightPlayer.transform.right);
                StartCoroutine(AffectSpeed(BallHitPlayerSpeedModifier, 1.5f));
            }
        }
    }

    private IEnumerator AffectSpeed(float modifier, float duration)
    {
        float tempSpeed = _currentSpeed;
        _currentSpeed = _currentSpeed * modifier;
        yield return new WaitForSeconds(duration);
        _currentSpeed = tempSpeed;
    }

    private void ReflectDirection(Vector3 normal)
    {
        _direction = Vector2.Reflect(_direction, normal);
    }

    private bool DidBallHitLeftPlayerBody(Collider collider)
    {
        Transform player = collider.transform;
        return player == _leftPlayer.transform
            && _direction.x < 0
            && player.position.x < transform.position.x;
    }

    private bool DidBallHitRightPlayerBody(Collider collider)
    {
        Transform player = collider.transform;
        return player == _rightPlayer.transform
            && _direction.x > 0
            && player.position.x > transform.position.x;
    }

    private bool LeftPlayerHitBall(Collider collider)
    {
        Transform player = collider.transform.parent;
        return player == _leftPlayer.transform
            && _direction.x < 0
            && player.position.x < transform.position.x
            && _leftPlayerMovement.IsFacingRight;
    }

    private bool RightPlayerHitBall(Collider collider)
    {
        Transform player = collider.transform.parent;
        return player == _rightPlayer.transform
            && _direction.x > 0
            && player.position.x > transform.position.x
            && !_rightPlayerMovement.IsFacingRight;
    }

    private void AddSpeed()
    {
        if (_currentSpeed < BaseSpeed * 3f)
        {
            _currentSpeed += Time.deltaTime * 0.50f;
        }
    }
}
