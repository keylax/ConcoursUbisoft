using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectable : MonoBehaviour
{
    private SphereCollider _collider;
    private float _lifeTime;


    public delegate void PickupAction(Transform instigator);
    public event PickupAction OnPickup;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.enabled = false;
    }

    private void Update()
    {
        if (!_collider.enabled)
            return;

        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0)
        {
            _collider.enabled = false;
            Destroy(gameObject);
        }
    }

    public void SetActive(bool active, float lifetime)
    {
        _lifeTime = lifetime;
        _collider.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Utility.PLAYER_TAG) && OnPickup != null)
        {
            _collider.enabled = false;
            OnPickup(other.transform);
            Destroy(gameObject);
        }
    }
}
