using UnityEngine;

public class StartMinigameTriggerManager : TriggerManager
{
    public Player player;
    
    protected override void PressTrigger(Collider collider)
    {
        if (++_pressedCount != 1) return;
        
        player = collider.gameObject.GetComponent<Player>();
        _triggerGroupManager.TriggerIsPressed(true);
    }

    protected override void UnpressTrigger(Collider collider)
    {
        if (--_pressedCount != 0) return;
        
        player = null;
        _triggerGroupManager.TriggerIsPressed(false);
    }
}
