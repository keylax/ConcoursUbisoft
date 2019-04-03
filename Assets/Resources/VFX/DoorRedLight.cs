using UnityEngine;

public class DoorRedLight : MonoBehaviour
{
    public bool isActive;
    public GameObject lightBulb;
    public GameObject emitLight;
    public GameObject spotLight1;
    public GameObject spotLight2;
    private Light _emitLight;
    private Light _spotLight1;
    private Light _spotLight2;
    private bool _lightEmitPartial;
    private AudioSource _audioSource;
    public AudioClip clip;

    private void Start()
    {
        isActive = false;
        _lightEmitPartial = false;
        _emitLight = emitLight.GetComponent<Light>();
        _spotLight1 = spotLight1.GetComponent<Light>();
        _spotLight2 = spotLight2.GetComponent<Light>();
        _audioSource = this.GetComponent<AudioSource>();
    }

    public void ActivateRedLight(bool activation)
    {
        isActive = activation;
        if (isActive)
            _audioSource.PlayOneShot(clip);
    }

    private void Update()
    {
        if (isActive)
        {
            _emitLight.enabled = true;
            _spotLight1.enabled = true;
            _spotLight2.enabled = true;
            if (_emitLight.intensity >= 7)
                _lightEmitPartial = false;
            else if (_emitLight.intensity <= 1)
                _lightEmitPartial = true;

            if (_lightEmitPartial == true)
                _emitLight.intensity += Time.deltaTime * 10;
            else
                _emitLight.intensity -= Time.deltaTime * 10;

            spotLight1.transform.RotateAround(spotLight1.transform.position, Vector3.forward, 180 * Time.deltaTime);
            spotLight2.transform.RotateAround(spotLight2.transform.position, Vector3.forward, 180 * Time.deltaTime);
        }
        else
        {
            _emitLight.enabled = false;
            _spotLight1.enabled = false;
            _spotLight2.enabled = false;
        }
    }
}
