using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    private enum LightsType
    {
        UnitLight,
        PlatformLight
    }

    public float timeExpected = 10f;
    private bool _isActive;
    private float _originalMultiplier;
    private WaitForSeconds _durationEffect;
    private bool _searchForLightsCoroutineState;
    private readonly WaitForSeconds _searchForLights;
    private List<Light> _platformLights;
    private List<Light> _playersAndIALights;

    private Lights()
    {
        _searchForLights = new WaitForSeconds(3f);
        _platformLights = new List<Light>();
        _playersAndIALights = new List<Light>();
    }
    
    // DEBUG
    public bool manualStart = false;
    //

    public void Init(float durationEffect)
    {
        _durationEffect = new WaitForSeconds(durationEffect);
    }

    private void Start()
    {
        _isActive = false;
        _originalMultiplier = RenderSettings.ambientIntensity;
        _searchForLightsCoroutineState = false;
        // Debug, delete if Init function is used
        _durationEffect = new WaitForSeconds(timeExpected);
        //
    }

    private void Update()
    {
        if (manualStart)
        {
            manualStart = false;
            _isActive = true;
            ActivateLightEffect();
        }
        if (_isActive && _searchForLightsCoroutineState == false)
            StartCoroutine(SearchForLights());
    }

    public void ActivateLightEffect()
    {
        if (_isActive == false)
            return;
        _isActive = true;
        GetLights(LightsType.UnitLight);
        GetLights(LightsType.PlatformLight);
        SwitchLights(LightsType.UnitLight, true);
        SwitchLights(LightsType.PlatformLight, false);
        RenderSettings.ambientIntensity = 0f;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        StartCoroutine(ActivationTime());
    }

    private void DesactivateLightEffect()
    {
        _isActive = false;
        SwitchLights(LightsType.UnitLight, false);
        SwitchLights(LightsType.PlatformLight, true);
        RenderSettings.ambientIntensity = _originalMultiplier;
        Camera.main.clearFlags = CameraClearFlags.Skybox;
    }

    private IEnumerator SearchForLights()
    {
        _searchForLightsCoroutineState = true;
        yield return _searchForLights;
        GetLights(LightsType.UnitLight);
        GetLights(LightsType.PlatformLight);
        if (_isActive)
        {
            SwitchLights(LightsType.UnitLight, true);
            SwitchLights(LightsType.PlatformLight, false);
        }
        _searchForLightsCoroutineState = false;
    }

    private IEnumerator ActivationTime()
    {
        yield return _durationEffect;
        DesactivateLightEffect();
    }

    private void SwitchLights(LightsType type, bool activation)
    {
        if (type == LightsType.UnitLight)
            foreach (Light lightObject in _playersAndIALights)
                lightObject.enabled = activation;
        else if (type == LightsType.PlatformLight)
            foreach (Light lightObject in _platformLights)
                lightObject.enabled = activation;
    }

    private void GetLights(LightsType asked)
    {
        if (asked == LightsType.UnitLight)
            _playersAndIALights = new List<Light>();
        else
            _platformLights = new List<Light>();

        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag(asked.ToString());
        foreach (GameObject lightObject in lightObjects)
        {
            if (asked == LightsType.UnitLight)
                _playersAndIALights.Add(lightObject.GetComponent<Light>());
            else
                _platformLights.Add(lightObject.GetComponent<Light>());
        }
    }
}
