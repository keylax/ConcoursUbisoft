using System.Collections.Generic;
using UnityEngine;

public class DebugInputs : MonoBehaviour
{
    private List<Player> _players;

    private void Start()
    {
        _players = StaticObjects.GameController.GetPlayers();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            StaticObjects.ItemPool.SpawnItem(StaticObjects.GameController.GetPlayers()[0].transform.position + Vector3.up);

        
        if (Input.GetKeyDown(KeyCode.H))
            StaticObjects.CrowdClient.EndPoll();
        
        if (Input.GetKeyDown(KeyCode.G))
            StaticObjects.MapCreationController.StopActiveMiniGame();

        if (Input.GetKeyDown(KeyCode.Y) && _players.Count > 1)
            _players[1].transform.position = _players[0].transform.position;

    }
}
