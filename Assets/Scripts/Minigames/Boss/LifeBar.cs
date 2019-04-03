using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

public class LifeBar
{
    private float _xOffset;
    private float _yOffset;
    private float _zOffset;
    private float _maxValue;
    private Camera _mainCamera;
    private Slider _lifeBar;
    private float _currentLife;

    public LifeBar(float maxValue, Camera mainCamera, Slider slider,float xOffset, float yOffset, float zOffset)
    {
        _maxValue = maxValue;
        _currentLife = _maxValue;
        _mainCamera = mainCamera;
        _xOffset = xOffset;
        _yOffset = yOffset;
        _zOffset = zOffset;
        _lifeBar = slider;
        _lifeBar.minValue = 0;
        _lifeBar.maxValue = _maxValue;
        _lifeBar.gameObject.SetActive(false);
    }

    public bool IsLifeDepleted(Transform transformToFollow)
    {
        _lifeBar.gameObject.SetActive(true);
        _lifeBar.value = _currentLife;
        Vector3 position = transformToFollow.position;
        position.x += _xOffset;
        position.y += _yOffset;
        position.z += _zOffset;
        if (_mainCamera.transform.position.z - transformToFollow.position.z != 0)
            position.x += -1 * (_mainCamera.transform.position.x - transformToFollow.position.x) / (_mainCamera.transform.position.z - position.z);

        _lifeBar.transform.position = position;
        if (_currentLife <= 0)
        {
            _lifeBar.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public void Hide()
    {
        _lifeBar.gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        _currentLife -= damage;
    }

    public float GetCurrentLife()
    {
        return _currentLife;
    }

    public float GetMaxLife()
    {
        return _maxValue;
    }
}
