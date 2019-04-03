using System.Collections.Generic;
using UnityEngine;

public class Robots : PlatformingEvent
{
    [SerializeField] private GameObject robotPrefab;

    private readonly List<Enemy> _robots;
    private readonly Vector3 yOffset;

    private Robots()
    {
        _name = "Robots";

        _robots = new List<Enemy>();
        yOffset = Vector3.up * 1.25f;
    }

    public override void SetPlatformingEvent(MapManager mapManager)
    {
        base.SetPlatformingEvent(mapManager);

        foreach (Transform spawnPoint in mapManager.GetPlatformingEventSpawnPoints())
        {
            Enemy robot = Instantiate(robotPrefab, spawnPoint.position + yOffset, Quaternion.identity).GetComponent<Enemy>();
            robot.AIInput.SetPlatforms(mapManager.GetRespawnPlatforms());// ?
            robot.GetComponent<AIInput>().stayOnPatrol = true;
            _robots.Add(robot);
        }
    }
    
    private void RemoveKilledEnemies()
    {
        foreach (Enemy enemy in _robots)
        {
            if (enemy.isDead)
            {
                enemy._audioSource.PlayOneShot(enemy.deathSound);
                Destroy(enemy);
                enemy.gameObject.SetActive(false);
            }
        }
        _robots.RemoveAll(item => item.isDead);
    }

    private void Update()
    {
        RemoveKilledEnemies();
    }

    public override void StartPlatformingEvent()
    {
        base.StartPlatformingEvent();

        foreach (Enemy robot in _robots)
        {
            // ?
        }
    }

    public override void StopPlatformingEvent()
    {
        base.StopPlatformingEvent();

        foreach (Enemy robot in _robots)
        {
            robot.gameObject.SetActive(false);
        }
    }
}
