using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    private float _forceAppliedToUnitHit;
    private float _stunDuration;
    [SerializeField] private float damage = 50;

    private Unit _unit;

    private WaitForSeconds _cooldownDuration;
    private WaitForSeconds _hitBoxDuration;
    private bool _isOnCooldown;

    public void Init(float cooldown, float forceAppliedToUnitHit, float hitBoxPersistence, float stunDuration)
    {
        _forceAppliedToUnitHit = forceAppliedToUnitHit;
        _stunDuration = stunDuration;

        _cooldownDuration = new WaitForSeconds(cooldown);
        _hitBoxDuration = new WaitForSeconds(hitBoxPersistence);
    }

    private void Start()
    {
        _unit = GetComponent<Unit>();
    }

    public bool Attack()
    {
        if (_isOnCooldown)
            return false;

        _unit._audioSource.PlayOneShot(_unit.swingMeleeAttack);
        StartCoroutine(Cooldown());
        StartCoroutine(HitBoxActivationWindow());
        return true;
    }

    private IEnumerator Cooldown()
    {
        _isOnCooldown = true;

        yield return _cooldownDuration;

        _isOnCooldown = false;
    }

    private IEnumerator HitBoxActivationWindow()
    {
        _unit.MeleeAttackHitBox.gameObject.SetActive(true);

        yield return _hitBoxDuration;

        _unit.MeleeAttackHitBox.gameObject.SetActive(false);
    }

    public void PlayAttackEffect()
    {
        int ran = Random.Range(0, 3);
        if (ran == 0)
            _unit._audioSource.PlayOneShot(_unit.fightSound1);
        else if (ran == 1)
            _unit._audioSource.PlayOneShot(_unit.fightSound2);
        else
            _unit._audioSource.PlayOneShot(_unit.fightSound3);
    }

    public void HitUnit(GameObject unitHit)
    {
        PlayAttackEffect();
        int hitDirection = _unit.UnitMovement.IsFacingRight ? 1 : -1;
        Unit unitHitComponent = unitHit.GetComponent<Unit>();
        unitHitComponent.Rigidbody.AddForce(new Vector3(_forceAppliedToUnitHit * hitDirection, 0, 0), ForceMode.Impulse);
        unitHitComponent.TakeAHit(_unit, damage);

        if (!(_unit is Player player)) return;

        player.TouchAHit(unitHitComponent, damage);
        unitHitComponent.UnitMovement.StunUnit(_stunDuration);
    }
}
