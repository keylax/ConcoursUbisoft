using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransitionController : MonoBehaviour
{
    [SerializeField] private float countdownDuration = 3;
    [SerializeField] private float startAutoScrollTime = 1;
    [SerializeField] private float timeBeforeIncrementingFirstPoints = 0.5f;

    private bool _isFromPlatformingSection;
    private MovingObstacleManager _obstacleToMovePostCountdown;

    public bool platforming;
    public bool platformingWon = false;
    private Player _ahead;
    private float _lastPassTime;

    private void Awake()
    {
        StaticObjects.MapTransitionController = this;
    }

    private void Update()
    {
        if (platforming && !StaticObjects.GameController.isPaused)
        {
            Player old_ahead = _ahead;
            Player second_player = GetAheadPlayer(); // this function update _ahead player
            if (old_ahead != _ahead)
                _lastPassTime = Time.time;
            else if (Time.time - _lastPassTime > timeBeforeIncrementingFirstPoints && !platformingWon)
            {
                _ahead.YouAreFirstInPlatforming(GetDistance(second_player));
                _ahead.firstInPlatforming = true;
            }
        }
    }

    private IEnumerator SayPlatformingMidLine()
    {
        yield return new WaitForSeconds(25);
        if (StaticObjects.SpeakerLinesManager && !platformingWon)
            StaticObjects.SpeakerLinesManager.PlayPlatformingChirps();
    }

    private float GetDistance(Player second_player)
    {
        if (!second_player)
            return 1;
        Camera gameCamera = StaticObjects.CameraController.Camera;
        return (gameCamera.WorldToViewportPoint(_ahead.transform.position) - gameCamera.WorldToViewportPoint(second_player.transform.position)).x;
    }

    private Player GetAheadPlayer() // return second player
    {
        Player ahead = null;
        Player second = null;
        List<Player> players = StaticObjects.GameController.GetPlayers();

        foreach (Player player in players)
        {
            player.firstInPlatforming = false; // reset firstInPlatforming for everyone and put it to true for the first one in update
            if (!ahead || ahead.transform.position.x < player.transform.position.x)
                ahead = player;
        }
        foreach (Player player in players)
        {
            if (!ahead.Equals(player) && (!second || second.transform.position.x < player.transform.position.x))
                second = player;
        }
        _ahead = ahead;
        return second;
    }

    public void SetNextMapTransition(bool isFromPlatformingSection, MovingObstacleManager obstacleToMovePostCountdown)
    {
        _isFromPlatformingSection = isFromPlatformingSection;
        _obstacleToMovePostCountdown = obstacleToMovePostCountdown;
    }

    public void DoTransition()
    {
        if (_isFromPlatformingSection)
        {
            platforming = false;
            StaticObjects.MapCreationController.ChangeCurrentMap();
            _obstacleToMovePostCountdown.MoveWall();
            if (!StaticObjects.MapCreationController.IsBossMap())
            {
                if (StaticObjects.SpeakerLinesManager) // minigame entrance line
                    StaticObjects.SpeakerLinesManager.AddToQueue(
                        StaticObjects.MapCreationController.GetActiveMinigame().entranceLine);
                StartCoroutine(WaitImpatiencyMinigame());   
            }
            else
            {
                StaticObjects.SpeakerLinesManager.PlayBossEntrance();
            }
        }
        else
        {
            _ahead = null;
            platformingWon = false;
            StartCoroutine(Countdown());
            StartCoroutine(SayPlatformingMidLine());
            if (StaticObjects.SpeakerLinesManager)
                StaticObjects.SpeakerLinesManager.PlayPlatformingEntrance();
        }
    }

    private IEnumerator WaitImpatiencyMinigame()
    {
        yield return new WaitForSeconds(25);
        if (StaticObjects.SpeakerLinesManager && !StaticObjects.MapCreationController.GetActiveMinigame().GetIsActive())
            StaticObjects.SpeakerLinesManager.PlayPressurePlate(false);
    }

    private IEnumerator Countdown()
    {
        float currentCountdown = countdownDuration;

        while (currentCountdown > 0)
        {
            yield return null;

            currentCountdown -= Time.deltaTime;

            if (_isFromPlatformingSection || startAutoScrollTime < currentCountdown) continue;

            _isFromPlatformingSection = true;
            StaticObjects.CameraController.SetAutoScroll(true);
        }

        platforming = true;
        StaticObjects.MapCreationController.LaunchCurrentPlatformingEvent();
        StaticObjects.MapCreationController.ChangeCurrentMap();
        _obstacleToMovePostCountdown.MoveWall();
        StaticObjects.MapCreationController.StartPoll();
        StaticObjects.CameraController.EnableAheadPlayerCheck();
    }
}
