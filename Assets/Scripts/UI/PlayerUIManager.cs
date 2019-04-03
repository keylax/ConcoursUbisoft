using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Sprite ZombieImage;
    [SerializeField] private Sprite FirstPlaceImage;
    
    [Header("Item Images")] 
    [SerializeField] private Sprite TeddyImage;
    [SerializeField] private Sprite RemoteImage;
    [SerializeField] private Sprite HammerImage;
    [SerializeField] private Sprite DashImage;
    [SerializeField] private Sprite JetpackImage;
    
    private Player _player;
    private List<Player> _players;
    private Image _itemImage;
    private Image _onCooldownImage;
    private float _baseScaleY;
    private RectTransform _cooldownMaskTransform;
    private Text _cooldownText;

    public void SetPlayer(Player player)
    {
        _player = player;
    }
    
    private void Start()
    {
        _onCooldownImage = transform.GetChild(1).GetChild(2).gameObject.GetComponent<Image>();
        
        _onCooldownImage.gameObject.SetActive(true);
        _cooldownMaskTransform = _onCooldownImage.GetComponent<RectTransform>();
        _baseScaleY = _cooldownMaskTransform.localScale.y;
        _cooldownText = transform.GetChild(1).GetChild(3).gameObject.GetComponent<Text>();
        Vector3 scale = _cooldownText.transform.localScale;
        if (transform.localScale.x < 0)
            scale.x *= -1;
        _cooldownText.transform.localScale = scale;
        _onCooldownImage.gameObject.SetActive(false);
        _players = StaticObjects.GameController.GetPlayers();
        _itemImage = transform.GetChild(1).GetChild(1).gameObject.GetComponent<Image>();
        transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = ZombieImage;
        transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = FirstPlaceImage;
    }

    private void Update()
    {
        if (!_player)
            return;
        Player bestPlayer = null;
        bool even = false;

        foreach (Player player in _players)
        {
            if (bestPlayer == null || player.score > bestPlayer.score)
                bestPlayer = player;
        }
        if (bestPlayer != null)
        {
            foreach (Player player in _players)
            {
                if (!bestPlayer.Equals(player) && player.score == bestPlayer.score)
                    even = true;
            }
        }
        if (even)
            bestPlayer = null;
        if (!_player.hasItem) // if no item, no item box
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(1).gameObject.SetActive(true);
            if (_player.hammerActivation)
                _itemImage.sprite = HammerImage;
            else if (_player.dashActivation)
                _itemImage.sprite = DashImage;
            else if (_player.remoteControlActivation)
                _itemImage.sprite = RemoteImage;
            else if (_player.teddyBearActivation)
                _itemImage.sprite = TeddyImage;
            else if (_player.jetpackActivation)
                _itemImage.sprite = JetpackImage;

            // cooldown
            if (_player.UnitMovement.ItemCanBeUsed())
            {
                _onCooldownImage.gameObject.SetActive(false);
                _cooldownText.gameObject.SetActive(false);
            }
            else
            {
                float cooldown = _player.GetCooldown();
                _onCooldownImage.gameObject.SetActive(true);
                _cooldownText.gameObject.SetActive(true);
                _cooldownText.text = Math.Ceiling(cooldown <= 0 ? 1 : cooldown) + "";
            }
            if (_player.jetpackActivation && _player.UnitMovement.ItemCanBeUsed())
            {
                float percentageCooldown = (_player.Jetpack.GetUsageTimeMax() - _player.Jetpack.GetUsageTime()) / _player.Jetpack.GetUsageTimeMax();
                _onCooldownImage.gameObject.SetActive(true);
                _onCooldownImage.fillAmount = 1 - percentageCooldown;
            }
            else
            {
                _onCooldownImage.fillAmount = 1;
            }
        }
        
        // give medal
        if (bestPlayer && _player.Equals(bestPlayer))
            transform.GetChild(2).gameObject.SetActive(true);
        else
            transform.GetChild(2).gameObject.SetActive(false);
        
        // change playerUI position depending if in minigame or platforming
        Vector3 playerUiPosition;
        if (!StaticObjects.MapTransitionController.platforming)
        {
            playerUiPosition = transform.localPosition;
            if (playerUiPosition.y < 175)
            {
                playerUiPosition.y += 0.1f;
                transform.localPosition = playerUiPosition;
            }
        }
        else
        {
            playerUiPosition = transform.localPosition;
            if (playerUiPosition.y > 165)
            {
                playerUiPosition.y -= 0.1f;
                transform.localPosition = playerUiPosition;
            }
        }
    }
}
