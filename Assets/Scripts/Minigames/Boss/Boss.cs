using System.Collections.Generic;
using UnityEngine;

enum Phase
{
    ARMS, BODY_SLAM, LASER
}

public class Boss : MonoBehaviour
{
    private List<Player> _players;
    private Phase _currentPhase;
    private bool _fightInProgress;
    private Vector3 _initialPosition;
    private bool _isBossDestroyed;
    private List<Rigidbody> _playersRB;
    [SerializeField] private BossSounds _bossSounds;
    [Header("Phases")]
    [SerializeField] private PhaseOne _phaseOne;
    [SerializeField] private PhaseTwo _phaseTwo;
    [SerializeField] private PhaseThree _phaseThree;

    [Header("Map positions")]
    [SerializeField] private Transform _leftWallPos;
    [SerializeField] private Transform _rightWallPos;
    [SerializeField] private Transform _floorPos;
    [SerializeField] private Transform _ceilingWallPos;

    private void Start()
    {
        _fightInProgress = false;
        _players = StaticObjects.GameController.GetPlayers();
        _playersRB = new List<Rigidbody>();
        GetRbs(_players);
        _phaseOne.InitPhase(_leftWallPos, _rightWallPos, _floorPos, _ceilingWallPos, _bossSounds);
        _phaseTwo.InitPhase(_leftWallPos, _rightWallPos, _floorPos, _ceilingWallPos, _bossSounds);
        _phaseThree.InitPhase(_leftWallPos, _rightWallPos, _floorPos, _ceilingWallPos, _bossSounds);
        _phaseTwo.enabled = false;
        _phaseOne.enabled = false;
        _phaseThree.enabled = false;
    }

    private void Update()
    {
        if (_fightInProgress)
        {
            ClampPlayers(_playersRB);
        }

        CheckPhase();
    }

    private void ClampPlayers(List<Rigidbody> playersRB)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].transform.position.x < _leftWallPos.position.x - 1)
            {
                _players[i].transform.position = new Vector3(_leftWallPos.position.x, _players[i].transform.position.y, _players[i].transform.position.z);
            }

            if (_players[i].transform.position.y < _floorPos.position.y - 1)
            {
                _players[i].transform.position = new Vector3(_players[i].transform.position.x, _floorPos.position.y, _players[i].transform.position.z);
            }

            if (_players[i].transform.position.y > _ceilingWallPos.position.y + 1)
            {
                _players[i].transform.position = new Vector3(_players[i].transform.position.x, _ceilingWallPos.position.y, _players[i].transform.position.z);
            }
            if (playersRB[i].velocity.y >= 20f)
            {
                playersRB[i].velocity = new Vector3(playersRB[i].velocity.x, 20f, playersRB[i].velocity.z);
            }
        }
    }

    private void CheckPhase()
    {
        switch (_currentPhase)
        {
            case Phase.ARMS:
                if (_phaseOne.IsLeftArmDestroyed() && _phaseOne.IsRightArmDestroyed())
                    PhaseTransition();
                break;
            case Phase.BODY_SLAM:
                if (_phaseTwo.IsBodyDead())
                    PhaseTransition();
                break;
            case Phase.LASER:
                if (_phaseThree.IsReactorDestroyed() && !_isBossDestroyed)
                    PhaseTransition();
                break;
        }
    }

    private void PhaseTransition()
    {
        switch (_currentPhase)
        {
            case Phase.ARMS:
                StaticObjects.SpeakerLinesManager.PlayBossTransition();
                _phaseOne.enabled = false;
                _phaseOne.SetPhaseOneReady(false);
                _currentPhase = Phase.BODY_SLAM;
                _phaseTwo.enabled = true;
                _phaseTwo.SetPhaseTwoReady(true);
                break;
            case Phase.BODY_SLAM:
                _phaseTwo.enabled = false;
                _phaseTwo.SetPhaseTwoReady(false);
                _currentPhase = Phase.LASER;
                _phaseThree.enabled = true;
                _phaseThree.SetPhaseThreeReady(true);
                break;
            case Phase.LASER:
                Debug.Log("Game is over");
                StopFight();
                break;
        }
    }

    public bool IsBossDestroyed()
    {
        return _isBossDestroyed;
    }

    private void GetRbs(List<Player> players)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            _playersRB.Add(_players[i].GetComponent<Rigidbody>());
        }
    }

    public void StartFight()
    {
        _phaseOne.enabled = true;
        _currentPhase = Phase.ARMS;
        _phaseOne.SetPhaseOneReady(true);
        _fightInProgress = true;
    }

    public void StopFight()
    {
        _phaseOne.enabled = false;
        _phaseOne.SetPhaseOneReady(false);
        _phaseTwo.enabled = false;
        _phaseTwo.SetPhaseTwoReady(false);
        _phaseThree.enabled = false;
        _phaseThree.SetPhaseThreeReady(false);
        _fightInProgress = false;
        _isBossDestroyed = true;
    }
}
