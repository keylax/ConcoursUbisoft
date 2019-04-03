using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MinigamePong : MiniGame
{
    private PongBall _ball;
    private Player _bottomleftPlayer;
    private Player _topLeftPlayer;
    private Player _bottomLeftPlayer;
    private Player _topRightPlayer;
    private Player _bottomRightPlayer;
    private List<GameObject> _triggers;

    [Header("General components settings")] [SerializeField]
    private GameObject _triggersParent;

    [SerializeField] private GameObject _verticalSeparation;
    [SerializeField] public GameObject horizontalSeparation;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _leftLimit;
    [SerializeField] private Transform _rightLimit;
    [SerializeField] private Transform _topLimit;
    [SerializeField] private Transform _bottomLimit;
    [SerializeField] private float ratio = 0.5f;

    [Header("Visual settings")] [SerializeField]
    private Material _redMaterial;

    [SerializeField] private Material _blueMaterial;
    [SerializeField] private GameObject _topLeftWallVisual;
    [SerializeField] private GameObject _topRightWallVisual;
    [SerializeField] private GameObject _bottomLeftWallVisual;
    [SerializeField] private GameObject _bottomRightWallVisual;
    [SerializeField] private GameObject _tempPlatforms;
    [SerializeField] private GameObject _topPlatforms;

    private MinigamePong()
    {
        _name = "Pong";
    }

    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
    }

    protected override void Start()
    {
        base.Start();
        _triggers = new List<GameObject>();
    }

    public override void SpawnMiniGame(Vector3 pos)
    {
        base.SpawnMiniGame(pos);
        if (_players.Count == 2)
        {
            horizontalSeparation.SetActive(true);
            _tempPlatforms.SetActive(false);
            _topPlatforms.SetActive(false);
            _topLimit.transform.position = horizontalSeparation.transform.position;
            Vector3 basePosition = _verticalSeparation.transform.GetChild(0).position;
            _verticalSeparation.transform.GetChild(0).position = new Vector3(basePosition.x, basePosition.y - 7.5f, basePosition.z);
        }
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        for (int i = 0; i != _triggersParent.transform.childCount; ++i)
            _triggers.Add(_triggersParent.transform.GetChild(i).gameObject);
        SetMapActive(true);
        InitBall();
    }

    public override void StopMiniGame()
    {
        base.StopMiniGame();

        if (_ball) 
            Destroy(_ball.gameObject);
        SetMapActive(false);
    }

    private void SetMapActive(bool setActive)
    {
        if (_players.Count < 2 && _isActive)
        {
            StopMiniGame();
            return;
        }

        _leftLimit.gameObject.SetActive(setActive);
        _rightLimit.gameObject.SetActive(setActive);
        _topLimit.gameObject.SetActive(setActive);
        _bottomLimit.gameObject.SetActive(setActive);
        horizontalSeparation.SetActive(setActive);
        _verticalSeparation.SetActive(setActive);
        if (_players.Count > 2)
            _tempPlatforms.SetActive(!setActive);

        _bottomLeftPlayer = _triggers[0].GetComponent<StartMinigameTriggerManager>().player;
        if (_bottomLeftPlayer)
            _bottomLeftWallVisual.GetComponent<MeshRenderer>().material.color = _bottomLeftPlayer.color;
        _bottomRightPlayer = _triggers[1].GetComponent<StartMinigameTriggerManager>().player;
        if (_bottomRightPlayer)
            _bottomRightWallVisual.GetComponent<MeshRenderer>().material.color = _bottomRightPlayer.color;
        _topLeftPlayer = _triggers[2].GetComponent<StartMinigameTriggerManager>().player;
        if (_topLeftPlayer)
            _topLeftWallVisual.GetComponent<MeshRenderer>().material.color = _topLeftPlayer.color;
        else
            _topLeftWallVisual.GetComponent<MeshRenderer>().material.color = Color.white;
        _topRightPlayer = _triggers[3].GetComponent<StartMinigameTriggerManager>().player;
        if (_topRightPlayer)
            _topRightWallVisual.GetComponent<MeshRenderer>().material.color = _topRightPlayer.color;
        else
            _topRightWallVisual.GetComponent<MeshRenderer>().material.color = Color.white;
    }

    private void InitBall()
    {
        _ball = Instantiate(_ballPrefab, transform.position, Quaternion.identity).GetComponent<PongBall>();
        Vector3 basePosition = _verticalSeparation.transform.GetChild(0).position;
        _ball.Initialise(_leftLimit, _rightLimit, _topLimit, _bottomLimit, this, new Vector2(basePosition.x, basePosition.y), _bottomLeftPlayer, _bottomRightPlayer,
            _topLeftPlayer, _topRightPlayer);
        StartCoroutine(_ball.Reset());
    }

    public override void IncrementScore(Player player)
    {
        if (player == null)
            return;
        base.IncrementScore(player);
    }

    public void DecrementScore(Player player)
    {
        if (player == null)
            return;
        player.YouUnScoredMiniGamePoint(ratio);
        for (int i = 0; i != _players.Count; ++i)
        {
            if (_players[i].Equals(player) && _scores[i] > 0)
                _scores[i]--;
        }
    }

    public void IncrementAllScoreExcept(Player loserOfThePoint)
    {
        if (loserOfThePoint == null)
            return;
        for (int i = 0; i != _players.Count; ++i)
        {
            if (!_players[i].Equals(loserOfThePoint))
                base.IncrementScore(_players[i]);
        }
    }
}
