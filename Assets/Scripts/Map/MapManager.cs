using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform nextMapSpawnPoint;
    [SerializeField] private GameObject nextMapTriggerGroup;
    [SerializeField] private Transform ceilingOfPlatformingSection;
    [SerializeField] private GameObject platformingEventSpawnPointsParent;

    private readonly List<Transform> _respawnPlatforms;
    private readonly List<Transform> _platformingEventSpawnPoints;
    private MapTransitionTriggerGroupManager _mapTransitionTriggerGroupManager;

    private MapManager()
    {
        _respawnPlatforms = new List<Transform>();
        _platformingEventSpawnPoints = new List<Transform>();
    }

    private void Awake()
    {
        _mapTransitionTriggerGroupManager = nextMapTriggerGroup.GetComponent<MapTransitionTriggerGroupManager>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("PlatformRespawn"))
            {
                _respawnPlatforms.Add(child);
            }
        }

        if (!platformingEventSpawnPointsParent) return;
        
        foreach (Transform child in platformingEventSpawnPointsParent.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("PlatformingEventSpawnPoint"))
            {
                _platformingEventSpawnPoints.Add(child);
            }
        }
    }

    public Vector3 GetNextMapSpawnPoint()
    {
        return nextMapSpawnPoint.position;
    }

    public void SetCameraAutoScrollLimit(MapManager platformingMapManager)
    {
        _mapTransitionTriggerGroupManager.SetCameraAutoScrollLimit(platformingMapManager.GetPlatformingCeilingWidth());
    }

    public void ActivateNextMapTriggerGroup()
    {
        nextMapTriggerGroup.SetActive(true);
    }

    private float GetPlatformingCeilingWidth()
    {
        return ceilingOfPlatformingSection.localScale.x;
    }

    public List<Transform> GetRespawnPlatforms()
    {
        return _respawnPlatforms;
    }

    public Transform GetCeiling()
    {
        return ceilingOfPlatformingSection;
    }

    public List<Transform> GetPlatformingEventSpawnPoints()
    {
        return _platformingEventSpawnPoints;
    }
}
