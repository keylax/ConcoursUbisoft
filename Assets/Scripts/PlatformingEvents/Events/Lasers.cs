using System.Collections.Generic;
using UnityEngine;

public class Lasers : PlatformingEvent
{
    [SerializeField] private GameObject laserPrefab;

    private readonly List<Laser> _lasers;

    private Lasers()
    {
        _name = "Lasers";

        _lasers = new List<Laser>();
    }

    public override void SetPlatformingEvent(MapManager mapManager)
    {
        base.SetPlatformingEvent(mapManager);

        foreach (Transform spawnPoint in mapManager.GetPlatformingEventSpawnPoints())
        {
            _lasers.Add(Instantiate(laserPrefab, spawnPoint.position, Quaternion.identity).GetComponent<Laser>());
            if (Physics.Raycast(spawnPoint.position, -spawnPoint.up, out RaycastHit hit, 2f))
            {
                _lasers[_lasers.Count - 1].transform.rotation = hit.transform.rotation;
            }
        }
    }

    public override void StartPlatformingEvent()
    {
        base.StartPlatformingEvent();

        foreach (Laser laser in _lasers)
        {
            laser.ActivateLaser();
        }
    }

    public override void StopPlatformingEvent()
    {
        base.StopPlatformingEvent();

        foreach (Laser laser in _lasers)
        {
            laser.StopLaser();
        }
    }
}
