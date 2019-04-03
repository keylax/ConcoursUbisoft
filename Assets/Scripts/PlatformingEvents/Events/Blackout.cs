using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackout : PlatformingEvent
{
    [SerializeField] private float timeLightsOff = 5;
    [SerializeField] private float timeLightsOn = 8;
    [SerializeField] private Light gameLight;

    private WaitForSeconds _delayLightsOff;
    private WaitForSeconds _delayLightsOn;
    private float _originalMultiplier;
    private Color _originalAmbientLight;

    private readonly List<Light> _playerLights;
    private readonly List<Light> _platformingLights;
    private Camera _camera;

    private Blackout()
    {
        _name = "Blackout";

        _playerLights = new List<Light>();
        _platformingLights = new List<Light>();
    }

    protected override void Awake()
    {
        base.Awake();

        _originalMultiplier = RenderSettings.ambientIntensity;
        _delayLightsOff = new WaitForSeconds(timeLightsOff);
        _delayLightsOn = new WaitForSeconds(timeLightsOn);
    }

    protected override void Start()
    {
        base.Start();

        _camera = StaticObjects.CameraController.Camera;
        GetLights();
    }

    private void GetLights()
    {
        foreach (Player player in StaticObjects.GameController.GetPlayers())
        {
            Light playerLight = player.GetComponentInChildren<Light>(true);
            _playerLights.Add(playerLight);
            playerLight.enabled = false;
        }
        
        _platformingLights.Add(gameLight);
    }

    public override void StartPlatformingEvent()
    {
        base.StartPlatformingEvent();
        
        StartCoroutine(LightsOnAndOff());
    }

    public override void StopPlatformingEvent()
    {
        base.StopPlatformingEvent();
        
        StopAllCoroutines();
        TurnLightsOn();
    }

    private IEnumerator LightsOnAndOff()
    {
        while (_isActive)
        {
            yield return _delayLightsOn;

            TurnLightsOff();

            yield return _delayLightsOff;

            TurnLightsOn();
        }
    }

    public void TurnLightsOff()
    {
        SetPlayerLights(true);
        SetPlatformingLights(false);
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.ambientLight = Color.black;
        _camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void TurnLightsOn()
    {
        SetPlayerLights(false);
        SetPlatformingLights(true);
        RenderSettings.ambientIntensity = _originalMultiplier;
        RenderSettings.ambientLight = _originalAmbientLight;
        _camera.clearFlags = CameraClearFlags.Skybox;
    }

    private void SetPlayerLights(bool enable)
    {
        foreach (Light playerLight in _playerLights)
            playerLight.enabled = enable;
    }

    private void SetPlatformingLights(bool enable)
    {
        foreach (Light platformingLight in _platformingLights)
            platformingLight.enabled = enable;
    }
}
