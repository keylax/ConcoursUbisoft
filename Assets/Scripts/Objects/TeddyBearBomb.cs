using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TeddyBearBomb : MonoBehaviour
{
    private bool _canBeLaunched;
    private WaitForSeconds _durationBeforeExplosion;
    private readonly WaitForSeconds _durationBeforeDestroy;
    private float _radiusExplosion;
    private float _forceKnockBack;
    private bool _isExploding;
    private bool _destroyGameObject;
    private Player _player;
    private AudioSource _audio;
    private Collider _collider;
    private Renderer _renderer;
    private Rigidbody _rigidbody;

    [SerializeField] private float damage = 85;
    [SerializeField] private ParticleSystem specialEffectTeddyBearBomb;
    [SerializeField] private AudioClip teddySound;
    [SerializeField] private AudioClip explosionSound; // https://www.freesoundeffects.com/free-sounds/explosion-10070/

    private TeddyBearBomb()
    {
        _durationBeforeDestroy = new WaitForSeconds(1.5f);
    }

    public void Init(bool canBeLaunched, float durationBeforeExplosion, float radiusExplosion, float forceKnockBack, Player player)
    {
        _canBeLaunched = canBeLaunched;
        _audio = GetComponent<AudioSource>();
        _durationBeforeExplosion = new WaitForSeconds(durationBeforeExplosion);
        _radiusExplosion = radiusExplosion;
        _forceKnockBack = forceKnockBack;
        _player = player;
        ActiveSpecialEffectTeddyBearBomb(false);
    }

    public void AddTorque()
    {
        _rigidbody.AddTorque(transform.forward * Random.Range(-20, 20));
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        if (!_canBeLaunched) return;

        _audio.PlayOneShot(teddySound);
        if (!_player.UnitMovement.IsFacingRight)
            transform.rotation = new Quaternion(0, -90, 0, 0);
    }

    private void Update()
    {
        if (_isExploding)
        {
            ExplosionAffectingOthers();
            ActiveSpecialEffectTeddyBearBomb(true);
            _renderer.enabled = false;
            StartCoroutine(Destroy());
            _isExploding = false;
        }

        if (_destroyGameObject)
        {
            Destroy(gameObject);
        }
    }

    private void ExplosionAffectingOthers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radiusExplosion);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (!rb || (!rb.CompareTag("Enemy") && !rb.CompareTag("Player"))) continue;

            rb.AddExplosionForce(_forceKnockBack, transform.position, _radiusExplosion);

            if (!_canBeLaunched || rb.Equals(_player.Rigidbody)) continue;

            Unit unitHit = rb.GetComponent<Unit>();
            unitHit.TakeAHit(_player, damage);
            _player.TouchAHit(unitHit, damage);
        }
    }

    public void ActivateBomb()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return _durationBeforeExplosion;

        transform.GetChild(1).gameObject.SetActive(false);
        _collider.enabled = false;
        _isExploding = true;
        _audio.PlayOneShot(explosionSound);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_canBeLaunched) return;

        try
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            // ignored
        }

        _collider.enabled = false;
        _isExploding = true;
        _audio.PlayOneShot(explosionSound);
        /*
        if (transform.childCount == 2)
            _audio.Play();
        */
    }

    private IEnumerator Destroy()
    {
        yield return _durationBeforeDestroy;

        _destroyGameObject = true;
    }

    private void ActiveSpecialEffectTeddyBearBomb(bool active)
    {
        if (active)
            specialEffectTeddyBearBomb.Play();
        else
            specialEffectTeddyBearBomb.Stop();
    }
}
