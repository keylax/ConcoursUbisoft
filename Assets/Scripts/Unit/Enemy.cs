using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    [SerializeField] public float maxLife = 100;

    public float life;
    public AIInput AIInput { get; private set; }
    private Slider _lifeBar;

    private readonly WaitForSeconds _effectsDuration;

    private Enemy()
    {
        _effectsDuration = new WaitForSeconds(1);
    }

    protected override void Awake()
    {
        AIInput = gameObject.AddComponent<AIInput>();
        isDead = false;
        life = maxLife;
        
        base.Awake();
        
        UnitMovement.Init(jumpForce, horizontalMovementSmoothing, groundLayers, groundCheck, customGravityForce, stunEffectObject);
        _lifeBar = GetComponentInChildren<Slider>();
        _lifeBar.maxValue = maxLife;
        _lifeBar.minValue = 0;
        _lifeBar.value = life;
        _lifeBar.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        isDead = false;
        life = maxLife;
        Rigidbody.velocity = Vector3.zero;
        MeleeAttackHitBox.gameObject.SetActive(false);
        AIInput.ResetStateOnRespawn();
        UnitMovement.ResetStun();
        _lifeBar.value = life;
        _lifeBar.gameObject.SetActive(false);
    }

    protected void Update()
    {
        Vector3 position = transform.position;
        position.y += 1.5f;
        position.z -= 1;
        if (_camera && _camera.transform.position.z - transform.position.z != 0)
            position.x += -1 * (_camera.transform.position.x - transform.position.x) / (_camera.transform.position.z - transform.position.z);
        
        _lifeBar.transform.position = position;
    }

    protected override UnitInput GetUnitInput()
    {
        return AIInput;
    }

    protected override void KillUnit()
    {
        // isDead = true;
    }

    public IEnumerator StopRespawnEffect()
    {
        specialEffectRespawn.SetActive(true);
        _particleSystemForRespawn.Play();

        yield return _effectsDuration;
        
        _particleSystemForRespawn.Stop();
        specialEffectRespawn.SetActive(false);
    }

    public IEnumerator StopDeathEffect()
    {
        GameObject deathEffect = Instantiate(specialEffectDeath, specialEffectDeath.transform.position, specialEffectDeath.transform.rotation);
        deathEffect.SetActive(true);
        deathEffect.GetComponent<ParticleSystem>().Play();

        yield return _effectsDuration;

        deathEffect.SetActive(false);
        Destroy(deathEffect);
    }

    public void PlayApparitionSound()
    {
        _audioSource.PlayOneShot(_apparitionSound);
    }

    public override void TakeAHit(Unit hitter, float damage)
    {
        if (hitter.GetComponent<Enemy>())  return;

        base.TakeAHit(hitter, damage);
        
        life -= damage;
        _lifeBar.gameObject.SetActive(life < maxLife);
        _lifeBar.value = life;
        
        if (life <= 0)
        {
            StartCoroutine(StopDeathEffect());
            _audioSource.PlayOneShot(deathSound);
            PlayApparitionSound();
            isDead = true;
        }
    }
}
