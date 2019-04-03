using System.Collections.Generic;
using UnityEngine;

public class Saws : PlatformingEvent
{
    [SerializeField] private GameObject sawPrefab;

    private readonly List<Saw> _saws;

    private Saws()
    {
        _name = "Saws";

        _saws = new List<Saw>();
    }

    public override void SetPlatformingEvent(MapManager mapManager)
    {
        base.SetPlatformingEvent(mapManager);

        foreach (Transform spawnPoint in mapManager.GetPlatformingEventSpawnPoints())
        {
            _saws.Add(Instantiate(sawPrefab, spawnPoint.position, Quaternion.identity).GetComponent<Saw>());
            if (Physics.Raycast(spawnPoint.position, -spawnPoint.up, out RaycastHit hit, 2f))
                _saws[_saws.Count - 1].transform.rotation = hit.transform.rotation;
        }
    }

    public override void StartPlatformingEvent()
    {
        base.StartPlatformingEvent();

        foreach (Saw saw in _saws)
        {
            saw.ActivateSaw();
        }
    }

    public override void StopPlatformingEvent()
    {
        base.StopPlatformingEvent();

        foreach (Saw saw in _saws)
        {
            saw.DeactivateSaw();
        }
    }
}
