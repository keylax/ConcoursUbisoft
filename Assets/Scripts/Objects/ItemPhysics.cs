using UnityEngine;

public class ItemPhysics : MonoBehaviour
{
    private Rigidbody _rigidbody;

    public delegate void OnCollisionHandler(Collision collision);
    public event OnCollisionHandler OnCollision;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        _rigidbody.velocity = Vector3.zero;
        OnCollision?.Invoke(other);
    }
}
