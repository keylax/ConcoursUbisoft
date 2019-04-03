using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] private GameObject leverObject;
    [SerializeField] private MovingPlatform _movingPlatformToActivate;
    [SerializeField] private MovingObstacleManager movingObstacleManager;
    [SerializeField] private DisplayButton _displayButton;
    
    private AudioSource _audioSource;
    private bool _contact;
    public AudioClip clip;

    private MeshRenderer _mesh;
    private bool _alreadyHit;

    private void Start()
    {
        _contact = false;
        _displayButton.Init(gameObject, DisplayButton.ButtonEnum.X);
    }

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
        _audioSource = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        _displayButton.ActivateButton(_contact);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyHit) return;
        
        if (other.gameObject.CompareTag("Player"))
            _contact = true;

        if (!other.CompareTag("Weapon")) return;

        _contact = false;
        _alreadyHit = true;
        ChangeLeverObjectRotation();
        _audioSource.PlayOneShot(clip);
        if (_movingPlatformToActivate)
            _movingPlatformToActivate.EnablePlatformMovement();
        else if (movingObstacleManager)
            movingObstacleManager.MoveWall();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _contact = false;
    }

    private void ChangeLeverObjectRotation()
    {
        Quaternion pos = leverObject.transform.rotation;
        pos.y = -pos.y;
        leverObject.transform.rotation = pos;
    }
}
