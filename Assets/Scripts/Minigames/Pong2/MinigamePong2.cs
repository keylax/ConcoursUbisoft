using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MinigamePong2 : MiniGame
{
    private PongBall2 _ball;
    private Player _leftPlayer;
    private Player _rightPlayer;

    [Header("General components settings")]
    [SerializeField] private GameObject _middleSeparation;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _leftLimit;
    [SerializeField] private Transform _rightLimit;
    [SerializeField] private Transform _topLimit;
    [SerializeField] private Transform _bottomLimit;
    [SerializeField] private float ratio = 1;

    [Header("Visual settings")]
    [SerializeField] private Material _redMaterial;
    [SerializeField] private Material _blueMaterial;
    [SerializeField] private GameObject _leftWallVisual;
    [SerializeField] private GameObject _rightWallVisual;

    private MinigamePong2()
    {
        _name = "Pong";
    }

    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        SetMapActive(true);
        InitBall();
    }

    public override void StopMiniGame()
    {
        base.StopMiniGame();

        Destroy(_ball.gameObject);
        SetMapActive(false);
    }

    private void SetMapActive(bool setActive)
    {
        _leftLimit.gameObject.SetActive(setActive);
        _rightLimit.gameObject.SetActive(setActive);
        _topLimit.gameObject.SetActive(setActive);
        _bottomLimit.gameObject.SetActive(setActive);
        _middleSeparation.SetActive(setActive);

        if (_players[0].transform.position.x < transform.position.x)
        {
            _leftWallVisual.GetComponent<MeshRenderer>().material = _redMaterial;
            _rightWallVisual.GetComponent<MeshRenderer>().material = _blueMaterial;
            _leftPlayer = _players[0];
            _rightPlayer = _players[1];
        } else
        {
            _leftWallVisual.GetComponent<MeshRenderer>().material = _blueMaterial;
            _rightWallVisual.GetComponent<MeshRenderer>().material = _redMaterial;
            _leftPlayer = _players[1];
            _rightPlayer = _players[0];
        }
    }

    private void InitBall()
    {
        _ball = Instantiate(_ballPrefab, transform.position, Quaternion.identity).GetComponent<PongBall2>();
        _ball.Initialise(_leftLimit, _rightLimit, _topLimit, _bottomLimit, this, new Vector2(transform.position.x, transform.position.y), _leftPlayer, _rightPlayer);
        StartCoroutine(_ball.Reset());
    }

    public override void IncrementScore(Player player)
    {
        base.IncrementScore(player);
        StartCoroutine(_ball.Reset());
    }
}
