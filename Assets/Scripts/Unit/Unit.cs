using System.Collections;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] protected float runSpeed = 30;

    [Header("Melee Attack")]
    [SerializeField] private float cooldownMeleeAttack = 0.7f;

    [SerializeField] private float forceAppliedToUnitHitMeleeAttack = 25;
    [SerializeField] private float hitBoxPersistenceMeleeAttack = 0.2f;
    [SerializeField] private float stunDurationMeleeAttack = 0.5f;
    [SerializeField] protected GameObject stunEffectObject;


    [Header("Movement")]
    [SerializeField] protected float jumpForce = 800f;
    [Range(0, .3f)] [SerializeField] protected float horizontalMovementSmoothing = .05f;
    [SerializeField] protected LayerMask groundLayers;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected ConstantForce customGravityForce;

    [Header("Death")]
    [SerializeField] private float timeBetweenScreenPositionChecks = 0.5f;
    [SerializeField] protected float timeToResetLastHit = 1;
    public bool isDead;
    [SerializeField] protected GameObject specialEffectRespawn;
    [SerializeField] protected GameObject specialEffectDeath;
    [SerializeField] private float respawnEffectDuration = 1;

    [Header("Sound Unit")]
    [SerializeField] protected AudioClip _apparitionSound;
    [SerializeField] public AudioClip fightSound1;
    [SerializeField] public AudioClip fightSound2;
    [SerializeField] public AudioClip fightSound3;
    [SerializeField] public AudioClip deathSound;
    [SerializeField] public AudioClip jumpSound;
    [SerializeField] public AudioClip landSound;
    [SerializeField] public AudioClip swingMeleeAttack;


    public MeleeAttack MeleeAttack { get; private set; }
    public MeleeAttackHitBox MeleeAttackHitBox { get; private set; }
    private UnitInput UnitInput => GetUnitInput();
    public UnitMovement UnitMovement { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    public int ID { get; set; }

    public Unit lastHit;
    private readonly WaitForSeconds _delayLastHitReset;

    protected Camera _camera;

    protected ParticleSystem _particleSystemForRespawn;
    protected readonly WaitForSeconds _delayRespawnEffect;

    public  AudioSource _audioSource;

    protected Unit()
    {
        _delayLastHitReset = new WaitForSeconds(timeToResetLastHit);
        _delayRespawnEffect = new WaitForSeconds(respawnEffectDuration);
    }

    protected virtual void Awake()
    {
        MeleeAttack = gameObject.AddComponent<MeleeAttack>();
        MeleeAttackHitBox = GetComponentInChildren<MeleeAttackHitBox>(true);

        UnitMovement = gameObject.AddComponent<UnitMovement>();
        Rigidbody = GetComponent<Rigidbody>();

        MeleeAttack.Init(cooldownMeleeAttack, forceAppliedToUnitHitMeleeAttack, hitBoxPersistenceMeleeAttack, stunDurationMeleeAttack);

        UnitInput.Init(runSpeed);

        _particleSystemForRespawn = specialEffectRespawn.GetComponent<ParticleSystem>();
        _audioSource = this.GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        if (StaticObjects.CameraController)
            _camera = StaticObjects.CameraController.Camera;

        InvokeRepeating(nameof(IsUnitOutOfScreen), timeBetweenScreenPositionChecks, timeBetweenScreenPositionChecks);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        UnitMovement.UpdateMovement();
        UnitInput.UpdateInput();
    }

    // TODO: This is only good in platforming sections, a hitbox outside of the camera would be better
    private void IsUnitOutOfScreen()
    {
        if (_camera == null)
            return;
        Vector3 screenPos = _camera.WorldToViewportPoint(transform.position);
        if (screenPos.x < -0.05 || screenPos.x > 1.05 || screenPos.y < -0.05 || screenPos.y > 1.05 || screenPos.z < 0)
            KillUnit();
    }

    public virtual void TakeAHit(Unit hitter, float damage)
    {
        lastHit = hitter;
        StartCoroutine(ResetLastHit());
    }

    private IEnumerator ResetLastHit()
    {
        yield return _delayLastHitReset;

        lastHit = null;
    }

    protected abstract UnitInput GetUnitInput();
    protected abstract void KillUnit();
}
