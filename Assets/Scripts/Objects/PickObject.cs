using UnityEngine;

public class PickObject : MonoBehaviour
{
    public ObjectName itemName;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private GameObject jetpackObject;
    [SerializeField] private GameObject dashObject;
    [SerializeField] private GameObject baseballBatObject;
    [SerializeField] private GameObject teddyBearObject;
    [SerializeField] private GameObject remoteControllObject;
    [SerializeField] private DisplayButton _displayButton;

    private bool _canBePickedUp;
    private bool _canPlayVoiceLine;

    public enum ObjectName
    {
        Jetpack,
        Dash,
        RemoteControl,
        TeddyBear,
        Hammer
    }

    public void Init(ObjectName newItemName, bool canPlayVoiceLine)
    {
        itemName = newItemName;
        _canPlayVoiceLine = canPlayVoiceLine;
        Assign3DModel();
    }

    private void Start()
    {
        _displayButton.Init(gameObject, DisplayButton.ButtonEnum.Y);
        _canBePickedUp = true;
    }

    private void Update()
    {
        Rotate3DModel();
    }

    private void Assign3DModel()
    {
        switch (itemName)
        {
            case ObjectName.Jetpack:
                jetpackObject.SetActive(true);
                break;
            case ObjectName.Dash:
                dashObject.SetActive(true);
                break;
            case ObjectName.Hammer:
                baseballBatObject.SetActive(true);
                break;
            case ObjectName.TeddyBear:
                teddyBearObject.SetActive(true);
                break;
            case ObjectName.RemoteControl:
                remoteControllObject.SetActive(true);
                break;
        }
    }

    private void Rotate3DModel()
    {
        Quaternion localRotation = transform.parent.localRotation;
        transform.parent.Rotate(localRotation.x, localRotation.y + rotationSpeed, localRotation.z);
    }

    public void DestroyObject()
    {
        Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _displayButton.ActivateButton(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            _displayButton.ActivateButton(false);
    }

    public bool CanPickUp()
    {
        return _canBePickedUp;
    }

    public void LockPickUp()
    {
        _canBePickedUp = false;
    }

    public bool CanPlayVoiceLine()
    {
        return _canPlayVoiceLine;
    }

    public void LockPlayVoiceLine()
    {
        _canPlayVoiceLine = false;
    }
}
