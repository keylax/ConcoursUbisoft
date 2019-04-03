using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public GameObject arrowTarget;
    public GameObject arrow;
    public bool _isActive;
    private Image _arrowPic;
    private bool _animWay;
    private float _minHeight;
    private float _maxHeight;
    private float _speed;
    private bool _placeOrigin;
    private Renderer _visibility;
    private float _flyingOffset = 0;
    private float _maxOffset = 15f;
    private bool _goingUp = false;

    public void Init(GameObject askedTarget)
    {
        arrowTarget = askedTarget;
    }

    void Start()
    {
        _isActive = true;
        _arrowPic = arrow.GetComponent<Image>();
        _animWay = false;
        _speed = 22f;
        _placeOrigin = false;
        _visibility = arrowTarget.GetComponent<Renderer>();
    }

    public void SetTarget(GameObject obj)
    {
        arrowTarget = obj;
    }

    private void InitializeTargetPointing()
    {
        if (arrowTarget != null)
        {
            Vector3 screenPos = StaticObjects.CameraController.Camera.WorldToScreenPoint(arrowTarget.transform.position);
            screenPos.y += 100;
            arrow.transform.position = screenPos;
        }
    }

    public void ActiveArrow(bool activation)
    {
        _isActive = activation;
        if (_placeOrigin == false)
        {
            // InitializeTargetPointing();
            _placeOrigin = true;
            _flyingOffset = 0;
        }
    }

    void Update()
    {

        if (_visibility.isVisible)
            ActiveArrow(true);

        if (arrowTarget != null && _isActive == true && _placeOrigin)
        {
            Vector2 targetPosition =
                StaticObjects.CameraController.Camera.WorldToScreenPoint(arrowTarget.transform.position);
            targetPosition.y += 50;
            _flyingOffset += (_goingUp ? Time.deltaTime * _speed : Time.deltaTime * _speed * -1);
            if (_flyingOffset > _maxOffset || _flyingOffset < -_maxOffset)
                _goingUp = !_goingUp;
            arrow.transform.position = new Vector3(targetPosition.x, targetPosition.y + _flyingOffset, targetPosition.y);
            _arrowPic.enabled = true;
        }
        else
            _arrowPic.enabled = false;
    }
}
