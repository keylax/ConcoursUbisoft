using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float firingTime = 5;
    [SerializeField] private float offTime = 3;
    [SerializeField] private float sparksTime = 1;
    [SerializeField] private ParticleSystem laserParticleSystem;
    [SerializeField] private ParticleSystem sparksParticleSystem;
    [SerializeField] private Light baseLight;
    [SerializeField] private Collider hitbox;
    [SerializeField] private float knockBackHorizontalForce = 1000;
    [SerializeField] private float knockBackVerticalForce = 200;
    [SerializeField] private float stunDuration = 0.7f;
    private float damages = 15;

    private bool _isActive;
    private bool _laserIsFiring;

    private WaitForSeconds _firingDuration;
    private WaitForSeconds _offDuration;
    private WaitForSeconds _sparksDuration;

    private void Start()
    {
        _firingDuration = new WaitForSeconds(firingTime);
        _offDuration = new WaitForSeconds(offTime);
        _sparksDuration = new WaitForSeconds(sparksTime);

        LaserOff();
    }

    public void ActivateLaser()
    {
        _isActive = true;
        StartCoroutine(LaserLife());
    }

    public void StopLaser()
    {
        _isActive = false;
    }

    private IEnumerator LaserLife()
    {
        while (_isActive)
        {
            yield return _offDuration;
            
            Spark();
            
            yield return _sparksDuration;
            
            LaserOn();

            yield return _firingDuration;

            LaserOff();
        }
    }

    private void Spark()
    {
        sparksParticleSystem.enableEmission = true;
    }

    private void LaserOn()
    {
        laserParticleSystem.enableEmission = true;
        baseLight.enabled = true;
        hitbox.enabled = true;
        _laserIsFiring = true;
    }

    private void LaserOff()
    {
        laserParticleSystem.enableEmission = false;
        sparksParticleSystem.enableEmission = false;
        baseLight.enabled = false;
        hitbox.enabled = false;
        _laserIsFiring = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_laserIsFiring || !other.gameObject.CompareTag("Player")) return;

        Player player = other.gameObject.GetComponent<Player>();
        player.Rigidbody.AddForce(new Vector2(other.transform.position.x < transform.position.x ? -knockBackHorizontalForce : knockBackHorizontalForce, knockBackVerticalForce));

        player.UnitMovement.StunUnit(stunDuration);
        player.TakeAHitFromSawOrLaser(damages);
    }
}
