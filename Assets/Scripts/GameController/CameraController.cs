using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [Range(0, 5)] [SerializeField] private float autoScrollSpeed = .9f;
    [Range(0, 20)] [SerializeField] private int cameraTranslationSpeed = 9;
    [SerializeField] private int platformingCameraZPosition = -20;
    [SerializeField] private int miniGameCameraZPosition = -30;

    private AheadPlayerCheck _aheadPlayerCheck;
    private bool _cameraIsAutoScrolling;
    public bool _cameraIsMovingToNewPosition;
    private bool _cameraWasInPlatformingSection;
    private Vector3 _newCameraPosition;

    private float _endOfPlatformingXPosition;

    public Camera Camera { get; private set; }

    private void Awake()
    {
        StaticObjects.CameraController = this;
        StaticObjects.MainCameraTransform = cameraTransform;
        _aheadPlayerCheck = cameraTransform.GetComponentInChildren<AheadPlayerCheck>();
        _aheadPlayerCheck.enabled = false;

        Camera = cameraTransform.GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (_cameraIsAutoScrolling)
            AutoScroll();

        if ((_cameraIsAutoScrolling || _aheadPlayerCheck.IsFollowingAPlayer()) && cameraTransform.position.x >= _endOfPlatformingXPosition)
        {
            _aheadPlayerCheck.enabled = false;
            SetAutoScroll(false);
            cameraTransform.position = new Vector3(_endOfPlatformingXPosition, cameraTransform.position.y, cameraTransform.position.z);
        }

        if (!_cameraIsMovingToNewPosition) return;

        if (cameraTransform.position != _newCameraPosition)
        {
            cameraTransform.position = Vector3.MoveTowards(cameraTransform.position,
                _newCameraPosition, Time.fixedDeltaTime * cameraTranslationSpeed);
        }
        else
        {
            if (!_cameraWasInPlatformingSection)
            {
                StaticObjects.MapTransitionController.DoTransition();
            }

            _cameraIsMovingToNewPosition = false;
        }
    }

    public void SetAutoScroll(bool autoScroll)
    {
        _cameraIsAutoScrolling = autoScroll;
    }

    public void AutoScroll()
    {
        cameraTransform.position += Vector3.right * autoScrollSpeed * Time.fixedDeltaTime;
    }

    public void StartNextSection(Vector3 focusPosition, bool isFromPlatformingSection, MovingObstacleManager movingObstacleManager, float endOfPlatformingXPosition)
    {
        _cameraWasInPlatformingSection = isFromPlatformingSection;
        StaticObjects.MapTransitionController.SetNextMapTransition(isFromPlatformingSection, movingObstacleManager);
        if (isFromPlatformingSection)
        {
            StaticObjects.MapTransitionController.DoTransition();
            _aheadPlayerCheck.enabled = false;
        }

        _newCameraPosition = new Vector3(focusPosition.x, focusPosition.y, isFromPlatformingSection ? miniGameCameraZPosition : platformingCameraZPosition);
        _endOfPlatformingXPosition = endOfPlatformingXPosition + _newCameraPosition.z;
        _cameraIsMovingToNewPosition = true;
    }

    public IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }

    public void EnableAheadPlayerCheck()
    {
        _aheadPlayerCheck.enabled = true;
    }
}