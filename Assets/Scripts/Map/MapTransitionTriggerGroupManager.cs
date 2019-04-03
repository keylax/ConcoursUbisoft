using System.Collections;
using UnityEngine;

public class MapTransitionTriggerGroupManager : TriggerGroupManager
{
    [SerializeField] private MovingObstacleManager obstacleToMovePostCountdown;
    [SerializeField] private GameObject[] invisibleWallsToActivate;
    [SerializeField] private Transform cameraFocusPointOnEvent;
    [SerializeField] private bool isFromPlatformingSection;
    [SerializeField] private DoorRedLight[] redLights;
    [SerializeField] private float minusDistanceWhenChangingToPlatformingSection = 12;
    [SerializeField] private float timeBeforeForcingPressurePlateLine = 20;

    private float _endOfPlatformingXPosition;
    private Vector3 _platformingOffset;
    private WaitForSeconds _delayPressurePlate;
    private IEnumerator _pressurePlateCoroutine;

    protected override void Awake()
    {
        _platformingOffset = Vector3.right * minusDistanceWhenChangingToPlatformingSection;
        _delayPressurePlate = new WaitForSeconds(timeBeforeForcingPressurePlateLine);

        _triggerManagers.Add(GetComponentInChildren<TriggerManager>());
        _triggerManagersCount = StaticObjects.GameController.GetPlayers().Count;
        _pressurePlateCoroutine = WaitImpatiency();
        StartCoroutine(_pressurePlateCoroutine);
    }

    private IEnumerator WaitImpatiency()
    {
        yield return _delayPressurePlate;

        if (StaticObjects.SpeakerLinesManager)
            StaticObjects.SpeakerLinesManager.PlayPressurePlate(!StaticObjects.MapTransitionController.platforming);
    }

    public void SetCameraAutoScrollLimit(float platformingCeilingWidth)
    {
        _endOfPlatformingXPosition = platformingCeilingWidth + cameraFocusPointOnEvent.position.x;
    }

    public override void TriggerIsPressed(bool isPressed)
    {
        UpdateTriggersPressedCount(isPressed);
        if (_triggersPressedCount != _triggerManagersCount) return;

        if (_pressurePlateCoroutine != null)
            StopCoroutine(_pressurePlateCoroutine);

        gameObject.SetActive(false);
        foreach (GameObject wall in invisibleWallsToActivate)
        {
            wall.SetActive(true);
        }

        foreach (DoorRedLight redLight in redLights)
        {
            redLight.ActivateRedLight(true);
        }

        Vector3 focusPosition = cameraFocusPointOnEvent.position - (isFromPlatformingSection ? Vector3.zero : _platformingOffset);
        StaticObjects.CameraController.StartNextSection(focusPosition, isFromPlatformingSection, obstacleToMovePostCountdown, _endOfPlatformingXPosition);
    }
}