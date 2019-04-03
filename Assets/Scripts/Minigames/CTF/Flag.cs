using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Flag : MonoBehaviour
{
    public int FlagId { get; set; }
    public MinigameCTF MgCtrl { get; set; }

    private CapsuleCollider _collider;
    private MeshRenderer _renderer;
    private Rigidbody _rigidbody;

    public Transform Instigator { get; protected set; }
    private UnitMovement _unitMovement;
    private Vector3 _pickupPositionOffset;
    private bool _throwing = false;
    public GameObject specialEffect;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _collider.enabled = false;
        _pickupPositionOffset = new Vector3(0f, .75f, 0f);
        _rigidbody = GetComponentInParent<Rigidbody>();
        specialEffect.SetActive(true);

        GetComponentInParent<ItemPhysics>().OnCollision += OnCollision;
    }

    private void Update()
    {
        // specialEffect.SetActive(false);
        if (Instigator && !_unitMovement.HasControl())
            Detach();
    }

    public void SetActive(bool active, Vector3 position)
    {
        if (!active)
            Instigator = null;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        transform.parent.position = position;
        _collider.enabled = active;
    }

    public void Pickup(Transform instigator)
    {
        if (!MgCtrl.CanPickup(instigator, FlagId)) return;

        Player player = instigator.GetComponent<Player>();
        _unitMovement = instigator.GetComponent<UnitMovement>();

        if (!_unitMovement.HasControl()) return;

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _collider.enabled = false;
        Instigator = instigator;
        specialEffect.SetActive(false);

        transform.parent.SetParent(player.rightHand.transform);
        transform.parent.localPosition = new Vector3(0, 0, 0);
        // transform.position = instigator.position + _pickupPositionOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_throwing && other != null && other.CompareTag(Utility.PLAYER_TAG))
            Pickup(other.transform);
    }

    private void Detach()
    {
        _throwing = true;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.AddForce(500 * (_unitMovement.IsFacingRight ? -1 : 1), 500, 0);
        _collider.enabled = true;
        Instigator = null;
        _unitMovement = null;
        specialEffect.SetActive(true);

        transform.parent.SetParent(null);
        // transform.position -= _pickupPositionOffset;
    }

    private void OnCollision(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            _throwing = false;
    }
}