using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerGroupManager : MonoBehaviour
{
    protected readonly List<TriggerManager> _triggerManagers;
    
    protected int _triggersPressedCount;
    protected int _triggerManagersCount;

    protected TriggerGroupManager()
    {
        _triggerManagers = new List<TriggerManager>();
    }
    
    protected virtual void Awake()
    {
        TriggerManager[] triggerManagers = GetComponentsInChildren<TriggerManager>(true);
        for (int i = 0; i < StaticObjects.GameController.GetPlayers().Count; i++)
        {
            triggerManagers[i].gameObject.SetActive(true);
            _triggerManagers.Add(triggerManagers[i]);
        }
        
        _triggerManagersCount = _triggerManagers.Count;
    }

    protected void UpdateTriggersPressedCount(bool isPressed)
    {
        _triggersPressedCount += isPressed ? 1 : -1;
    }
    
    public abstract void TriggerIsPressed(bool isPressed);
}
