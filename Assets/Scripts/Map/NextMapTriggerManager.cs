using UnityEngine;

public class NextMapTriggerManager : TriggerManager
{
    protected override void PressTrigger(Collider collider)
    {
        _triggerGroupManager.TriggerIsPressed(true);
    }

    protected override void UnpressTrigger(Collider collider)
    {
        _triggerGroupManager.TriggerIsPressed(false);
    }
}
