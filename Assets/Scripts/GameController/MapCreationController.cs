using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapCreationController : MonoBehaviour
{
    [SerializeField] private int numberOfMiniGameRooms = 4;
    [SerializeField] private int numberOfMiniGameRoomsPerPoll = 2;
    [SerializeField] private int numberOfPlatformingEventsPerPoll = 2;
    [SerializeField] private GameObject hubMap;
    [SerializeField] private GameObject miniGameMapSkeleton;
    [SerializeField] private List<GameObject> platformingMaps;
    [SerializeField] private List<GameObject> miniGameMaps;
    [SerializeField] private GameObject bossMap;
    [SerializeField] private Vector3 miniGameHidingSpot;

    private List<String> _miniGameNames;
    private List<String> _platformingEventNames;

    private MiniGame _activeMiniGame;
    private PlatformingEvent _activePlatformingEvent;

    private MapManager _currentMap;
    private int _currentMapId;

    private readonly List<MapManager> _platformingMapManagers;
    private readonly List<MapManager> _miniGameMapManagers;
    private readonly List<MapManager> _maps;
    private readonly List<MiniGame> _miniGames;

    private BossFight _boss;
    private List<PlatformingEvent> _platformingEvents;

    private List<MiniGame> _currentPollMiniGames;
    private List<PlatformingEvent> _currentPollPlatformingEvents;

    private bool _currentVoteIsForMiniGame;

    private bool _activateLastPlatformingEventOnNextTransition;

    private int _nextPlatformingMapId;
    private int _nextMiniGameMapId;
    private int _numberOfPlayedMiniGames;

    private MapCreationController()
    {
        _platformingMapManagers = new List<MapManager>();
        _miniGameMapManagers = new List<MapManager>();
        _maps = new List<MapManager>();
        _miniGames = new List<MiniGame>();

        _nextMiniGameMapId = 1;
    }

    private void Awake()
    {
        StaticObjects.MapCreationController = this;
    }

    private void Start()
    {
        MapManager hubMapManager = Instantiate(hubMap, new Vector3(-9.5f, 6), Quaternion.identity).GetComponent<MapManager>();
        _miniGameMapManagers.Add(hubMapManager);
        _maps.Add(hubMapManager);
        _currentMap = hubMapManager;

        Vector3 nextMapSpawnPoint = hubMapManager.GetNextMapSpawnPoint();
        for (int i = 0; i <= numberOfMiniGameRooms; i++)
        {
            nextMapSpawnPoint = SpawnPlatformingMap(nextMapSpawnPoint);
            _miniGameMapManagers[i].SetCameraAutoScrollLimit(_platformingMapManagers[i]);
            nextMapSpawnPoint = SpawnMiniGameMap(nextMapSpawnPoint);
        }

        foreach (GameObject miniGameMap in miniGameMaps)
        {
            MiniGame miniGame = Instantiate(miniGameMap, miniGameHidingSpot, Quaternion.identity).GetComponent<MiniGame>();
            _miniGames.Add(miniGame);
        }

        _boss = Instantiate(bossMap, _miniGameMapManagers[numberOfMiniGameRooms + 1].transform.position, Quaternion.identity).GetComponent<BossFight>();

        miniGameMaps.Clear();
        bossMap = null;

        _platformingEvents = StaticObjects.PlatformingEventController.GetPlatformingEvents();
    }

    private Vector3 SpawnPlatformingMap(Vector3 nextMapSpawnPoint)
    {
        int mapId = Random.Range(0, platformingMaps.Count);
        MapManager platformingMapManager = Instantiate(platformingMaps[mapId], nextMapSpawnPoint, Quaternion.identity).GetComponent<MapManager>();
        platformingMaps.RemoveAt(mapId);
        _platformingMapManagers.Add(platformingMapManager);
        _maps.Add(platformingMapManager);

        return platformingMapManager.GetNextMapSpawnPoint();
    }

    private Vector3 SpawnMiniGameMap(Vector3 nextMapSpawnPoint)
    {
        MapManager miniGameMapManager = Instantiate(miniGameMapSkeleton, nextMapSpawnPoint, Quaternion.identity).GetComponent<MapManager>();
        _miniGameMapManagers.Add(miniGameMapManager);
        _maps.Add(miniGameMapManager);

        return miniGameMapManager.GetNextMapSpawnPoint();
    }

    public void LaunchCurrentMiniGame()
    {
        _activeMiniGame.StartMiniGame();
    }

    public void LaunchCurrentPlatformingEvent()
    {
        _activePlatformingEvent.StartPlatformingEvent();
    }

    public void StopCurrentPlatformingEvent()
    {
        _activePlatformingEvent.StopPlatformingEvent();
    }

    public void LaunchBoss()
    {
        _boss.StartBoss();
    }

    public void StartPoll(int duration = -1)
    {
        if (_currentVoteIsForMiniGame)
        {
            if (_numberOfPlayedMiniGames == numberOfMiniGameRooms) return;

            _currentPollMiniGames = new List<MiniGame>();
            _miniGameNames = new List<string>();
            for (int i = 0; i < numberOfMiniGameRoomsPerPoll; i++)
            {
                int miniGameId = Random.Range(0, _miniGames.Count);
                MiniGame miniGame = _miniGames[miniGameId];
                _currentPollMiniGames.Add(miniGame);
                _miniGames.Remove(miniGame);
                _miniGameNames.Add(miniGame.GetName());
            }

            StaticObjects.CrowdController.StartPoll(_miniGameNames, duration);
        }
        else if (_platformingEvents.Count > 0)
        {
            if (_platformingEvents.Count > 1)
            {
                _currentPollPlatformingEvents = new List<PlatformingEvent>();
                _platformingEventNames = new List<string>();
                for (int i = 0; i < numberOfPlatformingEventsPerPoll; i++)
                {
                    int platformingEventId = Random.Range(0, _platformingEvents.Count);
                    PlatformingEvent platformingEvent = _platformingEvents[platformingEventId];
                    _currentPollPlatformingEvents.Add(platformingEvent);
                    _platformingEvents.Remove(platformingEvent);
                    _platformingEventNames.Add(platformingEvent.GetName());
                }

                StaticObjects.CrowdController.StartPoll(_platformingEventNames, duration);
            }
            else if (_platformingEvents.Count == 1)
            {
                _currentPollPlatformingEvents = _platformingEvents;
                _activateLastPlatformingEventOnNextTransition = true;
            }
        }
    }

    public void ReceivePollResult(int pollResult)
    {
        Debug.Log("Poll result: " + pollResult);

        if (_currentVoteIsForMiniGame)
        {
            if (_nextMiniGameMapId == _miniGameMapManagers.Count - 1) return;

            _activeMiniGame = _currentPollMiniGames[pollResult];
            _activeMiniGame.SpawnMiniGame(_miniGameMapManagers[_nextMiniGameMapId].transform.position);

            _currentPollMiniGames.RemoveAt(pollResult);
            foreach (MiniGame miniGame in _currentPollMiniGames)
            {
                _miniGames.Add(miniGame);
            }

            _nextMiniGameMapId++;
            _numberOfPlayedMiniGames++;
        }
        else
        {
            _activePlatformingEvent = _currentPollPlatformingEvents[pollResult];
            _activePlatformingEvent.SetPlatformingEvent(_platformingMapManagers[_nextPlatformingMapId]);

            _currentPollPlatformingEvents.RemoveAt(pollResult);
            foreach (PlatformingEvent platformingEvent in _currentPollPlatformingEvents)
            {
                _platformingEvents.Add(platformingEvent);
            }
            _miniGameMapManagers[_nextPlatformingMapId++].ActivateNextMapTriggerGroup();
        }
        _currentVoteIsForMiniGame = !_currentVoteIsForMiniGame;
    }

    public void MoveOnToNextMap()
    {
        if (_activateLastPlatformingEventOnNextTransition)
        {
            _activateLastPlatformingEventOnNextTransition = false;
            ReceivePollResult(0);
        }
        else
        {
            StaticObjects.CrowdController.EndPoll();
        }
    }

    public MiniGame GetActiveMinigame()
    {
        return _activeMiniGame;
    }

    public void ChangeCurrentMap()
    {
        _currentMap = _maps[++_currentMapId];
    }

    public bool IsBossMap()
    {
        return _currentMapId == _maps.Count - 1;
    }

    public MapManager GetCurrentMap()
    {
        return _currentMap;
    }

    public void StopActiveMiniGame()
    {
        if (!_activeMiniGame || !_activeMiniGame.GetIsActive()) return;
        
        _activeMiniGame.StopMiniGame();
    }

    public List<MiniGame> GetCurrentPollMiniGames()
    {
        return _currentPollMiniGames;
    }

    public List<PlatformingEvent> GetCurrentPollPlatformingEvents()
    {
        return _currentPollPlatformingEvents;
    }
}