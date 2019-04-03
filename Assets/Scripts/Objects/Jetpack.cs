using System.Collections;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
    private Player _player;

    private WaitForSeconds _cooldownDuration;
    private readonly WaitForSeconds _isGroundedForXSeconds;
    private readonly WaitForSeconds _isOnUseDuration;
    private IEnumerator _groundCoroutine;
    private bool _isOnUse;
    private float _usageTimeMaxJetPack;
    private float _usageTimeJetPack;
    private bool _isGroundedForEnoughTime;
    private ParticleSystem[] _childrenParticleSystems;
    private IEnumerator _onUseCoroutine;

    public bool isOnCooldown;
    public float cooldownDuration;
    public float useTime;

    private WaitForSeconds _groundCooldownDuration;

    private Jetpack()
    {
        _isGroundedForXSeconds = new WaitForSeconds(1f);
        _isOnUseDuration = new WaitForSeconds(0.1f);
    }

    public void Init(float cooldown, float usageTimeMaxJetPack)
    {
        _cooldownDuration = new WaitForSeconds(cooldown);
        _groundCooldownDuration = new WaitForSeconds(cooldown - 1);
        _usageTimeMaxJetPack = usageTimeMaxJetPack;
        cooldownDuration = cooldown;
        useTime = -1;
    }

    public float GetUsageTimeMax()
    {
        return _usageTimeMaxJetPack;
    }

    public float GetUsageTime()
    {
        return _usageTimeJetPack;
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        _childrenParticleSystems = _player.specialEffectJetPack.GetComponentsInChildren<ParticleSystem>();
        _usageTimeJetPack = 0;
        _isOnUse = false;
        _isGroundedForEnoughTime = true;
        ActiveSpecialEffectJetPack(false);
    }

    public void Update()
    {
        ActiveSpecialEffectJetPack(_isOnUse);
        if (_player.jetpackSound.isPlaying && !_isOnUse)
            _player.jetpackSound.Pause();
        if (_player.UnitMovement._isGrounded && _groundCoroutine == null && !_isGroundedForEnoughTime)
        {
            _groundCoroutine = GroundedForEnoughTime();
            StartCoroutine(_groundCoroutine);
        }

        if (!_isGroundedForEnoughTime) return;

        if (_usageTimeJetPack > 0)
            _usageTimeJetPack -= Time.deltaTime;
    }

    public bool ActivateJetPack()
    {
        if (isOnCooldown) return false;

        if (_groundCoroutine != null)
        {
            StopCoroutine(_groundCoroutine);
            _groundCoroutine = null;
        }

        _isGroundedForEnoughTime = false;

        if (_onUseCoroutine != null)
            StopCoroutine(_onUseCoroutine);

        _onUseCoroutine = OnUse();
        StartCoroutine(_onUseCoroutine);
        _usageTimeJetPack += Time.deltaTime;

        if (_usageTimeJetPack <= _usageTimeMaxJetPack)
        {
            if (!_player.jetpackSound.isPlaying && _isOnUse)
                _player.jetpackSound.Play();

            return true;
        }

        _player.jetpackSound.Stop();
        if (_onUseCoroutine != null)
        {
            StopCoroutine(_onUseCoroutine);
            _onUseCoroutine = null;
        }

        _isOnUse = false;
        _usageTimeJetPack = 0;
        StartCoroutine(Cooldown());
        return false;
    }

    private IEnumerator GroundedForEnoughTime()
    {
        _isGroundedForEnoughTime = false;

        yield return _groundCooldownDuration;

        _isGroundedForEnoughTime = true;
    }

    public void ForceCooldown()
    {
        StartCoroutine(Cooldown());
        _usageTimeJetPack = 0;
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

        yield return _isOnUseDuration;

        _isOnUse = false;
    }

    private void ActiveSpecialEffectJetPack(bool active)
    {
        Vector3 position;
        foreach (ParticleSystem child in _childrenParticleSystems)
        {
            ParticleSystem.EmissionModule childPSEmissionModule = child.emission;
            childPSEmissionModule.enabled = active;
            position = child.transform.localPosition;
            position.x = _player.UnitMovement.IsFacingRight ? 0.5f : 1f;
            child.transform.localPosition = position;
        }
    }

    public bool IsBeingUsed()
    {
        return !_isGroundedForEnoughTime;
    }
}
