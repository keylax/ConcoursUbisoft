using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] public float maxMiniGameDuration = 90;

    public Sprite logo;

    [Header("Speaker lines")] 
    public VoiceLine entranceLine;
    public VoiceLine halfLine;

    protected List<Player> _players;
    protected string _name;
    protected bool _isActive;
    public float startTime = -1;

    private float _timeLeft;

    protected float _ratio;
    protected int[] _scores;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
    }

    public virtual void SpawnMiniGame(Vector3 newPosition)
    {
        Debug.Log("MiniGame Spawned: " + _name);

        _players = StaticObjects.GameController.GetPlayers();
        _scores = new int[_players.Count];

        transform.position = newPosition;
    }

    public virtual void StartMiniGame()
    {
        Debug.Log("MiniGame Started: " + _name);

        _timeLeft = maxMiniGameDuration;
        if (StaticObjects.SpeakerLinesManager)
            StaticObjects.SpeakerLinesManager.PlayMGStart();
        _isActive = true;
        startTime = Time.time;
        StartCoroutine(MiniGameTimer());
    }

    public virtual void StopMiniGame()
    {
        Debug.Log("MiniGame Stopped: " + _name);

        _isActive = false;
        bool itemSpawned = SpawnItemForWinner();
        if (StaticObjects.SpeakerLinesManager)
            StaticObjects.SpeakerLinesManager.PlayMGWin(!itemSpawned);
        StaticObjects.MapCreationController.MoveOnToNextMap();
    }

    public virtual void DestroyMiniGame()
    {
        Debug.Log("MiniGame Destroyed: " + _name);

        Destroy(gameObject);
    }

    public string GetName()
    {
        return _name;
    }

    private IEnumerator MiniGameTimer()
    {
        while (_timeLeft > 0 && _isActive)
        {
            yield return null;
            float previousTimeLeft = _timeLeft;
            _timeLeft -= Time.deltaTime;
            if (previousTimeLeft >= (float) maxMiniGameDuration / 2 &&
                _timeLeft < (float) maxMiniGameDuration / 2 && StaticObjects.SpeakerLinesManager)
                StaticObjects.SpeakerLinesManager.AddToQueue(halfLine);
            else if (previousTimeLeft >= (float) maxMiniGameDuration * 0.1 &&
                     _timeLeft < (float) maxMiniGameDuration * 0.1 && StaticObjects.SpeakerLinesManager)
                StaticObjects.SpeakerLinesManager.PlayMGAlmostOver();
        }

        if (_isActive)
            StopMiniGame();
    }

    private bool SpawnItemForWinner()
    {
        Player winner = GetWinner();

        if (!winner) return false;
        
        winner.YouWonMiniGame();

        StaticObjects.ItemPool.SpawnItem(winner.transform.position + Vector3.up);
        return true;
    }

    protected Player GetWinner()
    {
        int maxScoreIndex = 0;
        bool multiple = false;

        for (int i = 1; i != _scores.Length; ++i)
        {
            if (_scores[maxScoreIndex] <= _scores[i])
            {
                if (_scores[maxScoreIndex] == _scores[i])
                    multiple = true;
                else
                    multiple = false;
                maxScoreIndex = i;
            }
        }

        if (multiple || _scores[maxScoreIndex] == 0)
            return null;

        return StaticObjects.GameController.GetPlayers()[maxScoreIndex];
    }

    public virtual void IncrementScore(Player player)
    {
        player.YouScoredMiniGamePoint(_ratio);
        for (int i = 0; i != _players.Count; ++i)
        {
            if (_players[i].Equals(player))
                _scores[i]++;
        }
    }

    public int[] GetScores()
    {
        return _scores;
    }

    public bool GetIsActive()
    {
        return _isActive;
    }
}