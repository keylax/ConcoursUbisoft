using System.Collections;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [SerializeField] private float damage = 75;

    private Player _player;

    private WaitForSeconds _cooldownDuration;
    private readonly WaitForSeconds _isOnUseDuration;
    private float _stunDurationDash;
    private float _dashForce;
    private bool _isOnUse;
    private GameObject _lastHit;
    private ParticleSystem[] _childrenParticleSystems;

    public bool isOnCooldown;
    public float cooldownDuration;
    public float useTime;
 
    private Dash()
    {
        _isOnUseDuration = new WaitForSeconds(0.15f);
    }

    public void Init(float cooldown, float stunDurationDash, float dashForce)
    {
        _cooldownDuration = new WaitForSeconds(cooldown);
        cooldownDuration = cooldown;
        useTime = -1;
        _stunDurationDash = stunDurationDash;
        _dashForce = dashForce;
    }

    public void FixedUpdate()
    {
        if (!_isOnUse) return;

        _player.Rigidbody.velocity = Vector3.right * _player.Rigidbody.velocity.x;
        _player.Rigidbody.AddForce(new Vector2(_player.UnitMovement.IsFacingRight ? _dashForce : -_dashForce, 0), ForceMode.Force);
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        _childrenParticleSystems = _player.specialEffectDash.GetComponentsInChildren<ParticleSystem>();
        ActiveSpecialEffectDash(false);
        _isOnUse = false;
    }

    public bool ActivateDash()
    {
        if (isOnCooldown)
            return false;
        _player.UnitMovement.alreadyUsedAnObject = true;
        _player._audioSource.PlayOneShot(_player._dashSound);
        StartCoroutine(OnUse());
        StartCoroutine(Cooldown());
        return true;
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        useTime = Time.time;
        yield return _cooldownDuration;
        isOnCooldown = false;
        useTime = -1;
    }

    private IEnumerator OnUse()
    {
        _isOnUse = true;
        ActiveSpecialEffectDash(true);

        yield return _isOnUseDuration;

        _isOnUse = false;
        ActiveSpecialEffectDash(false);
    }

    private void ActiveSpecialEffectDash(bool active)
    {
        foreach (ParticleSystem child in _childrenParticleSystems)
        {
            if (active)
                child.Play();
            else
                child.Stop();
        }
    }

    private void OnCollisionEnter(Collision collider)
    {
        Collision(collider);
    }
    
    private void OnCollisionStay(Collision collider)
    {
       Collision(collider);
    }

    private void OnCollisionExit(Collision other)
    {
        _lastHit = null;
    }

    private void Collision(Collision collider)
    {
        if (!CanHitUnit(collider.gameObject) || !_isOnUse || (_lastHit && collider.gameObject.Equals(_lastHit))) return;

        _lastHit = collider.gameObject;
        Unit unitHit = collider.gameObject.GetComponent<Unit>();
        unitHit.UnitMovement.StunUnit(_stunDurationDash);
        unitHit.TakeAHit(_player, damage);
        _player.TouchAHit(collider.gameObject.GetComponent<Unit>(), damage);
    }

    private bool CanHitUnit(GameObject unitObject)
    {
        return (unitObject.CompareTag("Enemy") || unitObject.CompareTag("Player"));
    }
}
