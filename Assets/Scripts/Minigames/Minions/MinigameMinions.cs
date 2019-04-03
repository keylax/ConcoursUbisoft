using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameMinions : MiniGame
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfWaves = 1;
    [SerializeField] private int numberOfEnemies = 20;
    [SerializeField] private float _lapsBetweenSpawning = 3f;
    [SerializeField] private float _timeStartSpawning = 0f;
    [SerializeField] private int _maxEnemies = 10; // max number of enemies on the scene at the same time
    public GameObject enemySpawnsGameObject;
    [SerializeField] private float ratio = 1;

    private readonly List<Enemy> _enemies;
    private readonly List<Enemy> _pool;
    private readonly Color _enemyColor;
    private readonly WaitForSeconds _delayToHidePlatform;
    private int _enemiesSpawnedNb;
    private int _waveIndex;
    private readonly List<Transform> _platforms;
    private List<GameObject> _enemySpawnPlatforms;

    private float _lastSpawn;

    private MinigameMinions()
    {
        _enemies = new List<Enemy>();
        _pool = new List<Enemy>();
        _enemyColor = Color.green;
        _delayToHidePlatform = new WaitForSeconds(1);
        _platforms = new List<Transform>();

        _name = "Scrapyard";
    }
    
    protected override void Awake()
    {
        base.Awake();

        _ratio = ratio;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("PlatformRespawn") || child.CompareTag("Plank"))
            {
                _platforms.Add(child);
            }
        }
        _enemySpawnPlatforms = new List<GameObject>();
        for (int i = 0; i != enemySpawnsGameObject.transform.childCount; ++i)
            _enemySpawnPlatforms.Add(enemySpawnsGameObject.transform.GetChild(i).gameObject);
    }

    public override void StartMiniGame()
    {
        foreach (Transform child in StaticObjects.MapCreationController.GetCurrentMap().GetRespawnPlatforms())
        {
            _platforms.Add(child);
        }
        
        base.StartMiniGame();
    }

    public override void StopMiniGame()
    {
        base.StopMiniGame();

        foreach (Enemy enemy in _enemies)
        {
            _pool.Add(enemy);
            enemy.gameObject.SetActive(false);
        }
        _enemies.Clear();
    }

    public override void DestroyMiniGame()
    {
        foreach (Enemy enemy in _pool)
            Destroy(enemy.gameObject);

        base.DestroyMiniGame();
    }

    private void Update()
    {
        if (!_isActive)
            return;

        RemoveKilledEnemies();
        if (Time.time - _lastSpawn > _lapsBetweenSpawning)
        {
            SpawnEnemy();
            _lastSpawn = Time.time;
        }
        if (_enemiesSpawnedNb >= numberOfEnemies && _enemies.Count == 0)
        {
            if (!GetWinner())
                _enemiesSpawnedNb -= 1;
            else if (_waveIndex >= numberOfWaves - 1)
                StopMiniGame();
            else
            {
                _enemiesSpawnedNb = 0;
                _waveIndex += 1;
            }
        }
    }

    private void RemoveKilledEnemies()
    {
        foreach (Enemy enemy in _enemies)
        {
            if (enemy.isDead)
            {
                enemy._audioSource.PlayOneShot(enemy.deathSound);
                if (enemy.lastHit && enemy.lastHit is Player player)
                    IncrementScore(player);
                _pool.Add(enemy);
                enemy.gameObject.SetActive(false);
            }
        }
        _enemies.RemoveAll(item => item.isDead);
    }

    private void SpawnEnemy()
    {
        foreach (GameObject enemySpawn in _enemySpawnPlatforms)
        {
            if (_enemiesSpawnedNb >= numberOfEnemies || _enemies.Count >= _maxEnemies)
                return;
            if (enemySpawn.activeSelf)
                continue;
            enemySpawn.SetActive(true);
            Vector3 position = enemySpawn.transform.position;
            position.y += 1.5f;
            Enemy enemy = null;
            if (_pool.Count == 0)
            {
                enemy = Instantiate(enemyPrefab, position, Quaternion.identity).GetComponent<Enemy>();
                enemy.AIInput.SetPlatforms(_platforms);
                enemy.GetComponentInChildren<SkinnedMeshRenderer>().material.color = _enemyColor;
                enemy.GetComponent<Enemy>().ID = _enemiesSpawnedNb - 1;
                StartCoroutine(enemy.StopRespawnEffect());
                enemy.PlayApparitionSound();
            }
            else
            {
                enemy = _pool[0];
                enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
                enemy.Respawn();
                enemy.gameObject.SetActive(true);
                _pool.RemoveAt(0);
                StartCoroutine(enemy.StopRespawnEffect());
                enemy.PlayApparitionSound();
            }
            _enemies.Add(enemy);
            StartCoroutine(HidePlatform(enemySpawn));
            _enemiesSpawnedNb += 1;
        }
    }

    private IEnumerator HidePlatform(GameObject platform)
    {
        yield return _delayToHidePlatform;

        if (platform)
            platform.SetActive(false);
    }
}
