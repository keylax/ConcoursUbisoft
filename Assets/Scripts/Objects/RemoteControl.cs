using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteControl : MonoBehaviour
{
    [SerializeField] private float damage = 0f;

    public bool isOnCooldown;
    public float cooldownDuration;
    public float useTime;

    private Player _player;

    private WaitForSeconds _cooldownDuration;
    private WaitForSeconds _durationRemoteControl;
    private int _id;
    private Player _itemUser;

    private Camera _camera;
    private List<Player> _players;

    public void Init(float cooldownRemoteControl, float durationRemoteControl)
    {
        _cooldownDuration = new WaitForSeconds(cooldownRemoteControl);
        _durationRemoteControl = new WaitForSeconds(durationRemoteControl);
        cooldownDuration = cooldownRemoteControl;
        useTime = -1;
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        _id = _player.ID;
        ActiveSpecialEffectsRemoteControl(false);

        _camera = StaticObjects.CameraController.Camera;
        _players = StaticObjects.GameController.GetPlayers();
    }

    private void Update()
    {
        if (!_itemUser) return;


        Vector3 screenPos = _camera.WorldToScreenPoint(_player.Rigidbody.transform.position);
        screenPos.y += 2;
        //_player.TakeAHit(_itemUser, damage);
        //_itemUser.TouchAHit(_player, damage * 2);
        _player.specialEffectCracklingPictureRemoteControl.gameObject.transform.position = screenPos;

        if (!isOnCooldown && !_player.remoteControlSound.isPlaying)
            _player.remoteControlSound.Play();
        if (isOnCooldown)
            _player.remoteControlSound.Stop();
    }

    public bool ActivateRemoteControl()
    {
        if (isOnCooldown) return false;

        _player.UnitMovement.alreadyUsedAnObject = true;
        foreach (Player player in _players)
        {
            if (player.ID == _id) continue;

            player.RemoteControl.ActivateFog();
            player.RemoteControl._itemUser = _player;
        }

        StartCoroutine(Cooldown());
        return true;
    }

    private void ActivateFog()
    {
        StartCoroutine(InUse());
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        useTime = Time.time;

        yield return _cooldownDuration;

        isOnCooldown = false;
        useTime = -1;
    }

    private IEnumerator InUse()
    {
        ActiveSpecialEffectsRemoteControl(true);
        _player.PlayerInput.nerfedMovement = true;

        yield return _durationRemoteControl;

        _player.remoteControlSound.Stop();
        _itemUser = null;
        _player.PlayerInput.nerfedMovement = false;
        ActiveSpecialEffectsRemoteControl(false);
    }

    private void ActiveSpecialEffectsRemoteControl(bool active)
    {
        _player.specialEffectCracklingPictureRemoteControl.SetActive(active);
        /*ParticleSystem[] childrenParticleSytems;
        childrenParticleSytems = _unit.specialEffectRemoteControl.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem child in childrenParticleSytems)
        {
            if (active)
                child.Play();
            else
                child.Stop();
        }*/
    }
}
