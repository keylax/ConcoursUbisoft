using UnityEngine;

public class BossSounds : MonoBehaviour
{
    private AudioSource _audioSource;
    public AudioClip laserSound;
    public AudioClip armSound;
    public AudioClip slamSound;
    public AudioClip deathSound;
    public AudioClip armImpact;
    public AudioClip movingLaser;
    public AudioClip shortCircuit;
    public AudioClip keyboardSound;
    public AudioClip phaseTwoDead;
    public AudioClip armExplosion;
    private bool _isPlayingSlam;
    private bool _isPlayingMovingLaser;
    private bool _isPlayingKeyboard;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayLaserSound()
    {
        _audioSource.PlayOneShot(laserSound);
    }

    public void PlayArmSound()
    {
        _audioSource.PlayOneShot(armSound);
    }

    public void PlaySlamSound()
    {
        if (!_isPlayingSlam)
        {
            _audioSource.PlayOneShot(slamSound);
            _isPlayingSlam = true;
        }
    }

    public void PlayDeathSound()
    {
        _audioSource.PlayOneShot(deathSound);
    }

    public void PlayImpact()
    {
        _audioSource.PlayOneShot(armImpact);
    }

    public void PlayMovingLaser()
    {
        if (!_isPlayingMovingLaser)
        {
            _audioSource.PlayOneShot(movingLaser);
            _isPlayingMovingLaser = true;
        }
    }

    public void StopSlamSound()
    {
        if (_isPlayingSlam)
            _audioSource.Stop();
        _isPlayingSlam = false;
    }

    public void StopMovingLaserSound()
    {
        if (_isPlayingMovingLaser)
            _audioSource.Stop();
        _isPlayingMovingLaser = false;
    }

    public void PlayKeyboard()
    {
        if (!_isPlayingKeyboard)
        {
            _audioSource.PlayOneShot(keyboardSound);
            _isPlayingKeyboard = true;
        }
    }

    public void PlayShortCircuit()
    {
        _audioSource.PlayOneShot(shortCircuit);
    }

    public void PlayPhase2Dead()
    {
        _audioSource.PlayOneShot(phaseTwoDead);
    }

    public void StopKeyboard()
    {
        if (_isPlayingKeyboard)
            _audioSource.Stop();
        _isPlayingKeyboard = false;
    }

    public void PlayArmExplosion()
    {
        _audioSource.PlayOneShot(armExplosion);
    }
}
