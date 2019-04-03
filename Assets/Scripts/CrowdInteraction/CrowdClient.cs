using SocketIO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class CrowdClient : MonoBehaviour
{
    private const int RoomCodeLength = 3;
    private const string RoomCode = "LAVAL";

    private SocketIOComponent _socket;
    [SerializeField] private string _roomName = "LAVAL";
    private bool _roomActive = false;
    private NetworkReachability _connectionStateAtTimeOfPoll;
    private int[] _voteStatus { get; set; }
    private Action<int> ResultNotifyMethod { get; set; }
    private WaitForSeconds _connectionCheckInterval = new WaitForSeconds(30);
    [SerializeField] private GameObject _connectionProblemUIElement;
    public Action<int[]> VoteStatusNotifyMethod { get; set; }

    public Poll currentPoll;

    private void Awake()
    {
        StaticObjects.CrowdClient = this;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(StaticObjects.RoomName))
            _roomName = StaticObjects.RoomName;
        else if (_roomName == "")
        {
            _roomName = GenerateRoomCode(4);
            Debug.Log("Room name: " + _roomName);
        }
        _socket = GetComponent<SocketIOComponent>();
        _socket.On("updateVotesGame", DefaultVoteStatus);
        _socket.On("result", DefaultNotifyMethod);
        _socket.On("open", OnConnect);
        StaticObjects.UIManager.pollUIManager.SetRoomName(_roomName);
    }

    public void OnConnect(SocketIOEvent ev)
    {
        Dictionary<string, string> roomDetails = new Dictionary<string, string>();
        roomDetails["roomCode"] = _roomName;
        _socket.Emit("gameSocketJoin", new JSONObject(roomDetails));

        if (!_roomActive)
        {
            CreateRoom(roomDetails);
            _roomActive = true;
        }
    }

    private IEnumerator EndPollAfterDuration(int duration, Poll poll)
    {
        yield return new WaitForSeconds(duration);
        if (poll == currentPoll)
            EndPoll();
    }

    public void PollCrowd(List<string> options, int duration, Action<int> ResultCallBack)
    {
        ResultNotifyMethod = ResultCallBack;
        currentPoll = new Poll(_roomName, duration, options);
        if (duration > 0)
            StartCoroutine(EndPollAfterDuration(duration, currentPoll));
        _connectionStateAtTimeOfPoll = Application.internetReachability;
        if (_connectionStateAtTimeOfPoll == NetworkReachability.NotReachable)
        {
            _connectionProblemUIElement.SetActive(true);
        } else
        {
            _connectionProblemUIElement.SetActive(false);
        }
        _socket.Emit("poll", currentPoll.toJson());
    }

    public void EndPoll()
    {
        Dictionary<string, string> roomDetails = new Dictionary<string, string>();
        roomDetails["roomCode"] = _roomName;
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (_connectionStateAtTimeOfPoll == NetworkReachability.NotReachable)
            {
                if (currentPoll != null)
                {
                    int winningVote = _voteStatus != null ? _voteStatus.ToList().IndexOf(_voteStatus.Max()) : Random.Range(0, currentPoll.options.Count);
                    ResultNotifyMethod(winningVote);
                }
            }
            else
            {
                _socket.Emit("endPoll", new JSONObject(roomDetails));
            }
        }
        else
        {
            if (currentPoll != null)
            {
                int winningVote = _voteStatus != null ? _voteStatus.ToList().IndexOf(_voteStatus.Max()) : Random.Range(0, currentPoll.options.Count);
                ResultNotifyMethod(winningVote);
            }
        }
        _voteStatus = null;
        currentPoll = null;
    }

    private void DefaultVoteStatus(SocketIOEvent eventData)
    {
        string voteArrayString = eventData.data["votes"].ToString();
        string votesString = voteArrayString.TrimStart('[').TrimEnd(']');
        _voteStatus = Array.ConvertAll(votesString.Split(','), int.Parse);

        VoteStatusNotifyMethod(_voteStatus);
    }

    private string GenerateRoomCode(int length)
    {
        System.Random random = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private void CloseRoom()
    {
        Dictionary<string, string> roomDetails = new Dictionary<string, string>();
        roomDetails["roomCode"] = _roomName;
        _socket.Emit("closeRoom", new JSONObject(roomDetails));
        _roomActive = false;
    }

    private void CreateRoom(Dictionary<string, string> roomDetails)
    {
        _socket.Emit("newRoom", new JSONObject(roomDetails));
    }

    private void DefaultNotifyMethod(SocketIOEvent eventData)
    {
        _voteStatus = null;
        currentPoll = null;
        int chosenIndex = int.Parse(eventData.data["result"].ToString());
        ResultNotifyMethod(chosenIndex);
    }
    
    private void OnDestroy()
    {
        CloseRoom();
    }

    private void OnApplicationQuit()
    {
        CloseRoom();
    }
}
