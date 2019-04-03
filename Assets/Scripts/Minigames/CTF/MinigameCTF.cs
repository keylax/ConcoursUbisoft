using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameCTF : MiniGame
{
    //[SerializeField] private Flag flagComponent;
    [SerializeField] private Base baseComponent;
    [SerializeField] private GameObject FlagPrefab;
    [SerializeField] private float ratio = 0.75f;

    // MG Parameters
    public int pointsToWin = 3;
    public int numberOfFlags = 2;
    
    private FlagSpawner[] _spawnpoints;
    private Flag[] _flags;

    private MinigameCTF()
    {
        _name = "Capture the Skulls";
    }
    
    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
    }

    private void Update()
    {
        if (_scores == null)
            return;
        int totalScore = 0;
        bool someoneHasFlag = false;
        
        foreach (int score in _scores)
            totalScore += score;
        foreach (Flag flag in _flags)
        {
            if (flag.Instigator != null)
            {
                someoneHasFlag = true;
                break;
            }
        }

        if (totalScore == 0 && someoneHasFlag)
            baseComponent.SetArrowActive(true);
        else
            baseComponent.SetArrowActive(false);
    }

    protected override void Start()
    {
        base.Start();
        
        _spawnpoints = GetComponentsInChildren<FlagSpawner>();
        
        // Instanciate Flags
        _flags = new Flag[numberOfFlags];
        for (int i = 0; i < numberOfFlags; i++)
        {
            GameObject flagGo = Instantiate(FlagPrefab, Vector3.back * 99f, Quaternion.identity, transform); // derrière la camera
            flagGo.transform.rotation = Quaternion.Euler(90, 0, 0);
            _flags[i] = flagGo.GetComponentInChildren<Flag>();
            _flags[i].MgCtrl = this;
            _flags[i].FlagId = i;
        }
        baseComponent.OnDropFlag += OnFlagDropped;
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        
        // Activate and spawn flags
        for (int i = 0; i < _flags.Length; i++)
        {
            StartCoroutine(SpawnFlag(i, 1.5f));
        }
        
        // Activate Base
        baseComponent.SetActive(true);
    }

    public override void StopMiniGame()
    {   
        base.StopMiniGame();

        for (int i = 0; i < _flags.Length; i++)
        {
            _flags[i].transform.parent.parent = transform;
            _flags[i].SetActive(false, Vector3.back * 99f);
        }
    }

    public override void DestroyMiniGame()
    {
        for (int i = 0; i < _flags.Length; i++)
        {
            Destroy(_flags[i].transform.parent.gameObject);
        }
        
        base.DestroyMiniGame();
    }

    private IEnumerator SpawnFlag(int flagId, float delay = 0f)
    {
        bool shouldSpawn = true;
        int i = Random.Range(0, _spawnpoints.Length - 1);
        
        yield return new WaitForSeconds(delay);
        
        if (_isActive)
        {
            while (shouldSpawn)
            {
                if (Time.time - _spawnpoints[i].TimeSinceLastSpawn > 10f && !Physics.CheckSphere(_spawnpoints[i].transform.position, 0.4f))
                {
                    _flags[flagId].SetActive(true, _spawnpoints[i].transform.position);
                    _flags[flagId].specialEffect.SetActive(true);
                    _spawnpoints[i].TimeSinceLastSpawn = Time.time;
                    shouldSpawn = false;
                }
                else
                {
                    i = (i + 1) % (_spawnpoints.Length - 1);
                    yield return null;
                }
            }
        }
    }
    
    public void OnFlagDropped(Transform instigator)
    {
        for (int i = 0; i < _flags.Length; i++)
        {
            if (_flags[i].Instigator == instigator)
            {
                // Score
                Player player = instigator.GetComponent<Player>();
                IncrementScore(player);
                
                // Detach and respawn flag
                _flags[i].transform.parent.parent = transform;
                _flags[i].SetActive(false, Vector3.back * 99f);
                StartCoroutine(SpawnFlag(i, 2f));
            }
        }
    }

    public override void IncrementScore(Player player)
    {
        player._audioSource.PlayOneShot(player.pointsSound);
        base.IncrementScore(player);
        for (int index = 0; index < _players.Count; index++)
        {
            if (player == _players[index])
            {
                if (_scores[index] >= pointsToWin)
                    StopMiniGame();
            }
        }
    }

    public bool CanPickup(Transform instigator, int flagId)
    {
        if (!instigator)
            return false;
        
        foreach (Flag flag in _flags)
        {
            if (flag.FlagId != flagId && flag.Instigator == instigator)
                return false;
        }
        return true;
    }
}
