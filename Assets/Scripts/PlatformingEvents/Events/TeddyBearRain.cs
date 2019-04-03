using System.Collections;
using UnityEngine;

public class TeddyBearRain : PlatformingEvent
{
    [SerializeField] private GameObject teddyBearBomb;
    [SerializeField] private float timeBeforeBeginningToSpawnBombs = 2;
    [SerializeField] private float timeBetweenBombSpawns = 1.75f;
    [SerializeField] private float cameraDistanceFromCenterForSpawn = 16;
    [SerializeField] private float spawnDistanceFromCeiling = 3;
    [SerializeField] private float radiusExplosion = 4;
    [SerializeField] private float forceKnockBack = 1500;

    private Transform _ceiling;
    private float _ceilingMinX;
    private float _ceilingMaxX;
    private Transform _cameraTransform;

    private WaitForSeconds _delayStartSpawn;
    private WaitForSeconds _delaySpawnBomb;

    private TeddyBearRain()
    {
        _name = "Teddy Bear Rain";
    }

    protected override void Awake()
    {
        base.Awake();

        _delayStartSpawn = new WaitForSeconds(timeBeforeBeginningToSpawnBombs);
        _delaySpawnBomb = new WaitForSeconds(timeBetweenBombSpawns);
    }

    protected override void Start()
    {
        base.Start();

        _cameraTransform = StaticObjects.MainCameraTransform;
    }

    public override void SetPlatformingEvent(MapManager mapManager)
    {
        base.SetPlatformingEvent(mapManager);

        _ceiling = mapManager.GetCeiling();
        float half = _ceiling.localScale.x * 0.5f;
        _ceilingMaxX = _ceiling.position.x + half - 22;
        _ceilingMinX = _ceiling.position.x - half + 7;
    }

    public override void StartPlatformingEvent()
    {
        base.StartPlatformingEvent();

        StartCoroutine(SpawnBombs());
    }

    private IEnumerator SpawnBombs()
    {
        yield return _delayStartSpawn;

        while (_isActive)
        {
            SpawnBomb();

            yield return _delaySpawnBomb;
        }
    }

    private void SpawnBomb()
    {
        Vector3 position = new Vector2(GetBombXPosition(), _ceiling.position.y - spawnDistanceFromCeiling);
        TeddyBearBomb teddy = Instantiate(teddyBearBomb, position, Quaternion.identity).GetComponent<TeddyBearBomb>();
        teddy.Init(false, 0, radiusExplosion, forceKnockBack, null);
        teddy.AddTorque();
    }

    private float GetBombXPosition()
    {
        float min = _cameraTransform.position.x - cameraDistanceFromCenterForSpawn;
        float max = _cameraTransform.position.x + cameraDistanceFromCenterForSpawn;

        if (max > _ceilingMaxX)
            max = _ceilingMaxX;
        if (min < _ceilingMinX)
            min = _ceilingMinX;

        return Random.Range(min, max);
    }
}
