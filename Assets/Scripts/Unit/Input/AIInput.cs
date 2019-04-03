using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIInput : UnitInput
{
    private readonly List<Action> _registeredActions;
    private GameObject _aggro;
    private GameObject _softAggro;
    private bool _lock;
    private GameObject _lockingPlatform;
    private bool _triggered;
    private float _stuck;
    private MyLidar _lidar;
    private List<Transform> _platforms;
    private bool _landed = false;

    [SerializeField] public bool debug;
    [SerializeField] public bool stayOnPatrol;
    [SerializeField] public bool setableGoal;
    
    private Enemy _enemy;
    
    private Camera _camera;

    private struct MyTuple
    {
        public bool didItHit;
        public RaycastHit hit;
    }
    
    private struct MyLidar
    {
        public bool isFacingRight;
        public bool isGrounded;
        public float leftDistance;
        public float rightDistance;
        public GameObject platform;
        public MyTuple upHit;
        public MyTuple downHit;
        public MyTuple closeLeftHit;
        public MyTuple closeRightHit;
        public MyTuple farLeftHit;
        public MyTuple farRightHit;
        public MyTuple dropLeftHit;
        public MyTuple dropRightHit;
        public MyTuple jumpLeftHit;
        public MyTuple jumpRightHit;
    }
    
    
    private void UpdateMyLidar()
    {
        Vector3 transformPosition = transform.position;
        _lidar.isFacingRight = _enemy.UnitMovement.IsFacingRight;
        _lidar.isGrounded = _enemy.UnitMovement._isGrounded;
        
        _lidar.downHit.didItHit = Physics.Raycast(transformPosition, Vector3.down, out _lidar.downHit.hit, 1.5f);
        _lidar.upHit.didItHit = Physics.Raycast(transformPosition, Vector3.up, out _lidar.upHit.hit, 1.5f);
        if (_lidar.downHit.didItHit)
        {
            if (!_lidar.downHit.hit.collider.CompareTag("EnemySpawn"))
                _landed = true;
            _lidar.platform = _lidar.downHit.hit.collider.gameObject;
            _lidar.leftDistance = (_lidar.platform.transform.localScale.x / 2) - (_lidar.platform.transform.position.x - transform.position.x);
            _lidar.rightDistance = _lidar.platform.transform.localScale.x - _lidar.leftDistance;
        }
        _lidar.closeLeftHit.didItHit = Physics.Raycast(transformPosition, Vector3.left, out _lidar.closeLeftHit.hit, 0.8f);
        _lidar.closeRightHit.didItHit = Physics.Raycast(transformPosition, Vector3.right, out _lidar.closeRightHit.hit, 0.8f);
        
        _lidar.farLeftHit.didItHit = Physics.Raycast(transformPosition, Vector3.left, out _lidar.farLeftHit.hit, _lidar.leftDistance);
        _lidar.farRightHit.didItHit = Physics.Raycast(transformPosition, Vector3.right, out _lidar.farRightHit.hit, _lidar.rightDistance);
        
        _lidar.dropLeftHit.didItHit = Physics.Raycast(transformPosition, Vector3.left * 4 + Vector3.down * 5, out _lidar.dropLeftHit.hit, 7.1f);
        Debug.DrawRay(transformPosition, Vector3.left * 4 + Vector3.down * 5, Color.red);
        _lidar.dropRightHit.didItHit = Physics.Raycast(transformPosition, Vector3.right * 4 + Vector3.down * 5, out _lidar.dropRightHit.hit, 7.1f);
        Debug.DrawRay(transformPosition, Vector3.right * 4 + Vector3.down * 5, Color.red);
        _lidar.jumpLeftHit.didItHit = Physics.Raycast(transformPosition, Vector3.left * 5 + Vector3.up * 5, out _lidar.jumpLeftHit.hit, 7.1f);
        Debug.DrawRay(transformPosition, Vector3.left * 5 + Vector3.up * 5, Color.red);
        /*
        if (_lidar.jumpLeftHit.didItHit)
            Debug.Log(_lidar.jumpLeftHit.hit.collider.name);
            */
        _lidar.jumpRightHit.didItHit = Physics.Raycast(transformPosition, Vector3.right * 5 + Vector3.up * 5, out _lidar.jumpRightHit.hit, 7.1f);
        Debug.DrawRay(transformPosition, Vector3.right * 5 + Vector3.up * 5, Color.red);
        /*
        if (_lidar.jumpRightHit.didItHit)
            Debug.Log(_lidar.jumpRightHit.hit.collider.name);
            */
    }
    
    private AIInput()
    {
        _registeredActions = new List<Action>();
        _stuck = -1;
        _lidar = new MyLidar
        {
            upHit = new MyTuple(),
            downHit = new MyTuple(),
            closeLeftHit = new MyTuple(),
            closeRightHit = new MyTuple(),
            farLeftHit = new MyTuple(),
            farRightHit = new MyTuple(),
            dropLeftHit = new MyTuple(),
            dropRightHit = new MyTuple(),
            jumpLeftHit = new MyTuple(),
            jumpRightHit = new MyTuple()
        };
    }

    public override void UpdateInput()
    {
        _enemy.UnitMovement.Move(_horizontalMovement * Time.fixedDeltaTime, _jump, false, _attack, _useItem, _interaction);
        _jump = false;
        _attack = false;
        _useItem = false;
        _interaction = false;
    }

    private void Awake()
    {
        InvokeRepeating(nameof(FindSomewhereToExplore), 5f, 10f);
    }

    protected void Start()
    {
        _enemy = GetComponent<Enemy>();
        if (StaticObjects.CameraController)
            _camera = StaticObjects.CameraController.Camera;
        /*GameObject[] tempPlatforms = GameObject.FindGameObjectsWithTag("PlatformRespawn");
        _platforms = new List<Transform>();
        foreach (GameObject platform in tempPlatforms)
        {
            if (platform.GetComponentInChildren<MeshRenderer>().isVisible)
                _platforms.Add(platform.transform);
        }*/
        // _platforms = StaticObjects.MapCreationController.GetCurrentMap().GetRespawnPlatforms();
    }

    public void SetPlatforms(List<Transform> platforms)
    {
        _platforms = platforms;
    }
    
    public void ResetStateOnRespawn()
    {
        _lock = false;
        _landed = false;
        _lockingPlatform = null;
        _aggro = null;
        _softAggro = null;
        _triggered = false;
        _registeredActions.Clear();
    }

    private void FindSomewhereToExplore() // reset
    {
        _lock = false;
        _lockingPlatform = null;
        if(_platforms != null)
            _aggro = _platforms[Random.Range(0, _platforms.Count)].gameObject;
    }

    private void Update()
    {
        _registeredActions.Clear();
        UpdateMyLidar();
        if ((!_lidar.isFacingRight && _lidar.closeLeftHit.didItHit && _lidar.closeLeftHit.hit.collider.CompareTag("Player")) ||
            (_lidar.isFacingRight && _lidar.closeRightHit.didItHit && _lidar.closeRightHit.hit.collider.CompareTag("Player")))
        {
            _registeredActions.Add(Action.Action);
            _registeredActions.Add(_enemy.UnitMovement.IsFacingRight ? Action.MoveRight: Action.MoveLeft);
            ProcessMovement();
            return;
        }
        /*
        if (_stuck == -1 && gameObject.GetComponent<Rigidbody>().velocity == new Vector3(0, 0, 0) && !_softAggro)
            _stuck = Time.time;
        else if (_stuck != -1 && gameObject.GetComponent<Rigidbody>().velocity != new Vector3(0, 0, 0))
            _stuck = -1;
        else if (_stuck != -1 && gameObject.GetComponent<Rigidbody>().velocity == new Vector3(0, 0, 0) && Time.time - _stuck > 0.3f)
        {
            FindSomewhereToExplore();
            // Debug.Log("reset");
        }
        */
        if (!_landed && _camera != null)
        {
            _registeredActions.Add(transform.position.x > _camera.transform.position.x
                ? Action.MoveLeft
                : Action.MoveRight);
            if (debug)
                Debug.Log("enemySpawn");
        }
        else
        {
            if (setableGoal)
                SetGoal();
            if (!stayOnPatrol && !_softAggro && _aggro)
            {
                if (!GoToGoal())
                    _aggro = null;
            }
            else
                Patrol();
        }

        if (!_registeredActions.Contains(Action.Jump))
        {
            AvoidingFriends(_registeredActions.Contains(Action.MoveRight));
        }
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        _horizontalMovement = 0;
        float speed = _aggro || _softAggro || _triggered ? _runSpeed : _runSpeed / 2;

        if (_registeredActions.Contains(Action.MoveLeft) && !_registeredActions.Contains(Action.MoveRight))
        {
            _horizontalMovement = -speed;
        }

        if (!_registeredActions.Contains(Action.MoveLeft) && _registeredActions.Contains(Action.MoveRight))
        {
            _horizontalMovement = speed;
        }

        if (_registeredActions.Contains(Action.Jump))
        {
            _jump = true;
        }

        if (_registeredActions.Contains(Action.Action))
        {
            _attack = true;
        }
    }
    
    private void SetGoal()
    {
        List<Player> players = StaticObjects.GameController.GetPlayers();
        if (Input.GetMouseButtonDown(0) && players.Count > 0)
        {
            _aggro = players[0].gameObject;
            GameObject temp = FindClosestPlatform(players[0].gameObject);
            if (temp)
            {
                _aggro = temp;
                Debug.Log("Goal set to player 1 (the red one) current platform " + temp.name);
            }
            else
                Debug.Log("Can't set goal, player 1 (the red one) is not currently on a platform");
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _aggro = players[0].gameObject;
            Debug.Log("Goal set to player 2 (the blue one). Run.");
        }
    }

    private GameObject FindClosestPlatform(GameObject pos)
    {
        if (!pos)
            return null;
        GameObject closest = null;
        float minDist = float.MaxValue;
        foreach (Transform o in _platforms)
        {
            if (!pos.transform.Equals(o) && Vector3.Distance(o.transform.position, pos.transform.position) < minDist)
            {
                minDist = Vector3.Distance(o.transform.position, pos.transform.position);
                closest = o.gameObject;
            }
        }
        return closest;
    }

    private bool isPlatform(GameObject o)
    {
        if (o.CompareTag("Plank") || o.CompareTag("PlatformRespawn"))
            return true;
        return false;
    }

    private bool AvoidingFriends(bool wantToGoRight)
    {
        if (wantToGoRight && _lidar.isFacingRight && _lidar.closeRightHit.didItHit &&
            _lidar.closeRightHit.hit.collider.CompareTag("Enemy") && _lidar.leftDistance < _lidar.rightDistance)
        {
            if (debug)
                Debug.Log("friend ahead right, jumping");
            _registeredActions.Add(Action.Jump);
            _registeredActions.Add(Action.MoveRight);
        }
        else if (!wantToGoRight && !_lidar.isFacingRight && _lidar.closeLeftHit.didItHit &&
                 _lidar.closeLeftHit.hit.collider.CompareTag("Enemy") && _lidar.leftDistance >= _lidar.rightDistance)
        {
            if (debug)
                Debug.Log("friend ahead left, jumping");
            _registeredActions.Add(Action.Jump);
            _registeredActions.Add(Action.MoveRight);
        }
        else
            return false;
        return true;
    }

    private bool GoToGoal()
    {
        if (debug)
            Debug.Log("objectif = " + _aggro.name);
        if (_lidar.platform == _aggro)
        {
            if (debug)
                Debug.Log("finished");
            return false;
        }

        if (!_lidar.isGrounded)
        {
            _registeredActions.Add(_lidar.isFacingRight ? Action.MoveRight : Action.MoveLeft);
            if (debug)
                Debug.Log("is flying");
            return true;
        }

        if (ChoseSoftAggro())
            return false;
        if (_lidar.isFacingRight)
        {
            if (debug && _lidar.jumpRightHit.didItHit)
                Debug.Log("jump right did it it -> " + _lidar.jumpRightHit.hit.collider.name + " tag -> " + _lidar.jumpRightHit.hit.collider.tag);
            if (debug && _lidar.dropRightHit.didItHit)
                Debug.Log("drop right did it it -> " + _lidar.dropRightHit.hit.collider.name + " tag -> " + _lidar.dropRightHit.hit.collider.tag);
            if (_aggro.transform.position.y >= transform.position.y && _lidar.jumpRightHit.didItHit &&
                isPlatform(_lidar.jumpRightHit.hit.collider.gameObject)) // can jump right
            {
                _registeredActions.Add(Action.MoveRight);
                _registeredActions.Add(Action.Jump);
                if (debug)
                    Debug.Log("jump right");
            }
            else if (_lidar.rightDistance <= 0.4f && _lidar.dropRightHit.didItHit &&
                     isPlatform(_lidar.dropRightHit.hit.collider.gameObject)) // can drop right
            {
                _registeredActions.Add(Action.MoveRight);
                if (debug)
                    Debug.Log("drop right");
            }
        }
        else if (!_lidar.isFacingRight) // && _lidar.leftDistance < 0.4f)
        {
            if (debug && _lidar.jumpLeftHit.didItHit)
                Debug.Log("jump left did it it -> " + _lidar.jumpLeftHit.hit.collider.name + " tag -> " +
                          _lidar.jumpLeftHit.hit.collider.tag);
            if (debug && _lidar.dropLeftHit.didItHit)
                Debug.Log("drop left did it it -> " + _lidar.dropLeftHit.hit.collider.name + " tag -> " +
                          _lidar.dropLeftHit.hit.collider.tag);
            if (_aggro.transform.position.y >= transform.position.y && _lidar.jumpLeftHit.didItHit &&
                isPlatform(_lidar.jumpLeftHit.hit.collider.gameObject)) // can jump left
            {
                _registeredActions.Add(Action.MoveLeft);
                _registeredActions.Add(Action.Jump);
                if (debug)
                    Debug.Log("jump left " + _aggro.gameObject.name);
            }
            else if (_lidar.leftDistance <= 0.4f && _lidar.dropLeftHit.didItHit &&
                     isPlatform(_lidar.dropLeftHit.hit.collider.gameObject)) // can drop left
            {
                _registeredActions.Add(Action.MoveLeft);
                if (debug)
                    Debug.Log("drop left");
            }
        }
        if (_registeredActions.Count == 0 && Mathf.Abs(_aggro.transform.position.x - transform.position.x) > 0.5f)// continue forward
        {
            if (debug)
                Debug.Log("toward objective " + _aggro.gameObject.name);
            _registeredActions.Add(_aggro.transform.position.x < transform.position.x
                ? Action.MoveLeft
                : Action.MoveRight);
        }
        else if (Mathf.Abs(_aggro.transform.position.x - transform.position.x) <= 0.5f) // is on target, resetting
        {
            if (debug)
                Debug.Log("found it ! resetting aggro");
            return false;
        }
        return true;
    }
    
    private bool ChoseSoftAggro()
    {
        GameObject leftAggro = null;
        GameObject rightAggro = null;
        
        if (_lidar.farRightHit.didItHit && _lidar.farRightHit.hit.collider.CompareTag("Player"))
        {
            _registeredActions.Add(Action.MoveRight);
            rightAggro = _lidar.farRightHit.hit.collider.gameObject;
            _triggered = true;
            if (debug)
                Debug.Log("trigger right");
        }
        if (_lidar.farLeftHit.didItHit && _lidar.farLeftHit.hit.collider.CompareTag("Player"))
        {
            _registeredActions.Add(Action.MoveLeft);
            leftAggro = _lidar.farLeftHit.hit.collider.gameObject;
            _triggered = true;
            if (debug)
                Debug.Log("trigger left");
        }

        if (_softAggro && (_softAggro.transform.position.x >
                           _lidar.platform.transform.position.x + _lidar.platform.transform.localScale.x / 2 ||
                           _softAggro.transform.position.x <
                           _lidar.platform.transform.position.x - _lidar.platform.transform.localScale.x / 2 ||
                           _softAggro.transform.position.y > _lidar.platform.transform.position.y + 7 ||
                           _softAggro.transform.position.y < _lidar.platform.transform.position.y)) // reset aggro
        {
            _softAggro = null;
            _triggered = false;
            if (debug)
                Debug.Log("reset trigger");
            return false;
        }

        if (_softAggro)
            return true;
        if (leftAggro && rightAggro)
            _softAggro =
                (Vector3.Distance(leftAggro.transform.position, transform.position) <
                 Vector3.Distance(rightAggro.transform.position, transform.position)
                    ? leftAggro
                    : rightAggro);
        else
            _softAggro = leftAggro == null ? rightAggro : leftAggro;
        return (_softAggro);
    }

    private void Patrol()
    {
        _aggro = null;
        if (debug)
            Debug.Log("Patrol");
        if (ChoseSoftAggro())
        {
            if (debug)
                Debug.Log("soft aggro is " + _softAggro);
            if (_softAggro.transform.position.x < transform.position.x)
                _registeredActions.Add(Action.MoveLeft);
            else
                _registeredActions.Add(Action.MoveRight);
            return;
        }
        if (debug)
            Debug.Log("no soft aggro");
        _triggered = false;
        if ((_lidar.isFacingRight && _lidar.rightDistance <= 0.4) || (_lidar.isFacingRight && _lidar.closeRightHit.didItHit))
            _registeredActions.Add(Action.MoveLeft);
        else if ((!_lidar.isFacingRight && _lidar.leftDistance <= 0.4f) || (!_lidar.isFacingRight && _lidar.closeLeftHit.didItHit))
            _registeredActions.Add(Action.MoveRight);
        else
            _registeredActions.Add(_lidar.isFacingRight ? Action.MoveRight : Action.MoveLeft);
    }

    private enum Action
    {
        MoveLeft,
        MoveRight,
        Jump,
        Action
    };
}
