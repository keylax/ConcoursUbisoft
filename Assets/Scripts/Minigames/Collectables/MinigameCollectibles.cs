using System.Collections;
using UnityEngine;

public class MinigameCollectibles : MiniGame
{
    private Spawner[] _spawners;
    private IEnumerator _boost;
    
    [SerializeField] private float ratio = 0.1f;

    private MinigameCollectibles()
    {
        _name = "Energy Gathering";
    }

    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
        _boost = Boost();
    }

    protected override void Start()
    {
        base.Start();
        
        _spawners = GetComponentsInChildren<Spawner>();
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        foreach (Spawner spawner in _spawners)
        {
            spawner.OnPickup += OnPickupCollectible; 
            spawner.Active = true;
        }

        StartCoroutine(_boost);
    }

    public override void StopMiniGame()
    {
        base.StopMiniGame();

        StopCoroutine(_boost);
        
        foreach (Spawner spawner in _spawners)
        {
            spawner.Active = false;
            spawner.Empty();
        }
    }
    
    public void OnPickupCollectible(Transform instigator, int numberCollected)
    {
        Player player = instigator.GetComponent<Player>();

        foreach (Player p in _players)
        {
            if (player != p) continue;
            
            player._audioSource.PlayOneShot(player.pointsSound);
            _scores[_players.IndexOf(player)] += numberCollected;
            player.YouScoredMiniGamePoint(ratio, numberCollected);
            return;
        }
    }

    public IEnumerator Boost()
    {
        float delay = 8f;
        float delayOffset = 2f;

        while (true)
        {
            yield return new WaitForSeconds(delay + Random.Range(-delayOffset, delayOffset));
            StartCoroutine(_spawners[Random.Range(0, _spawners.Length - 1)].Boost());
        }
    }
}
