using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct LaserMovement
{
    public Vector3 moveTargetPos;
    public float moveStartTime;
    public float moveDistance;
    public Vector3 moveInitialPos;
    public float moveSpeed;
}

public class MovingLaserTarget : MonoBehaviour
{
    private const float MoveSpeed = 10f;
    private bool _isMovingToLeftWall;
    private bool _isMovingUp;
    private bool _isMoving;
    private bool _isCycleComplete;
    private LaserMovement _currentLaserMovement;
    private Vector2 _middleWaypoint;
    private Vector2 _endWaypoint;
    [SerializeField] private Transform _floorPos;
    [SerializeField] private Transform _leftWallPos;
    private void Start()
    {
        _middleWaypoint = new Vector2(_leftWallPos.position.x - 1, _floorPos.position.y - 1);
        _endWaypoint = new Vector2(_leftWallPos.position.x - 1, 0);
    }

    private void Update()
    {
        if (_isMoving)
        {
            DoMovement(_currentLaserMovement.moveInitialPos, _currentLaserMovement.moveTargetPos);
        }
    }

    public void Move(float initialX)
    {
        _isCycleComplete = false;
        transform.position = new Vector2(initialX, _floorPos.position.y);
        SetCurrentMovement(_middleWaypoint);
        _currentLaserMovement.moveSpeed = MoveSpeed;
        _isMoving = true;
        _isMovingToLeftWall = true;
    }

    private void MoveUpLeftWall()
    {
        SetCurrentMovement(_endWaypoint);
        _currentLaserMovement.moveSpeed = MoveSpeed;
        _isMovingUp = true;
    }

    private void SetCurrentMovement(Vector2 targetPosition)
    {
        _currentLaserMovement.moveInitialPos = transform.position;
        _currentLaserMovement.moveStartTime = Time.time;
        _currentLaserMovement.moveTargetPos = targetPosition;
        _currentLaserMovement.moveDistance = Vector3.Distance(_currentLaserMovement.moveTargetPos, _currentLaserMovement.moveInitialPos);
    }

    public bool IsCycleComplete()
    {
        return _isCycleComplete;
    }

    private void DoMovement(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        float distanceCovered = (Time.time - _currentLaserMovement.moveStartTime) * _currentLaserMovement.moveSpeed;
        float fractionOfDistance = distanceCovered / distance;
        if (from != to)
            transform.position = Vector3.Lerp(from, to, fractionOfDistance);

        if (transform.position == to)
        {
            if (_isMovingToLeftWall)
            {
                _isMovingToLeftWall = false;
                MoveUpLeftWall();
            }
            else if (_isMovingUp)
            {
                _isMovingUp = false;
                _isMoving = false;
                _isCycleComplete = true;
            }
        }
    }
}
