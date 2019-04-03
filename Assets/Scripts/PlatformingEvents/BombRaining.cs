using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRaining : MonoBehaviour
{
    [Range(0.01f, 2f)]
    public float intensityBombing = 1f;
    public GameObject bomb;
    public bool isActive;
    private GameObject _ceiling;
    private float _ceilingMinX;
    private float _ceilingMaxX;
    private WaitForSeconds _spawnBombTime;
    private bool _coroutineRunning;
    private readonly WaitForSeconds _searchNewCeilingTime;
    private bool _searchCeilingCoroutine;

    private BombRaining()
    {
        _searchNewCeilingTime = new WaitForSeconds(4f);
    }

    private void Start()
    {
        isActive = false;
        _ceiling = null;
        _spawnBombTime = new WaitForSeconds(intensityBombing);
        _coroutineRunning = false;
        _searchCeilingCoroutine = false;
    }

    private void Update()
    {
        if (!isActive) return;
        
        if (_ceiling == null)
            GetNearestCeiling();
        if (_coroutineRunning == false)
            StartCoroutine(LaunchBomb());
        if (_searchCeilingCoroutine == false)
            StartCoroutine(SearchCeiling());
    }

    private IEnumerator LaunchBomb()
    {
        _coroutineRunning = true;
        yield return _spawnBombTime;
        _coroutineRunning = false;
        SpawnBomb();
    }

    private IEnumerator SearchCeiling()
    {
        GetNearestCeiling();
        DefineCeilingLimits();
        _searchCeilingCoroutine = true;
        yield return _searchNewCeilingTime;
        _searchCeilingCoroutine = false;
    }

    private void GetNearestCeiling()
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
        DefineCeilingLimits();
    }

    private void DefineCeilingLimits()
    {
        _ceilingMaxX = _ceiling.transform.position.x + (_ceiling.transform.localScale.x / 2);
        _ceilingMinX = _ceiling.transform.position.x - (_ceiling.transform.localScale.x / 2);
    }

    private void SpawnBomb()
    {
        float xPos = Random.Range(_ceilingMinX, _ceilingMaxX);
        Vector3 position = new Vector3(xPos, _ceiling.transform.position.y - 3, _ceiling.transform.position.z);
        GameObject bomb = Instantiate(this.bomb, position, _ceiling.transform.rotation);
        bomb.GetComponent<TeddyBearBomb>().Init(false, 100, 5, 2000, null);
    }
}
