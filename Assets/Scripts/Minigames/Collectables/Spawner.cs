using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool IsReady { get; private set; }
    public bool Active { get; set; }

    [Header("Spawn Parameters")]
    public GameObject collectableGameObject;
    [Range(.3f, 3f)] public float pickupRadius = 2f;
    [Range(1, 15)] public int maxObjectNumber = 6;
    [Range(.5f, 10f)] public float spawnTimer = 4f;
    [Range(0f, 5f)] public float maxTimerOffset = 4f;
    private float _spawnDelayAfterPickup = 1.5f;
    private float _timeBeforeNextSpawn;
    private float _timerOffset;
    private float _spawnDelay;
    
    private float _boostDuration = 4f;
    
    private SphereCollider _collider;
    private MeshRenderer[] _poolMeshRenderer;
    private Light[] _poolLights;
    private int _numCollectables;

    public delegate void PickupAction(Transform instigator, int number);
    public event PickupAction OnPickup;

    private void Start()
    {
        IsReady = false;
        _timerOffset = Random.Range(-maxTimerOffset, maxTimerOffset);
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    { 
        // Initialise the collider for pickup
        _collider = gameObject.AddComponent<SphereCollider>();
        _collider.radius = pickupRadius;
        _collider.isTrigger = true;

        yield return null;

        _poolMeshRenderer = new MeshRenderer[maxObjectNumber];
        _poolLights = new Light[maxObjectNumber];
        _timeBeforeNextSpawn = 0;
        _numCollectables = 0;
        _spawnDelay = 0;

        if (StaticObjects.GameController.GetPlayers().Count > 2)
        {
            spawnTimer /= 2f;
            maxTimerOffset /= 2f;
        }
        
        // Instanciate the collectables (pooling)
        for (int i = 0; i < maxObjectNumber; i++)
        {
            GameObject obj = Instantiate(collectableGameObject, transform.position + Vector3.up * i / 3f, Quaternion.identity, transform);
            _poolMeshRenderer[i] = obj.GetComponent<MeshRenderer>();
            _poolLights[i] = obj.GetComponentInChildren<Light>();
            _poolMeshRenderer[i].enabled = false;
            _poolLights[i].enabled = false;
            yield return null;
        }

        IsReady = true;
    }

    private void Update()
    {
        if (!Active || !IsReady || _numCollectables >= maxObjectNumber)
            return;

        if (_spawnDelay > 0f)
        {
            _spawnDelay -= Time.deltaTime;
        }
        else
        {
            _timeBeforeNextSpawn += Time.deltaTime;
            if (_timeBeforeNextSpawn >= spawnTimer + _timerOffset)
            {
                SpawnCollectable();
                _timeBeforeNextSpawn -= spawnTimer;
            }
        }
    }

    private void SpawnCollectable()
    {
        _timerOffset = Random.Range(-maxTimerOffset, maxTimerOffset);

        _poolMeshRenderer[_numCollectables].enabled = true;
        _poolLights[_numCollectables].enabled = true;
        _numCollectables++;
    }

    public void Empty()
    {
        while (_numCollectables > 0)
        {
            _poolMeshRenderer[_numCollectables - 1].enabled = false;
            _poolLights[_numCollectables - 1].enabled = false;
            _numCollectables--;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Active && IsReady && other.CompareTag(Utility.PLAYER_TAG))
        {
            OnPickup(other.transform, _numCollectables);
            _spawnDelay += _spawnDelayAfterPickup;
            Empty();
        }
    }

    public IEnumerator Boost()
    {
        // Save context
        float tmpTimer = spawnTimer;
        float tmpOffset = maxTimerOffset;

        // Apply Boost
        spawnTimer = .75f;
        maxTimerOffset = 0f;
        
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2f, Color.red, _boostDuration, false);

        yield return new WaitForSeconds(_boostDuration);
        
        // Restore context
        spawnTimer = tmpTimer;
        maxTimerOffset = tmpOffset;
    }
}
