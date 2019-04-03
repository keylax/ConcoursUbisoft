using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayButton : MonoBehaviour
{
    public GameObject arrowTarget;
    public GameObject button;
    public Sprite _buttonA;
    public Sprite _buttonB;
    public Sprite _buttonX;
    public Sprite _buttonY;
    public int height = 40;
    private ButtonEnum _buttonAsked;
    private Image _buttonPicture;

    public enum ButtonEnum
    {
        A,
        B,
        X,
        Y
    }

    public void Init(GameObject askedTarget, ButtonEnum buttonAsked)
    {
        arrowTarget = askedTarget;
        _buttonAsked = buttonAsked;
        _buttonPicture = button.GetComponent<Image>();
        SetPicture();
    }

    private void Start()
    {
        _buttonPicture = button.GetComponent<Image>();
    }

    private void Update()
    {
        if (!arrowTarget || StaticObjects.CameraController == null) return;

        Vector3 screenPos = StaticObjects.CameraController.Camera.WorldToScreenPoint(arrowTarget.transform.position);
        screenPos.y += height;
        button.transform.position = screenPos;
    }

    private void SetPicture()
    {
        _buttonPicture.enabled = true;

        switch (_buttonAsked)
        {
            case ButtonEnum.A:
                _buttonPicture.sprite = _buttonA;
                break;
            case ButtonEnum.B:
                _buttonPicture.sprite = _buttonB;
                break;
            case ButtonEnum.X:
                _buttonPicture.sprite = _buttonX;
                break;
            case ButtonEnum.Y:
                _buttonPicture.sprite = _buttonY;
                break;
        }

        _buttonPicture.enabled = false;
    }

    private void InitializeTargetPointing()
    {
        if (arrowTarget != null && StaticObjects.CameraController != null)
        {
            Vector3 screenPos = StaticObjects.CameraController.Camera.WorldToScreenPoint(arrowTarget.transform.position);
            screenPos.y += height;
            button.transform.position = screenPos;
        }
    }

    public void ActivateButton(bool activation)
    {
        if (_buttonPicture)
            _buttonPicture.enabled = activation;
    }
}