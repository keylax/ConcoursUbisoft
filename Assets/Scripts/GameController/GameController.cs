using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private int numberOfPlayers = 2;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int firstPollDuration = 30;

    public readonly List<Color> colors;
    private readonly List<Dictionary<string, KeyCode>> _inputKeySets;
    public readonly List<Player> _players;

    public bool isPaused = false;
    private readonly WaitForSeconds _startGameDelay;
    public bool playerPaused;
    public bool _ending;

    private GameController()
    {
        StaticObjects.GameController = this;
        _players = new List<Player>();

        colors = new List<Color>()
        {
            new Color(0.8157f, 0.3725f, 0.4157f),
            // Color.red,
            // Color.blue,
            new Color(0.3333f, 0.5647f, 0.8235f),
            //Color.magenta,
            new Color(0.82f, 0.625f, 0.128f),
            //Color.cyan,
            new Color(0.355f, 0.820f, 0.285f),
        };

        _inputKeySets = new List<Dictionary<string, KeyCode>>
        {
            new Dictionary<string, KeyCode>
            {
                {"UP", KeyCode.Space},
                {"DOWN", KeyCode.S},
                {"LEFT", KeyCode.A},
                {"RIGHT", KeyCode.D},
                {"ATTACK", KeyCode.E},
                {"ITEM", KeyCode.R},
                {"INTERACTION", KeyCode.Q}
            },
            new Dictionary<string, KeyCode>
            {
                {"UP", KeyCode.RightShift},
                {"DOWN", KeyCode.K},
                {"LEFT", KeyCode.J},
                {"RIGHT", KeyCode.L},
                {"ATTACK", KeyCode.O},
                {"ITEM", KeyCode.P},
                {"INTERACTION", KeyCode.U}
            },
            new Dictionary<string, KeyCode>
            {
                {"UP", KeyCode.UpArrow},
                {"DOWN", KeyCode.DownArrow},
                {"LEFT", KeyCode.LeftArrow},
                {"RIGHT", KeyCode.RightArrow},
                {"ATTACK", KeyCode.End},
                {"ITEM", KeyCode.RightControl},
                {"INTERACTION", KeyCode.KeypadEnter}
            },
        };

        _startGameDelay = new WaitForSeconds(0.5f);
    }

    private void Awake()
    {
        StaticObjects.GameController = this;
        if (StaticObjects.PlayerNumber != 0)
            numberOfPlayers = StaticObjects.PlayerNumber;
        StaticObjects.PlayerNumber = numberOfPlayers;
        SpawnPlayers();
    }

    private void Start()
    {
        StartCoroutine(StartGameAfterSmallDelay());
        _ending = false;
    }

    private IEnumerator StartGameAfterSmallDelay()
    {
        yield return _startGameDelay;

        StartGame();
    }

    public void Ending(bool val)
    {
        _ending = val;
    }

    private void SpawnPlayers()
    {
        string[] controllers = Input.GetJoystickNames();
        if (numberOfPlayers > (controllers.Length - controllers.Count(e => e == "")) + _inputKeySets.Count)
        {
            Debug.LogError("Not enough controllers connected to play with " + numberOfPlayers + " players. Please restart the game when there are enough.");
            return;
        }
        for (int i = 0; i < numberOfPlayers; i++)
        {
            SpawnPlayer(i);
        }
    }

    private void SpawnPlayer(int i, bool useSpawnPoint = true)
    {
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x += i * 1.5f;
        Player player = Instantiate(playerPrefab, useSpawnPoint ? spawnPosition : _players[0].transform.position, Quaternion.identity).GetComponent<Player>();
        _players.Add(player);
        player.ID = _players.Count;
        player.SetColor(colors[i]);
        string[] controllers = Input.GetJoystickNames();
        if (controllers == null)
            player.PlayerInput.SetInputKeys(_inputKeySets[i]);
        else if (controllers.Length < player.ID || controllers[player.ID - 1] == "")
            player.PlayerInput.SetInputKeys(_inputKeySets[i - controllers.Length + controllers.Count(e => e == "")]);
    }

    private void StartGame()
    {
        StaticObjects.MapCreationController.StartPoll(firstPollDuration);
    }

    public void SummonPause(bool wantedByPlayer)
    {
        if (wantedByPlayer && !playerPaused && isPaused) return;

        if (wantedByPlayer)
            playerPaused = true;

        if (isPaused)
        {
            isPaused = false;
            playerPaused = false;
            Time.timeScale = 1f;
            StaticObjects.UIManager.pauseUI.UnPause();
        }
        else
        {
            isPaused = true;
            Time.timeScale = 0f;
            StaticObjects.UIManager.pauseUI.Pause(!wantedByPlayer);
        }
    }

    public List<Player> GetPlayers()
    {
        return _players;
    }
}
