using System.Collections.Generic;
using UnityEngine;

public class PlayersInNextRoomCheck : MonoBehaviour
{
    [SerializeField] private MovingObstacleManager obstacleToMove;
    [SerializeField] private GameObject hitboxToActivate;

    private bool _wallIsMoving;
    private int _playersInMinigameRoomCount;
    private List<Player> _players;

    private void Start()
    {
        _players = StaticObjects.GameController.GetPlayers();
    }

    private void Update()
    {
        bool allOut = true;
        foreach (Player player in _players)
            if (player.transform.position.x <= transform.position.x + transform.localScale.x * 0.5f)
                allOut = false;
        if (!allOut || _wallIsMoving)
            return;

        _wallIsMoving = true;
        hitboxToActivate.SetActive(true);
        obstacleToMove.MoveWall();
        gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider collider)
    {
        bool closeWall = true;
        foreach (Player player in _players)
        {
            if (player.transform.position.x <= transform.position.x)
            {
                closeWall = false;
            }
        }

        if (!closeWall || _wallIsMoving) return;

        _wallIsMoving = true;
        hitboxToActivate.SetActive(true);
        obstacleToMove.MoveWall();
        gameObject.SetActive(false);
    }
}
