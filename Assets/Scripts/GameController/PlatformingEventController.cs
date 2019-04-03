using System.Collections.Generic;
using UnityEngine;

public class PlatformingEventController : MonoBehaviour
{
    private readonly List<PlatformingEvent> _platformingEvents;

    private PlatformingEventController()
    {
        _platformingEvents = new List<PlatformingEvent>();
    }
    
    private void Awake()
    {
        StaticObjects.PlatformingEventController = this;
        foreach (PlatformingEvent platformingEvent in GetComponents<PlatformingEvent>())
        {
            _platformingEvents.Add(platformingEvent);
        }
    }

    public List<PlatformingEvent> GetPlatformingEvents()
    {
        return _platformingEvents;
    }
}
