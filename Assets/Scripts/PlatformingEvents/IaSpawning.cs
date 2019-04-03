using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IaSpawning : MonoBehaviour
{
    [Range(0.01f, 5f)]
    public float intensitySpawning = 1f;
    public GameObject ia;
    public bool isActive;
    private GameObject _ceiling;
    private float _ceilingMinX;
    private float _ceilingMaxX;
    private WaitForSeconds _spawnIaTime;
    private bool _coroutineRunning;
    private int _enemiesSpawnedNb;
    private WaitForSeconds _searchNewCeilingTime = new WaitForSeconds(4f);
    private bool _searchCeilingCoroutine;


    void Start()
    {
        isActive = false;
        _ceiling = null;
        _spawnIaTime = new WaitForSeconds(intensitySpawning);
        _coroutineRunning = false;
        _enemiesSpawnedNb = 0;
        _searchCeilingCoroutine = false;
    }

    void Update()
    {
        if (isActive)
        {
            if (_ceiling == null)
            {
                // Pour le moment, ne prend que UNE SEULE FOIS le plafond le plus près.
                getNearestCeiling();
                defineCeilingLimits();
            }
            if (_coroutineRunning == false)
                StartCoroutine(LaunchSpawning());
            if (_searchCeilingCoroutine == false)
                StartCoroutine(SearchCeiling());
            KillIa();
        }
    }

    private void KillIa()
    {
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyList)
        {
            if (enemy.GetComponent<Enemy>().isDead)
                Destroy(enemy);
        }
    }

    private IEnumerator LaunchSpawning()
    {
        _coroutineRunning = true;
        yield return _spawnIaTime;
        _coroutineRunning = false;
        SpawnIa();
    }

    private IEnumerator SearchCeiling()
    {
        getNearestCeiling();
        defineCeilingLimits();
        _searchCeilingCoroutine = true;
        yield return _searchNewCeilingTime;
        _searchCeilingCoroutine = false;
    }

    private void getNearestCeiling()
    {
        GameObject[] ceilings = GameObject.FindGameObjectsWithTag("PlatformCeiling");
        _ceiling = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = Camera.main.transform.position;
        foreach (GameObject t in ceilings)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                _ceiling = t;
                minDist = dist;
            }
        }
    }

    private void defineCeilingLimits()
    {
        _ceilingMaxX = _ceiling.transform.position.x + (_ceiling.transform.localScale.x / 2);
        _ceilingMinX = _ceiling.transform.position.x - (_ceiling.transform.localScale.x / 2);
    }

    private void SpawnIa()
    {
        _enemiesSpawnedNb++;
        float xPos = Random.Range(_ceilingMinX, _ceilingMaxX);
        Vector3 position = new Vector3(xPos, _ceiling.transform.position.y - 3, _ceiling.transform.position.z);
        Enemy enemy = Instantiate(ia, position, Quaternion.identity).GetComponent<Enemy>();
        enemy.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        enemy.GetComponent<Enemy>().ID = _enemiesSpawnedNb - 1;
    }
}
