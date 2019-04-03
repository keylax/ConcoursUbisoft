using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LastZombieStandingMinigame : MiniGame
{
    public float DropPlatformCooldown = 4f;
    public float CooldownModifier = 1.5f;
    [SerializeField] private float ratio = 0.75f;
    [SerializeField] private GameObject _lavaEffect;
    public GameObject tempPlatforms;

    private FloorTrigger _floor;
    private List<DisapearingPlatform> _platforms;
    private List<DisapearingPlatform> _platformsTmp;
    private List<Player> _playersStillStanding;
    private List<Player> _precedentPlayersStillStanding;
    
    private LastZombieStandingMinigame()
    {
        _name = "Last Zombie Standing";
    }
    
    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
    }
    
    protected override void Start()
    {
        base.Start();

        DisapearingPlatform[] platforms = GetComponentsInChildren<DisapearingPlatform>();
        _platforms = platforms.ToList();
        _platformsTmp = new List<DisapearingPlatform>(_platforms);
        _floor = GetComponentInChildren<FloorTrigger>();
        _floor.OnFallInLava += OnFallInLava;
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        _lavaEffect.SetActive(true);
        for (int i = 0; i != tempPlatforms.transform.childCount; ++i)
            tempPlatforms.transform.GetChild(i).gameObject.SetActive(false);
        _playersStillStanding = new List<Player>(_players);
        _precedentPlayersStillStanding = new List<Player>(_players);

        foreach (DisapearingPlatform platform in _platforms)
        {
            platform.MGStart(this);
        }
        
        Drop();
        InvokeRepeating(nameof(CheckEnd), 0, 0.3f);
    }

    private void CheckEnd()
    {
        if (!_isActive)
            return;
        if (_precedentPlayersStillStanding.Count != _playersStillStanding.Count)
        {
            if (_playersStillStanding.Count == 0)
                StopMiniGame();
            else
            {
                foreach (Player player in _playersStillStanding)
                    IncrementScore(player);
            }
        }
        else if (_playersStillStanding.Count == 1)
        {
            IncrementScore(_playersStillStanding[0]);
            StopMiniGame();
        }
        else if (_playersStillStanding.Count == 0)
            StopMiniGame();
        _precedentPlayersStillStanding = _playersStillStanding;
    }

    private void Drop()
    {
        if (_platformsTmp.Count <= 0)
            return;
        
        int platformIndex = Random.Range(0, _platformsTmp.Count);
        _platformsTmp[platformIndex].Drop();
        _platformsTmp.RemoveAt(platformIndex);
    }
    
    public void OnPlatformIsAboutToFall()
    {
        if (_isActive)
            Drop();
    }

    public override void StopMiniGame()
    {
        _lavaEffect.SetActive(false);
        _isActive = false;
        
        for (int i = 0; i != tempPlatforms.transform.childCount; ++i)
            tempPlatforms.transform.GetChild(i).gameObject.SetActive(true);

        foreach (DisapearingPlatform platform in _platforms)
        {
            platform.ResetPosition();
        }
        
        CancelInvoke(nameof(CheckEnd));
        base.StopMiniGame();
    }

    private void OnFallInLava(Player player)
    {
        if (_isActive && _playersStillStanding.Contains(player))
        {
            _playersStillStanding.Remove(player);
        }
    }

    public bool GetActive()
    {
        return _isActive;
    }
}
