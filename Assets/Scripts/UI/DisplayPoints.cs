using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPoints : MonoBehaviour
{
    [SerializeField] private Text displayText;
    [SerializeField] private float speedFlying = 15;
    [SerializeField] private int normalScoreFontSize = 20;
    [SerializeField] private int bigScoreFontSize = 40;
    [SerializeField] private float textInitialDistanceOverPlayerGoingUp = 50;
    [SerializeField] private float textInitialDistanceOverPlayerGoingDown = 50;
    [SerializeField] private float bigPointsThreshold = 1000;

    private Transform target;
    private Color _color;
    private float _points;
    private readonly WaitForSeconds _displayTime;
    private float _currentFlyingOffset;
    private bool _isActive;

    private Camera _camera;

    private DisplayPoints()
    {
        _displayTime = new WaitForSeconds(1f);
    }

    public void SetDisplayText(GameObject askedTarget, float points)
    {
        _camera = StaticObjects.CameraController.Camera;

        target = askedTarget.transform;
        _color = target.GetComponent<Player>().color;
        displayText.color = _color;
        _points = points;

        displayText.text = Mathf.Round(_points).ToString();
        if (_points > 0)
            displayText.text = "+" + displayText.text;
        displayText.fontSize = _points >= bigPointsThreshold ? bigScoreFontSize : normalScoreFontSize;

        _isActive = true;
        StartCoroutine(_points >= 0 ? MakeTextFly(textInitialDistanceOverPlayerGoingUp) : MakeTextFly(textInitialDistanceOverPlayerGoingDown, -1));
        StartCoroutine(DisplayText());
    }

    private IEnumerator DisplayText()
    {
        yield return _displayTime;

        _isActive = false;
        Destroy(gameObject);
    }

    private void SetPosition(float initialPositionOverPlayer, float offset = 0)
    {
        Vector3 screenPos = _camera.WorldToScreenPoint(target.position);
        screenPos.y += initialPositionOverPlayer + offset;
        displayText.transform.position = screenPos;
    }

    private IEnumerator MakeTextFly(float initialPositionOverPlayer, int direction = 1)
    {
        float previousPosition = initialPositionOverPlayer;
        float offset = Time.deltaTime * speedFlying * direction;
        float alphaValue = -1.25f;

        SetPosition(previousPosition, offset);
        previousPosition += offset;
        
        yield return null;

        offset = Time.deltaTime * speedFlying * direction;
        SetPosition(previousPosition, offset);
        previousPosition += offset;
        alphaValue += 0.02f;
        displayText.color = new Color(_color.r, _color.g, _color.b, _color.a - alphaValue);

        displayText.enabled = true;
        while (_isActive)
        {
            yield return null;

            offset = Time.deltaTime * speedFlying * direction;
            SetPosition(previousPosition, offset);
            previousPosition += offset;
            alphaValue += 0.02f;
            displayText.color = new Color(_color.r, _color.g, _color.b, _color.a - alphaValue);
        }
    }
}
