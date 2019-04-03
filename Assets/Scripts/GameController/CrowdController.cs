using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    private void Awake()
    {
        StaticObjects.CrowdController = this;
    }

    public void StartPoll(List<string> votingOptions, int duration)
    {
        StaticObjects.CrowdClient.PollCrowd(votingOptions, duration, PollResult);
    }

    public void EndPoll()
    {
        StaticObjects.CrowdClient.EndPoll();
    }

    private void PollResult(int pollResult)
    {
        StaticObjects.MapCreationController.ReceivePollResult(pollResult);
        if (StaticObjects.SpeakerLinesManager && StaticObjects.SpeakerLinesManager.hub)
        {
            StaticObjects.SpeakerLinesManager.AddToQueue(
                StaticObjects.SpeakerLinesManager.hubLines[1]);
            StaticObjects.SpeakerLinesManager.StopHub();
        }
    }
}
