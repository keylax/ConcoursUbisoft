using System.Collections;
using System.Linq;
using Audio;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private AnimCtrl _animCtrl;
    private float _jumpForce;
    private float _horizontalMovementSmoothing;
    private LayerMask _groundLayers;
    private Transform _groundCheck;
    private ConstantForce _customGravityForce;
    private float _jetPackForce;

    private const float GroundedRadius = 0.2f;
    private const float MaximumDistanceFromPlatform = .0335f;

    public bool _isGrounded;
    private bool _jumpUsed;
    private bool _isJumping;
    private bool _isStunned;
    private bool _nextToObject;
    public bool alreadyUsedAnObject;
    private GameObject _lastObjectTouch;

    private readonly WaitForSeconds _plankDisableDelay;
    private Collider _playerCollider;
    private Collider _ignoredPlankCollider;

    private Unit _unit;
    private Rigidbody _rigidbody;

    private Vector3 _velocityReference;

    private bool _isFollowedByCamera;
    private bool _isEjected;

    private GameObject _stunSpecialEffect;
    private GameObject _stunEffect = null;

    public bool IsFacingRight { get; private set; }

    private UnitMovement()
    {
        IsFacingRight = true;

        _plankDisableDelay = new WaitForSeconds(0.3f);
    }

    public void Init(float jumpForce, float horizontalMovementSmoothing, LayerMask groundLayers, Transform groundCheck, ConstantForce customGravityForce, GameObject stunEffectObject,
        float jetPackForce = 0)
    {
        _stunSpecialEffect = stunEffectObject;
        _jumpForce = jumpForce;
        _horizontalMovementSmoothing = horizontalMovementSmoothing;
        _groundLayers = groundLayers;
        _groundCheck = groundCheck;
        _customGravityForce = customGravityForce;
        _jetPackForce = jetPackForce;
    }

    private void Start()
    {
        _unit = GetComponent<Unit>();
        _animCtrl = GetComponentInChildren<AnimCtrl>();
        _playerCollider = GetComponent<Collider>();
        _rigidbody = _unit.Rigidbody;
        _isEjected = false;
        _nextToObject = false;
        alreadyUsedAnObject = false;
    }

    private void Update()
    {
        if (_stunEffect != null)
        {
            Vector3 pos = transform.position;
            pos.y += 1f;
            _stunEffect.transform.position = pos;
        }
    }

    public void UpdateMovement()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = false;

        if (!_isJumping)
        {
            Collider[] colliders = Physics.OverlapSphere(_groundCheck.position, GroundedRadius, _groundLayers);
            if (colliders.Any(collider => collider.gameObject != gameObject && collider != _ignoredPlankCollider))
            {
                
                _isGrounded = true;
                _jumpUsed = false;
                if (!wasGrounded && _customGravityForce)
                {
                    _unit._audioSource.PlayOneShot(_unit.landSound);
                    _customGravityForce.enabled = false;
                }
            }
            else if (wasGrounded && _customGravityForce)
            {
                _customGravityForce.enabled = true;
            }
        }
        
        if (wasGrounded != _isGrounded) // Change state
            _animCtrl.UpdateGrounded(_isGrounded);

        if (_isFollowedByCamera)
        {
            if (_rigidbody.velocity.x > 1)
            {
                StaticObjects.MainCameraTransform.position += Vector3.right * _rigidbody.velocity.x * Time.fixedDeltaTime;
            }
            else
            {
                StaticObjects.CameraController.AutoScroll();
            }
        }
        if (_unit is Player player)
        {
            Player playertmp = (Player)_unit;
            if (!alreadyUsedAnObject && playertmp.hasItem)
                player.displayControllerButton.ActivateButton(true);
            else
                player.displayControllerButton.ActivateButton(false);
        }
    }

    public void StunUnit(float stunDuration)
    {
        StartCoroutine(ApplyStunForDuration(new WaitForSeconds(stunDuration)));
    }

    private IEnumerator ApplyStunForDuration(WaitForSeconds stunDuration)
    {
        // special effect
        Vector3 pos = transform.position;
        pos.y += 1f;

        _stunEffect = Instantiate(_stunSpecialEffect, pos, _stunSpecialEffect.transform.rotation);
        _stunEffect.SetActive(true);
        _stunEffect.GetComponent<ParticleSystem>().Play();

        // end special effect
        _isStunned = true;

        yield return stunDuration;
        if (_stunEffect)
        {
            _stunEffect.SetActive(false);
            Destroy(_stunEffect);
            _stunEffect = null;
        }

        _isStunned = false;
    }

    private void WalkSound(float move)
    {
        if (!(_unit is Player player)) return;
        if (StaticObjects.GameController._ending)
        {
            player.walkSound.Pause();
            return;
        }

        if (move != 0 && !_isEjected && !_isJumping && _isGrounded)
        {
            if (!player.walkSound.isPlaying)
                player.walkSound.Play();
        }
        else
            player.walkSound.Pause();
    }

    public void Move(float move, bool jump, bool jumpDown, bool attack, bool useItem, bool interaction)
    {
        if (_isStunned)
        {
            move = 0;
            jump = false;
            jumpDown = false;
            attack = false;
            useItem = false;
        }

        if (_isJumping && _rigidbody.velocity.y <= 0)
            _isJumping = false;

        WalkSound(move);

        Vector3 targetVelocity = new Vector2(move * 10f, _rigidbody.velocity.y);
        if (!_isEjected)
            _rigidbody.velocity = Vector3.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _velocityReference, _horizontalMovementSmoothing);

        if (!_isJumping)
        {
            if (Physics.Raycast(_groundCheck.position, Vector3.down, out RaycastHit hit, GroundedRadius, _groundLayers))
            {
                _rigidbody.velocity = Vector3.right * _rigidbody.velocity.x;
                if (PlatformDistanceIsBigEnough(hit))
                    transform.position += Vector3.down * hit.distance;
            }
        }

        if (IsMovingInTheOtherDirection(move) && !_unit.MeleeAttackHitBox.gameObject.activeSelf)
        {
            Flip();
            _animCtrl.Flip();
        }

        if (attack && _unit.MeleeAttack.Attack())
        {
            _animCtrl.TriggerAttack();
        }

        if (_nextToObject && interaction)
        {
            ChangeObject();
        }

        if (useItem && _unit is Player && UseObject()) return;

        if (CanJump(jump))
        {
            if (_ignoredPlankCollider)
            {
                Physics.IgnoreCollision(_ignoredPlankCollider, _playerCollider, false);
                _ignoredPlankCollider = null;
            }

            _isJumping = true;
            _jumpUsed = true;
            _isGrounded = false;
            _unit._audioSource.PlayOneShot(_unit.jumpSound);
            _rigidbody.velocity = Vector3.right * _rigidbody.velocity.x;
            _rigidbody.AddForce(new Vector2(0f, _jumpForce));
            if (_customGravityForce)
                _customGravityForce.enabled = true;
            _animCtrl.TriggerJump();
        }
        else if (CanJumpDown(jumpDown))
        {
            Collider[] colliders = Physics.OverlapSphere(_groundCheck.position, GroundedRadius, _groundLayers);
            foreach (Collider aCollider in colliders)
            {
                if (!aCollider.CompareTag("Plank")) continue;

                _ignoredPlankCollider = aCollider;
                Physics.IgnoreCollision(_ignoredPlankCollider, _playerCollider);
                _isGrounded = false;
                if (_customGravityForce)
                    _customGravityForce.enabled = true;
                StartCoroutine(RemoveIgnoredPlankCollider());
                break;
            }
        }
        
        _animCtrl.UpdateMove(Mathf.Abs(move));
    }

    private IEnumerator RemoveIgnoredPlankCollider()
    {
        yield return _plankDisableDelay;

        _ignoredPlankCollider = null;
    }

    public void SetIsFollowedByCamera(bool isFollowedByCamera)
    {
        _isFollowedByCamera = isFollowedByCamera;
    }

    public void SetIsEjected(bool isEjected)
    {
        _isEjected = isEjected;
    }

    public bool ItemCanBeUsed()
    {
        Player player = (Player)_unit;
        if (!player.hasItem)
            return false;

        return player.hammerActivation && !player.Hammer.isOnCooldown ||
               player.dashActivation && !player.Dash.isOnCooldown ||
               player.remoteControlActivation && !player.RemoteControl.isOnCooldown ||
               player.teddyBearActivation && !player.TeddyBear.isOnCooldown ||
               player.jetpackActivation && !player.Jetpack.isOnCooldown;
    }

    private bool UseObject()
    {
        Player player = (Player)_unit;

        if (player.jetpackActivation && player.Jetpack.ActivateJetPack()) // JetPack
        {
            if (_isGrounded)
                _rigidbody.AddForce(new Vector2(0f, _jumpForce));
            _isJumping = true;
            _isGrounded = false;
            _rigidbody.velocity = Vector3.right * _rigidbody.velocity.x;
            _rigidbody.AddForce(new Vector2(0f, _jetPackForce));
            if (_customGravityForce)
                _customGravityForce.enabled = true;
            alreadyUsedAnObject = true;
            return true;
        }

        // Other items
        return player.hammerActivation && player.Hammer.ActivateHammer() ||
               player.dashActivation && player.Dash.ActivateDash() ||
               player.remoteControlActivation && player.RemoteControl.ActivateRemoteControl() ||
               player.teddyBearActivation && player.TeddyBear.ActivateTeddyBear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Object")) return;

        _nextToObject = true;
        _lastObjectTouch = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Object"))
            _nextToObject = false;
    }

    private void ChangeObject()
    {
        Player player = (Player)_unit;

        ThrowObject();
        if (!_lastObjectTouch)
            return;

        PickObject item = _lastObjectTouch.GetComponent<PickObject>();
        if (!item.CanPickUp()) return;

        item.LockPickUp();

        PickObject.ObjectName itemName = item.itemName;
        player.item = itemName;
        player.hasItem = true;
        player.jetpackActivation = false;
        player.dashActivation = false;
        player.remoteControlActivation = false;
        player.teddyBearActivation = false;
        player.hammerActivation = false;
        player._audioSource.PlayOneShot(player._changeObjectSound);

        VoiceLine itemLine = null;
        
        switch (itemName)
        {
            case PickObject.ObjectName.Jetpack:
                player.jetpackActivation = true;
                itemLine = player.jetpackLine;
                break;
            case PickObject.ObjectName.Dash:
                player.dashActivation = true;
                itemLine = player.dashLine;
                break;
            case PickObject.ObjectName.RemoteControl:
                player.remoteControlActivation = true;
                itemLine = player.remoteLine;
                break;
            case PickObject.ObjectName.TeddyBear:
                player.teddyBearActivation = true;
                itemLine = player.teddyLine;
                break;
            case PickObject.ObjectName.Hammer:
                player.hammerActivation = true;
                itemLine = player.hammerLine;
                break;
        }

        if (item.CanPlayVoiceLine() && StaticObjects.SpeakerLinesManager && itemLine != null)
        {
            StaticObjects.SpeakerLinesManager.AddToQueue(itemLine); 
            item.LockPlayVoiceLine();
        }
        item.DestroyObject();
        _nextToObject = false;
    }

    private void ThrowObject()
    {
        Player player = (Player)_unit;

        PickObject.ObjectName actualObject;
        if (player.jetpackActivation)
            actualObject = PickObject.ObjectName.Jetpack;
        else if (player.dashActivation)
            actualObject = PickObject.ObjectName.Dash;
        else if (player.remoteControlActivation)
            actualObject = PickObject.ObjectName.RemoteControl;
        else if (player.teddyBearActivation)
            actualObject = PickObject.ObjectName.TeddyBear;
        else if (player.hammerActivation)
            actualObject = PickObject.ObjectName.Hammer;
        else
            return;

        Vector3 position = new Vector3(
            _unit.UnitMovement.IsFacingRight ? _unit.Rigidbody.transform.position.x + 1f : _unit.Rigidbody.transform.position.x - 1f,
            _unit.Rigidbody.transform.position.y,
            _unit.Rigidbody.transform.position.z);

        Rigidbody objectPickerRigidBody = StaticObjects.ItemPool.SpawnItem(position, actualObject, _unit.transform.rotation).GetComponent<Rigidbody>();
        objectPickerRigidBody.AddForce(_unit.UnitMovement.IsFacingRight ? new Vector2(-300, 200) : new Vector2(300, 200));
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;

        Vector3 localPosition = _unit.MeleeAttackHitBox.transform.localPosition;
        localPosition = new Vector3(localPosition.x * -1, localPosition.y,
            localPosition.z);
        _unit.MeleeAttackHitBox.transform.localPosition = localPosition;

        /*Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;*/
    }

    private bool CanJumpDown(bool jumpDown)
    {
        return jumpDown && _isGrounded;
    }

    private bool CanJump(bool jump)
    {
        return jump && (_isGrounded || (!_isGrounded && !_jumpUsed));
    }

    private bool IsMovingInTheOtherDirection(float move)
    {
        return (move > 0 && !IsFacingRight) || (move < 0 && IsFacingRight);
    }

    private bool PlatformDistanceIsBigEnough(RaycastHit hit)
    {
        return hit.distance > MaximumDistanceFromPlatform;
    }

    public bool HasControl()
    {
        return !_isStunned;
    }

    public void ResetStun()
    {
        _isStunned = false;
    }
}
