using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseOne : MonoBehaviour
{
    private const float ShootArmsDelay = 1f;
    private bool _phaseOneReady;
    private bool _armsReady;
    private Plane _floorPlane;
    private Plane _ceilingPlane;
    private Plane _leftWallPlane;
    private Plane _rightWallPlane;
    private Transform _leftWallPos;
    private Transform _rightWallPos;
    private Transform _floorPos;
    private Transform _ceilingWallPos;
    private List<Player> _players;
    private int _lastPlayerShotAt;
    private BossSounds _bossSounds;
    [Header("Arms")]
    [SerializeField] private Arm _leftArm;
    [SerializeField] private Arm _rightArm;
    private void Start()
    {
        _players = StaticObjects.GameController.GetPlayers();
    }

    public void Update()
    {
        _armsReady = CheckArmsStatus();
        PhaseOneCycle();
    }

    public void InitPhase(Transform leftWallPos, 
        Transform rightWallPos, 
        Transform floorPos, 
        Transform ceilingPos,
        BossSounds bossSounds)
    {
        _floorPlane = new Plane(floorPos.up, floorPos.position);
        _ceilingPlane = new Plane(-ceilingPos.up, ceilingPos.position);
        _leftWallPlane = new Plane(leftWallPos.right, leftWallPos.position);
        _rightWallPlane = new Plane(-rightWallPos.right, rightWallPos.position);
        _leftWallPos = leftWallPos;
        _rightWallPos = rightWallPos;
        _floorPos = floorPos;
        _ceilingWallPos = ceilingPos;
        _leftArm.SetBossSounds(bossSounds);
        _rightArm.SetBossSounds(bossSounds);
    }

    public void SetPhaseOneReady(bool isPhaseReady)
    {
        _phaseOneReady = isPhaseReady;
    }

    private bool CheckArmsStatus()
    {
        bool oneArmDestroyedAndOtherReady = _leftArm.IsDestroyed() && _rightArm.IsReady() || _leftArm.IsReady() && _rightArm.IsDestroyed();
        bool bothArmsReady = _leftArm.IsReady() && _rightArm.IsReady();
        if (bothArmsReady || oneArmDestroyedAndOtherReady)
        {
            return true;
        }
        return false;
    } 

    public bool IsLeftArmDestroyed()
    {
        return _leftArm.IsDestroyed();
    }

    public bool IsRightArmDestroyed()
    {
        return _rightArm.IsDestroyed();
    }

    private void ShootArms()
    {
        int leftArmTarget = 0;
        int rightArmTarget = 0;
        if (_players.Count > 1)
        {
            if (_leftArm.IsDestroyed() && !_rightArm.IsDestroyed())
            {
                do
                {
                    rightArmTarget = Random.Range(0, _players.Count);
                } while (rightArmTarget == _lastPlayerShotAt);
                _lastPlayerShotAt = rightArmTarget;
            }
            else if (!_leftArm.IsDestroyed() && _rightArm.IsDestroyed())
            {
                do
                {
                    leftArmTarget = Random.Range(0, _players.Count);
                } while (leftArmTarget == _lastPlayerShotAt);
                _lastPlayerShotAt = leftArmTarget;
            }
            else
            {
                leftArmTarget = Random.Range(0, _players.Count);
                do
                {
                    rightArmTarget = Random.Range(0, _players.Count);
                } while (leftArmTarget == rightArmTarget);
            }
        }

        try
        {
            _leftArm.Shoot(ComputeShootTargetPosition(_players[leftArmTarget].transform.position));
        }
        catch (UnityException exception)
        {
            Debug.Log("Something went wrong, not shooting the arm.");
        }

        try
        {
            _rightArm.Shoot(ComputeShootTargetPosition(_players[rightArmTarget].transform.position));
        }
        catch (UnityException exception)
        {
            Debug.Log("Something went wrong, not shooting the arm.");
        }
    }

    private Vector3 ComputeShootTargetPosition(Vector3 playerPosition)
    {
        Vector3 shootTarget = Vector3.zero;
        Vector3 direction = (playerPosition - transform.position).normalized;
        Debug.DrawRay(transform.position, (playerPosition - transform.position).normalized);
        Ray ray = new Ray(transform.position, direction);
        float enter = 0.0f;

        if (_floorPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.x >= _leftWallPos.position.x && hitPoint.x <= _rightWallPos.position.x)
            {
                return hitPoint;
            }
        }

        if (_ceilingPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.x >= _leftWallPos.position.x && hitPoint.x <= _rightWallPos.position.x)
            {
                return hitPoint;
            }
        }

        if (_leftWallPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.y >= _floorPos.position.y && hitPoint.y <= _ceilingWallPos.position.y)
            {
                return hitPoint;
            }
        }

        if (_rightWallPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            if (hitPoint.y >= _floorPos.position.y && hitPoint.y <= _ceilingWallPos.position.y)
            {
                return hitPoint;
            }
        }
        throw new UnityException();
    }

    private void PhaseOneCycle()
    {
        if (_phaseOneReady && _armsReady)
        {
            StartCoroutine(PrepareToShootArms());
        }
    }

    private IEnumerator PrepareToShootArms()
    {
        _phaseOneReady = false;
        yield return new WaitForSeconds(ShootArmsDelay);
        ShootArms();
        _phaseOneReady = true;
    }
}
