using System.Collections.Generic;
using Audio;
using UnityEngine;

public class PlatformingEvent : MonoBehaviour
{
    protected string _name;
    protected bool _isActive;

    public Sprite logo;

    [Header("Audio lines")] 
    public List<VoiceLine> pickEventLine;
    public List<VoiceLine> happeningLine;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        
    }

    public virtual void SetPlatformingEvent(MapManager mapManager)
    {
        Debug.Log("Set Platforming Event: " + _name);
    }

    public virtual void StartPlatformingEvent()
    {
        Debug.Log("Started Platforming Event: " + _name);
        
        _isActive = true;
        if (StaticObjects.SpeakerLinesManager)
            StaticObjects.SpeakerLinesManager.AddToQueue(pickEventLine);
    }

    public virtual void StopPlatformingEvent()
    {
        Debug.Log("Stopped Platforming Event: " + _name);

        _isActive = false;
    }

    public virtual void DestroyPlatformingEvent()
    {
        Debug.Log("Destroyed Platforming Event: " + _name);
    }

    public string GetName()
    {
        return _name;
    }
}
