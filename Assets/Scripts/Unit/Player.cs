using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Image = UnityEngine.UI.Image;

public class Player : Unit
{
    public PlayerInput PlayerInput { get; private set; }
    public bool hasItem;
    public PickObject.ObjectName item;
    public float score;
    public Color color;

    public Jetpack Jetpack { get; private set; }
    public Dash Dash { get; private set; }
    public RemoteControl RemoteControl { get; private set; }
    public TeddyBear TeddyBear { get; private set; }
    public Hammer Hammer { get; private set; }

    [Header("Other")]
    [SerializeField] public DisplayButton displayControllerButton;
    [SerializeField] public float movementAdvantageIfBehind = 0.075f; // [0, 1] percentage if 0.1, the players not ahead will go 10% faster
    public bool firstInPlatforming;
    public Image arrow;
    public Material materialExample;

    [Header("Jetpack")]
    [SerializeField] public bool jetpackActivation;
    [SerializeField] private float cooldownJetpack = 3f;
    [SerializeField] private float forceJetPack = 400f;
    [SerializeField] private float usageTimeMaxJetPack = 1.5f;
    [SerializeField] public GameObject specialEffectJetPack;
    [SerializeField] private bool putJetpackOnCooldownIfHitWhileUsing = true;
    public VoiceLine jetpackLine;
    public GameObject jetPackRig;
    
    [Header("Dash")]
    [SerializeField] public bool dashActivation;
    [SerializeField] private float cooldownDash = 4f;
    [SerializeField] private float forceDash = 400f;
    [SerializeField] private float stunDurationDash = 0.5f;
    [SerializeField] public GameObject specialEffectDash;
    public VoiceLine dashLine;
    public GameObject dashRig;

    [Header("RemoteControl")]
    [SerializeField] public bool remoteControlActivation;
    [SerializeField] private float cooldownRemoteControl = 10f;
    [SerializeField] private float durationRemoteControl = 400f;
    [SerializeField] public GameObject specialEffectCracklingPictureRemoteControl;
    [SerializeField] public float remoteMovementHandicap = 0.2f; // [0, 1] percentage if 0.1, the players affected by remote will go 10% slower
    public VoiceLine remoteLine;
    public GameObject remoteRig;

    [Header("TeddyBear")]
    [SerializeField] public bool teddyBearActivation;
    [SerializeField] private float cooldownTeddyBear = 4f;
    [SerializeField] private float durationBeforeExplosionTeddyBear = 1.25f;
    [SerializeField] private float throwingForceTeddyBear = 10f;
    [SerializeField] private float radiusExplosionTeddyBear = 5;
    [SerializeField] private float forceKnockBackTeddyBear = 2500;
    [SerializeField] public GameObject teddyBearBomb;
    public VoiceLine teddyLine;
    public GameObject teddyRig;

    [Header("Hammer")]
    [SerializeField] public bool hammerActivation;
    [SerializeField] private float cooldownHammer = 3f;
    [SerializeField] private float forceKnockBackHammer = 40;
    [SerializeField] private float hitBoxPersistenceHammer = 0.2f;
    [SerializeField] public GameObject specialEffectHammer;
    public VoiceLine hammerLine;
    public GameObject hammerRig;

    [Header("Points coefficients")]
    [SerializeField] public float deathMalus = 0.25f; // [0, 1] : percentage of points player loses when he dies
    [SerializeField] public float weaponMultiplicator = 5;
    [SerializeField] public float takeHitFromEnemy = 0.5f;
    [SerializeField] public float giveHitToEnemy = 0;
    [SerializeField] public float takeHitFromPlayer = 1;
    [SerializeField] public float giveHitToPlayer = 1;
    [SerializeField] private float takeHitFromBoss = 10;
    [SerializeField] private float giveHitToBoss = 0.5f;
    [SerializeField] public float firstInPlatformingPerFrame = 0.2f;
    [SerializeField] public float wonMinigame = 1000; // not to many points because he alreagy get an item + he get more jauge points during the minigame than others, it is enough
    [SerializeField] public float wonPlatforming = 1000;
    [SerializeField] public float scoreMinigamePoint = 100;
    [SerializeField] public float handicapFirstPlayer = 0.1f; // percentage of points the first player will get everytime he gets points
    [SerializeField] public float advantageLastPlayer = 0.1f; // percentage of points the last player will get everytime he gets points

    [Header("Points UI")]
    [SerializeField] public GameObject pointsUiEffectObject;

    [Header("Sound Player")]
    [SerializeField] protected AudioClip _congratulationSound;
    [SerializeField] public AudioClip _changeObjectSound;
    [SerializeField] public AudioClip _dashSound;
    [SerializeField] public AudioClip baseballBatSound;
    [SerializeField] public AudioClip pointsSound;
    [SerializeField] public AudioSource jetpackSound;
    [SerializeField] public AudioSource walkSound;
    [SerializeField] public AudioSource remoteControlSound;

    [Header("Rig elements")] 
    [SerializeField] public GameObject leftHand;
    [SerializeField] public GameObject rightHand;
    [SerializeField] public GameObject rightLeg;
    [SerializeField] public GameObject leftLeg;
    [SerializeField] public GameObject shirt;
    [SerializeField] public GameObject shortCloth;
    [SerializeField] public GameObject shoes;
    [SerializeField] public GameObject bandeau;
    
    private ParticleSystem _pointsUiEffect;

    protected override void Awake()
    {
        PlayerInput = gameObject.AddComponent<PlayerInput>();

        Jetpack = gameObject.AddComponent<Jetpack>();
        Dash = gameObject.AddComponent<Dash>();
        RemoteControl = gameObject.AddComponent<RemoteControl>();
        TeddyBear = gameObject.AddComponent<TeddyBear>();
        Hammer = gameObject.AddComponent<Hammer>();

        Jetpack.Init(cooldownJetpack, usageTimeMaxJetPack);
        Dash.Init(cooldownDash, stunDurationDash, forceDash);
        RemoteControl.Init(cooldownRemoteControl, durationRemoteControl);
        TeddyBear.Init(cooldownTeddyBear, durationBeforeExplosionTeddyBear, throwingForceTeddyBear, radiusExplosionTeddyBear, forceKnockBackTeddyBear);
        Hammer.Init(cooldownHammer, forceKnockBackHammer, hitBoxPersistenceHammer);

        if (hammerActivation || dashActivation || remoteControlActivation || teddyBearActivation || jetpackActivation)
            hasItem = true;

        base.Awake();

        UnitMovement.Init(jumpForce, horizontalMovementSmoothing, groundLayers, groundCheck, customGravityForce, stunEffectObject, forceJetPack);
        _pointsUiEffect = pointsUiEffectObject.GetComponent<ParticleSystem>();
    }

    protected override void Start()
    {
        base.Start();
        displayControllerButton.Init(gameObject, DisplayButton.ButtonEnum.B);
        displayControllerButton.height = 55;
    }

    public void SetColor(Color givenColor)
    {
        color = givenColor;
        // c'est moche désolé
        if (ID == 1)
            arrow.color = Color.red;
        else if (ID == 2)
            arrow.color = new Color(Color.cyan.r * 0.85f, Color.cyan.g * 0.85f, Color.cyan.b * 0.85f);
        else if (ID == 3)
            arrow.color = Color.yellow;
        else if (ID == 4)
            arrow.color = Color.green;
        /*
        SerializedObject halo = new SerializedObject(arrow.GetComponent("Halo"));
        Color haloColor = new Color(color.r + 0.1f, color.g + 0.1f, color.b + 0.1f);
        halo.FindProperty("m_Color").colorValue = haloColor;
        halo.ApplyModifiedProperties();
        */
        
        // set rig texture
        Material new_material = Instantiate(materialExample);
        new_material.color = color;
        shoes.GetComponent<SkinnedMeshRenderer>().material = new_material;
        shortCloth.GetComponent<SkinnedMeshRenderer>().material = new_material;
        shirt.GetComponent<SkinnedMeshRenderer>().material = new_material;
        bandeau.GetComponent<SkinnedMeshRenderer>().material = new_material;
    }

    private void Update()
    {
        if (hammerActivation || dashActivation || remoteControlActivation || teddyBearActivation || jetpackActivation)
            hasItem = true;
        hammerRig.SetActive(hammerActivation);
        dashRig.SetActive(dashActivation);
        remoteRig.SetActive(remoteControlActivation);
        teddyRig.SetActive(teddyBearActivation && UnitMovement.ItemCanBeUsed());
        jetPackRig.SetActive(jetpackActivation);
        
        Vector3 position = transform.position;
        position.y += 2;
        arrow.transform.position = position;
    }

    protected override UnitInput GetUnitInput()
    {
        return PlayerInput;
    }

    protected override void KillUnit()
    {
        Debug.Log("Death player " + ID);
        StartCoroutine(StopDeathEffect());
        isDead = true;
        Respawn();
    }

    public IEnumerator StopDeathEffect()
    {
        Vector3 pos = new Vector3();
        pos.x = this.transform.position.x + 8;
        pos.y = this.transform.position.y;
        pos.z = this.transform.position.z;
        GameObject deathEffect = Instantiate(specialEffectDeath, pos, specialEffectDeath.transform.rotation);
        deathEffect.GetComponent<Renderer>().material.color = color;
        deathEffect.SetActive(true);
        deathEffect.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(3f);

        deathEffect.SetActive(false);
        Destroy(deathEffect);
    }
    /*
    private bool GetRespawnPositionRecursively(GameObject platform, out Vector3 respawnPosition)
    {
        respawnPosition = Vector3.negativeInfinity;
        if (platform.transform.childCount == 0)
        {
            if (platform.CompareTag("PlatformRespawn") && platform.GetComponent<Renderer>() && platform.GetComponent<Renderer>().isVisible)
            {
                Vector3 position = platform.transform.position;
                position.y += (platform.transform.localScale.y / 2) + 1.4f;
                for (float i = platform.transform.position.x - platform.transform.localScale.x / 2 + 0.5f;
                    i <= (platform.transform.localScale.x - 1) / 2 + platform.transform.position.x;
                    i += 0.5f)
                {
                    Vector3 tempPosition = new Vector3(i, position.y, 0);
                    Vector3 screenPos = StaticObjects.CameraController.Camera.WorldToViewportPoint(tempPosition);
                    if (screenPos.x >= 0.5 && screenPos.x <= 0.66 && screenPos.y >= 0 && screenPos.y <= 1 && screenPos.z > 0 && !Physics.CheckSphere(tempPosition, 0.9f))
                    {
                        respawnPosition = tempPosition;
                        return true;
                    }
                }
            }

            return false;
        }

        for (int childIndex = 0; childIndex != platform.transform.childCount; ++childIndex)
        {
            if (GetRespawnPositionRecursively(platform.transform.GetChild(childIndex).gameObject, out respawnPosition))
                return true;
        }

        return false;
    }

    private bool GetRespawnPosition(out Vector3 respawnPlatform)
    {
        return GetRespawnPositionRecursively(StaticObjects.MapCreationController.GetCurrentMap().gameObject, out respawnPlatform);
    }
    */

    private Vector2 GetRespawnPosition()
    {
        float tropDursEnGeneral = 0.1f;
        Vector2 position = Vector2.negativeInfinity;
        for (int j = 1; j <= 5; ++j)
        {
            foreach (Transform bestPlatform in StaticObjects.MapCreationController.GetCurrentMap().GetRespawnPlatforms())
            {
                position = bestPlatform.position;
                position.y += (bestPlatform.localScale.y * 0.5f) + 1.4f;
                for (float i = bestPlatform.position.x - bestPlatform.localScale.x / 2 + 0.5f;
                    i <= (bestPlatform.localScale.x - 1) / 2 + bestPlatform.position.x;
                    i += 0.5f)
                {
                    Vector2 tempPosition = new Vector3(i, position.y);
                    Vector2 screenPos = StaticObjects.CameraController.Camera.WorldToViewportPoint(tempPosition);
                    if (screenPos.x >= 0.5 - tropDursEnGeneral  * j && screenPos.x <= 0.5 + tropDursEnGeneral * j &&
                        screenPos.y >= 0 && screenPos.y <= 1 &&
                        !Physics.CheckSphere(tempPosition, 0.9f))
                    {
                        position = tempPosition;
                        return position;
                    }
                }
            }
        }

        return position;
    }

    private void Respawn()
    {
        Vector2 position = GetRespawnPosition();
        if (position.Equals(Vector2.negativeInfinity))
            return;
        isDead = false;
        _audioSource.PlayOneShot(deathSound);
        _audioSource.PlayOneShot(_apparitionSound);
        StartCoroutine(StopRespawnEffect());
        transform.SetPositionAndRotation(position, Quaternion.identity);
        RemovePoints(score * deathMalus);
    }

    private IEnumerator StopRespawnEffect()
    {
        specialEffectRespawn.SetActive(true);
        _particleSystemForRespawn.Play();

        yield return _delayRespawnEffect;

        _particleSystemForRespawn.Stop();
        specialEffectRespawn.SetActive(false);
    }

    public override void TakeAHit(Unit hitter, float damage)
    {
        if (hitter.CompareTag("Player"))
        {
            RemovePoints(damage * weaponMultiplicator * 0.5f * takeHitFromPlayer);  
            if (putJetpackOnCooldownIfHitWhileUsing && jetpackActivation && Jetpack.IsBeingUsed())
                Jetpack.ForceCooldown();
        }
        else if (hitter.CompareTag("Enemy"))
            RemovePoints(damage * weaponMultiplicator * 0.5f * takeHitFromEnemy);
        
        base.TakeAHit(hitter, damage);
        // Debug.Log("Player " + ID + " score : " + score);
    }

    public void TakeAHitFromBoss(float damage)
    {
        RemovePoints((int)damage * weaponMultiplicator * takeHitFromBoss);
    }

    public void TouchAHit(Unit victim, float damage)
    {
        if (victim.CompareTag("Player"))
            AddPoints(damage * weaponMultiplicator * giveHitToPlayer);
        else if (victim.CompareTag("Enemy"))
            AddPoints(damage * weaponMultiplicator * giveHitToEnemy);
        // Debug.Log("Player " + ID + " score : " + score);
    }

    public void TouchAHitOnBoss(float damage)
    {
        AddPoints((int)damage * weaponMultiplicator * giveHitToBoss);
    }

    public void YouAreFirstInPlatforming( float distanceToSecondPlayer) // the platforming section waits for 0.5 second before starting increment first player point. Go to MapTransitionController to change this value
    {
        // Debug.Log(firstInPlatformingPerFrame * (distanceToSecondPlayer + 1));
        AddPoints(firstInPlatformingPerFrame * (distanceToSecondPlayer + 1));
    }

    public void YouWonMiniGame()
    {
        _audioSource.PlayOneShot(_congratulationSound);
        AddPoints(wonMinigame);
    }

    public void YouWonPlatforming()
    {
        _audioSource.PlayOneShot(_congratulationSound);
        AddPoints(wonPlatforming);
    }

    // every minigame objective shouldn't weight the same in scores (for exemple, killing enemy should be more rettributed than pick a collectable). ehence ratio float
    public void YouScoredMiniGamePoint(float ratio, int numberOfPoints = 1)
    {
        AddPoints(scoreMinigamePoint * ratio * numberOfPoints);
    }

    public void YouUnScoredMiniGamePoint(float ratio, int numberOfPoints = 1)
    {
        RemovePoints(scoreMinigamePoint * ratio * numberOfPoints);
    }

    private void AddPoints(float points)
    {
        if (points > 1)
            StartCoroutine(DisplayChangedScore(points));
        if (AreYouTheHighestScore())
            score += points * (1 - handicapFirstPlayer);
        else if (AreYouTheLowestScore())
            score += points * (1 + advantageLastPlayer);
        else
            score += points;
    }

    private void RemovePoints(float points)
    {
        if (points > 1)
            StartCoroutine(DisplayChangedScore(-points));
        if (score - points > 0)
        {
            if (AreYouTheHighestScore())
                score -= points * (1 + handicapFirstPlayer);
            else if (AreYouTheLowestScore())
                score -= points * (1 - advantageLastPlayer);
            else
                score -= points;
        }
        else
        {
            score = 0;
        }
    }

    public IEnumerator DisplayChangedScore(float points)
    {
        Vector3 pos = new Vector3();
        pos.x = this.transform.position.x;
        pos.y = this.transform.position.y + 2;
        pos.z = this.transform.position.z;

        GameObject displayScore = Instantiate(pointsUiEffectObject, pos, pointsUiEffectObject.transform.rotation);
        displayScore.SetActive(true);
        DisplayPoints displayPointScript = displayScore.GetComponent<DisplayPoints>();
        displayPointScript.SetDisplayText(gameObject, points);

        yield return new WaitForSeconds(1f);

        Destroy(displayScore);
    }

    public float GetCooldown()
    {
        if (!hasItem)
            return 0;
        if (hammerActivation && Hammer.isOnCooldown)
            return Hammer.cooldownDuration - (Time.time - Hammer.useTime);
        if (dashActivation && Dash.isOnCooldown)
            return Dash.cooldownDuration - (Time.time - Dash.useTime);
        if (remoteControlActivation && RemoteControl.isOnCooldown)
            return RemoteControl.cooldownDuration - (Time.time - RemoteControl.useTime);
        if (teddyBearActivation && TeddyBear.isOnCooldown)
            return TeddyBear.cooldownDuration - (Time.time - TeddyBear.useTime);
        if (jetpackActivation && Jetpack.isOnCooldown)
            return Jetpack.cooldownDuration - (Time.time - Jetpack.useTime);

        return 0;
    }

    private bool AreYouTheHighestScore()
    {
        Player best = null;

        foreach (Player player in StaticObjects.GameController.GetPlayers())
        {
            if (!best || best.score < player.score)
                best = player;
        }

        return !best && Equals(best);
    }

    private bool AreYouTheLowestScore()
    {
        Player last = null;

        foreach (Player player in StaticObjects.GameController.GetPlayers())
        {
            if (!last || last.score > player.score)
                last = player;
        }

        return !last && Equals(last);
    }
    
    
    public void TakeAHitFromSawOrLaser(float damages)
    {
        RemovePoints(damages);
    }
}
