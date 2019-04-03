using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagSpawner : MonoBehaviour
{
    public float TimeSinceLastSpawn { get; set; }

    private void Awake()
    {
        TimeSinceLastSpawn = -99f;
    }
}
