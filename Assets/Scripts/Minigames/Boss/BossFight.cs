using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight : MonoBehaviour
{
    private bool _gameIsOver;
    private EndGame _endgame;
    [SerializeField] private Boss _boss;
    [SerializeField] private GameObject _endGameObj;
    private void Start()
    {
        _endgame = _endGameObj.GetComponent<EndGame>();
    }

    private void Update()
    {
        if (_boss.IsBossDestroyed() && !_gameIsOver)
            DetermineGameWinner();

        if (Input.GetKeyDown(KeyCode.Alpha5) && !_gameIsOver)
            StartBoss();

        if (Input.GetKeyDown(KeyCode.Alpha6))
            DestroyBoss();
    }

    public void StartBoss()
    {
        _boss.StartFight();
    }

    public void DestroyBoss()
    {
        _boss.StopFight();
    }

    private void DetermineGameWinner()
    {
        Player playerWithMostPoints = null;

        foreach (Player player in StaticObjects.GameController.GetPlayers())
        {
            if (!playerWithMostPoints)
            {
                playerWithMostPoints = player;
                continue;
            }

            if (player.score > playerWithMostPoints.score)
            {
                playerWithMostPoints = player;
            }
            else if (player.score == playerWithMostPoints.score)
            {
                // What do we do?
            }
        }
        _endgame.Launch();
        _gameIsOver = true;
    }
}
