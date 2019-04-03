using UnityEngine;

public abstract class TriggerManager : MonoBehaviour
{
    protected TriggerGroupManager _triggerGroupManager;
    protected int _pressedCount;

    protected void Awake()
    {
        _triggerGroupManager = GetComponentInParent<TriggerGroupManager>();
    }

    protected void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        PressTrigger(collider);
    }

    protected void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        UnpressTrigger(collider);
    }

    protected abstract void PressTrigger(Collider collider);
    protected abstract void UnpressTrigger(Collider collider);
}
