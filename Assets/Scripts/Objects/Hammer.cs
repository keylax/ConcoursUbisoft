using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    [SerializeField] private float damage = 100;

    private Player _player;
    private AnimCtrl _animCtrl;

    private WaitForSeconds _cooldownDuration;
    private readonly WaitForSeconds _ejectionTime;
    private List<Unit> _lastUnitTouched;
    private float _forceKnockBack;
    private WaitForSeconds _hitBoxDuration;

    public bool isOnCooldown;
    public float cooldownDuration;
    public float useTime;

    private Hammer()
    {
        _lastUnitTouched = new List<Unit>();
        _ejectionTime = new WaitForSeconds(0.7f);
    }

    public void Init(float cooldownHammer, float forceKnockBackHammer, float hitBoxPersistenceHammer)
    {
        _cooldownDuration = new WaitForSeconds(cooldownHammer);
        _forceKnockBack = forceKnockBackHammer;
        _hitBoxDuration = new WaitForSeconds(hitBoxPersistenceHammer);
        _animCtrl = GetComponentInChildren<AnimCtrl>();
        cooldownDuration = cooldownHammer;
        useTime = -1;
    }

    private void Start()
    {
        _player = GetComponent<Player>();
    }

    public bool ActivateHammer()
    {
        if (isOnCooldown)
            return false;
        _player.UnitMovement.alreadyUsedAnObject = true;
        _animCtrl.UseBat();
        StartCoroutine(Cooldown());
        StartCoroutine(HitBoxActivationWindow());
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

    private IEnumerator HitBoxActivationWindow()
    {
        _player.MeleeAttackHitBox.isHammerTime = true;
        _player.MeleeAttackHitBox.gameObject.SetActive(true);

        yield return _hitBoxDuration;

        _player.MeleeAttackHitBox.gameObject.SetActive(false);
        _player.MeleeAttackHitBox.isHammerTime = false;
    }

    private IEnumerator EjectTarget()
    {
        foreach (Unit unit in _lastUnitTouched)
            unit.UnitMovement.SetIsEjected(true);

        yield return _ejectionTime;
        
        foreach (Unit unit in _lastUnitTouched)
            unit.UnitMovement.SetIsEjected(false);
        _lastUnitTouched.Clear();
    }

    public void HitUnit(Unit unitHit)
    {
        _player._audioSource.PlayOneShot(_player.baseballBatSound);
        _lastUnitTouched.Add(unitHit);
        StartCoroutine(EjectTarget());
        int hitDirection = _player.UnitMovement.IsFacingRight ? 1 : -1;
        unitHit.Rigidbody.AddForce(new Vector2(_forceKnockBack * hitDirection, (_forceKnockBack * 0.25f)), ForceMode.Impulse);
        unitHit.TakeAHit(_player, damage);
        _player.TouchAHit(unitHit, damage);
    }
}
