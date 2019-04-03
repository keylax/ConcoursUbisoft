using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackHitBox : MonoBehaviour
{
    private MeleeAttack _meleeAttack;
    private Hammer _hammer;
    public GameObject meleeAttackEffectObject;
    private ParticleSystem _meleeAttackParticles;




    public bool isHammerTime;

    private readonly List<GameObject> _unitsAlreadyHit;

    private MeleeAttackHitBox()
    {
        _unitsAlreadyHit = new List<GameObject>();
    }

    private void Awake()
    {
        _meleeAttack = GetComponentInParent<MeleeAttack>();
        _hammer = GetComponentInParent<Hammer>();
        _meleeAttackParticles = meleeAttackEffectObject.GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _unitsAlreadyHit.Clear();
        _unitsAlreadyHit.Add(transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!CanHitUnit(collider.gameObject)) return;

        StartCoroutine(StopMeleeAttackEffect());
        _unitsAlreadyHit.Add(collider.gameObject);
        if (!isHammerTime)
            _meleeAttack.HitUnit(collider.gameObject);
        else
            _hammer.HitUnit(collider.GetComponent<Unit>());
    }

    public IEnumerator StopMeleeAttackEffect()
    {
        meleeAttackEffectObject.SetActive(true);
        _meleeAttackParticles.Play();

        yield return new WaitForSeconds(0.6f);

        _meleeAttackParticles.Stop();
        meleeAttackEffectObject.SetActive(false);
    }

    private bool CanHitUnit(GameObject unitObject)
    {
        return !_unitsAlreadyHit.Contains(unitObject) && (unitObject.CompareTag("Enemy") || unitObject.CompareTag("Player"));
    }
}
