using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpeakerLinesManager : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _busy;
    private List<VoiceLine> queue;
    public List<int> hubLineOrder;

    public List<VoiceLine> hubLines;
    public bool hub = true;
    
    public List<VoiceLine> votesLines;
    public VoiceLine bossEntranceLine;
    public VoiceLine bossPhaseThreeLine;
    public List<VoiceLine> bossRandomLines;
    public List<VoiceLine> bossTransitionLines;

    private readonly WaitForSeconds _delayBusy;
    
    private SpeakerLinesManager()
    {
        _delayBusy = new WaitForSeconds(0.1f);
    }
    
    public void PlayPressurePlate(bool platforming)
    {
        int index = (platforming ? 1 : 0);
        AddToQueue(pressurePlateLines[index]);
    }
    public List<VoiceLine> pressurePlateLines;
    
    public void PlayMGAlmostOver()
    {
        int index = Random.Range(0, mGAlmostOverLines.Count);
        AddToQueue(mGAlmostOverLines[index]);
    }
    public List<VoiceLine> mGAlmostOverLines;
    
    public void PlayMGStart()
    {
        int index = Random.Range(0, mGStartLines.Count);
        AddToQueue(mGStartLines[index]);
    }
    public List<VoiceLine> mGStartLines;
    
    public void PlayMGWin(bool tie)
    {
        int index = (tie ? mGWinLines.Count - 1 : Random.Range(0, mGWinLines.Count - 1));
        AddToQueue(mGWinLines[index]);
    }
    public List<VoiceLine> mGWinLines;
    
    public void PlayPlatformingChirps()
    {
        int index = Random.Range(0, platformingChirpsLines.Count);
        AddToQueue(platformingChirpsLines[index]);
    }
    public List<VoiceLine> platformingChirpsLines;
    
    public void PlayPlatformingEnd()
    {
        int index = Random.Range(0, platformingEndLines.Count);
        AddToQueue(platformingEndLines[index]);
    }
    public List<VoiceLine> platformingEndLines;


    public void PlayPlatformingEntrance()
    {
        int index = Random.Range(0, 3);
        int index2 = Random.Range(4, platformingEntranceLines.Count);
        AddToQueue(platformingEntranceLines[index]);
        AddToQueue(platformingEntranceLines[index2]);
    }
    public List<VoiceLine> platformingEntranceLines;

    public void PlayBossPanicLine()
    {
        int index = Random.Range(0, bossRandomLines.Count);
        AddToQueue(bossRandomLines[index]);
    }

    public void PlayBossTransition()
    {
        int index = Random.Range(0, bossTransitionLines.Count);
        AddToQueue(bossTransitionLines[index]);
    }

    public void PlayBossEntrance()
    {
        AddToQueue(bossEntranceLine);
    }

    public void PlayBossLaserPhase()
    {
        AddToQueue(bossPhaseThreeLine);
    }

    private void PlayHub()
    {
        AddToQueue(hubLines[hubLineOrder[0]]);
        hubLineOrder.RemoveAt(0);
        if (hubLineOrder.Count == 0)
            CancelInvoke();
    }

    public void StopHub()
    {
        hub = false;
        hubLineOrder.Clear();
        CancelInvoke();
    }

    private void Awake()
    {
        StaticObjects.SpeakerLinesManager = this;
    }
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        queue = new List<VoiceLine>();
        InvokeRepeating(nameof(PlayHub), 1, 3);
    }

    private void Update()
    {
        if (queue.Count != 0)
            StartCoroutine(PlayLine());
    }

    public void AddToQueue(VoiceLine line)
    {
        List<VoiceLine> toRemove = new List<VoiceLine>();
        foreach (VoiceLine l in queue)
        {
            if (l.importance > line.importance)
                toRemove.Add(l);
        }
        queue.RemoveAll(e => toRemove.Contains(e));
        queue.Add(line);
    }
    
    public void AddToQueue(List<VoiceLine> lines)
    {
        int index = Random.Range(0, lines.Count);
        AddToQueue(lines[index]);
    }

    private IEnumerator PlayLine()
    {
        while (_busy || queue.Count == 0)
            yield return _delayBusy;
        
        VoiceLine line = queue[0];
        _audioSource.PlayOneShot(line.line);
        StartCoroutine(StaticObjects.UIManager.PrintSubtitles(line.subtitles, line.line.length));
        _busy = true;
        queue.Remove(line);
        yield return new WaitForSeconds(line.line.length);
        StaticObjects.UIManager.HideSubtitles();
        _busy = false;
    }
}
