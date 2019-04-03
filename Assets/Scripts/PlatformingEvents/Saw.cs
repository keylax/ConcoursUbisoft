using System.Collections;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private Transform sawBase;
    [SerializeField] private Transform sawComponent;
    [SerializeField] private Transform sawElem;
    [SerializeField] private float horizontalSpeed = 3;
    [SerializeField] private float rotatingSpeed = 5;

    private float _leftEdge;
    private float _rightEdge;
    private float _sawSize;

    private bool _isGoingRight;
    private bool _isActive;

    private void Start()
    {
        float half = sawBase.localScale.x * 0.5f;
        float baseX = sawBase.position.x;

        _rightEdge = baseX + half;
        _leftEdge = baseX - half;
        _sawSize = sawComponent.localScale.x * 0.5f + 0.5f;
    }

    private void Update()
    {
        if (!_isActive)
            return;
        sawElem.RotateAround(sawElem.position, Vector3.forward, rotatingSpeed);

        Vector3 pos = sawComponent.position;
        if (pos.x + _sawSize >= _rightEdge || pos.x - _sawSize <= _leftEdge)
            _isGoingRight = !_isGoingRight;
        pos.x += Time.deltaTime * (_isGoingRight ? horizontalSpeed : -horizontalSpeed);
        pos.y += Mathf.Tan(Mathf.Deg2Rad * transform.rotation.eulerAngles.z) * Time.deltaTime * (_isGoingRight ? horizontalSpeed : -horizontalSpeed);;
        sawComponent.position = pos;   
    }

    public void ActivateSaw()
    {
        _isActive = true;
    }

    public void DeactivateSaw()
    {
        _isActive = false;
    }
}
