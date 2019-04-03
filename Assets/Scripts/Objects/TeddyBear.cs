using System.Collections;
using UnityEngine;

public class TeddyBear : MonoBehaviour
{
    private Player _player;
    private AnimCtrl _animCtrl;

    private WaitForSeconds _cooldownDuration;
    private float _durationBeforeExplosion;
    private float _throwingForce;
    private float _radiusExplosion;
    private float _forceKnockBack;

    public bool isOnCooldown;
    public float cooldownDuration;
    public float useTime;

    public void Init(float cooldownTeddyBear, float durationBeforeExplosionTeddyBear, float throwingForceTeddyBear, float radiusExplosionTeddyBear,
        float forceKnockBackTeddyBear)
    {
        _cooldownDuration = new WaitForSeconds(cooldownTeddyBear);
        _durationBeforeExplosion = durationBeforeExplosionTeddyBear;
        _throwingForce = throwingForceTeddyBear;
        _radiusExplosion = radiusExplosionTeddyBear;
        _forceKnockBack = forceKnockBackTeddyBear;
        cooldownDuration = cooldownTeddyBear;
        useTime = -1;
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        _animCtrl = GetComponentInChildren<AnimCtrl>();
    }

    public bool ActivateTeddyBear()
    {
        if (isOnCooldown) return false;

        _player.UnitMovement.alreadyUsedAnObject = true;
        _animCtrl.UseTeddy();
        SpawnBomb();
        StartCoroutine(Cooldown());

        return true;
    }

    private void SpawnBomb()
    {
        Vector3 position = new Vector3(
            _player.Rigidbody.transform.position.x + (_player.UnitMovement.IsFacingRight ? 1 : -1),
            _player.Rigidbody.transform.position.y,
            _player.Rigidbody.transform.position.z);

        TeddyBearBomb bomb = Instantiate(_player.teddyBearBomb, position, _player.transform.rotation).GetComponent<TeddyBearBomb>();
        bomb.Init(true, _durationBeforeExplosion, _radiusExplosion, _forceKnockBack, _player);
        bomb.GetComponent<Rigidbody>().AddForce(new Vector2(_player.UnitMovement.IsFacingRight ? _throwingForce : -_throwingForce, _throwingForce * 0.5f));
        bomb.ActivateBomb();
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        useTime = Time.time;

        yield return _cooldownDuration;

        isOnCooldown = false;
        useTime = -1;
    }
}
