using UnityEngine;

public class PlatformingGoalManager : MonoBehaviour
{
    [SerializeField] private GameObject triggerGroupToActivateWhenGoalIsPickedUp;

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        collider.GetComponent<Player>().YouWonPlatforming();
        gameObject.SetActive(false);
        triggerGroupToActivateWhenGoalIsPickedUp.SetActive(true);
        StaticObjects.MapCreationController.StopCurrentPlatformingEvent();
        StaticObjects.MapTransitionController.platformingWon = true;
        StaticObjects.CrowdController.EndPoll();
        if (StaticObjects.SpeakerLinesManager)
            StaticObjects.SpeakerLinesManager.PlayPlatformingEnd();
    }
}
