using UnityEngine;

public class MinigameTest : MiniGame
{
    [SerializeField] private float ratio = 1;

    private MinigameTest()
    {
        _name = "TEST ";
    }

    protected override void Awake()
    {
        base.Awake();
        
        _name += Random.Range(0, 1000);
        _ratio = ratio;
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        IncrementScore(_players[0]);
    }
}
