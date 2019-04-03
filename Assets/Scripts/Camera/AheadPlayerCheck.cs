using System.Collections.Generic;
using UnityEngine;

public class AheadPlayerCheck : MonoBehaviour
{
    private List<UnitMovement> _playerMovements;
    private UnitMovement _aheadPlayer;
    private CameraController _cameraController;

    private void Start()
    {
        _cameraController = StaticObjects.CameraController;
        _playerMovements = new List<UnitMovement>();
        foreach (Player player in StaticObjects.GameController.GetPlayers())
        {
            _playerMovements.Add(player.UnitMovement);
        }
    }

    private void OnDisable()
    {
        if (!_aheadPlayer) return;

        _aheadPlayer.SetIsFollowedByCamera(false);
        _aheadPlayer = null;
    }

    private void Update()
    {
        foreach (UnitMovement playerMovement in _playerMovements)
        {
            bool isAheadPlayer = playerMovement == _aheadPlayer;
            if (!isAheadPlayer && playerMovement.transform.position.x > transform.position.x)
            {
                if (!_aheadPlayer)
                {
                    _cameraController.SetAutoScroll(false);
                    _aheadPlayer = playerMovement;
                    _aheadPlayer.SetIsFollowedByCamera(true);
                }
                else if (playerMovement.transform.position.x > _aheadPlayer.transform.position.x)
                {
                    _aheadPlayer.SetIsFollowedByCamera(false);
                    _aheadPlayer = playerMovement;
                    _aheadPlayer.SetIsFollowedByCamera(true);
                }
            }
            else if (isAheadPlayer && playerMovement.transform.position.x < transform.position.x)
            {
                _cameraController.SetAutoScroll(true);
                _aheadPlayer.SetIsFollowedByCamera(false);
                _aheadPlayer = null;
            }
        }
    }

    public bool IsFollowingAPlayer()
    {
        return _aheadPlayer;
    }
}
