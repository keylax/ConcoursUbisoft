using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [SerializeField] Image _blackBackGround;
    [SerializeField] Image _presentateur;
    [SerializeField] Image _winnerBackground;

    [SerializeField] GameObject _winner;
    [SerializeField] private Text _bestScoreText;

    [SerializeField] GameObject subtitlesObject;
    [SerializeField] GameObject subtitlesText;

    [SerializeField] private AudioClip _player1WonSound;
    [SerializeField] private AudioClip _player2WonSound;
    [SerializeField] private AudioClip _player3WonSound;
    [SerializeField] private AudioClip _player4WonSound;

    [SerializeField] private AudioClip _icantDoThisAnymoreSound;
    [SerializeField] private AudioClip _seeYouNextWeekSound;
    [SerializeField] private AudioClip _congratulationsSound;

    [SerializeField] private AudioClip _drumRollSound;
    [SerializeField] private AudioClip _happyGuysSound;
    [SerializeField] private AudioClip _trumpetSound;


    private GameController _actualGame;
    private bool _blackbackGroundIsSet;
    private bool _presentateurIsSet;
    private bool _isActive;
    private AudioSource _audioSource;
    private Text _subTitleText;
    private bool _coroutineRunning;
    private string _bestPlayer;
    private float _bestScore;
    private Color _colorBestPlayer;
    
    //
    public bool manualLaunch = false;
    //

    void Start()
    {
        _subTitleText = subtitlesText.GetComponent<Text>();
        _audioSource = this.GetComponent<AudioSource>();
        _isActive = false;
        _blackbackGroundIsSet = false;
        _presentateurIsSet = false;
        _blackBackGround.enabled = false;
        _coroutineRunning = false;
        _bestPlayer = "";
        _actualGame = StaticObjects.GameController;
    }

    public void Launch()
    {
        _isActive = true;
        _blackBackGround.enabled = true;
    }

    private void CustomPlay(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    void Update()
    {
        if (manualLaunch && !_audioSource.isPlaying)
        {
            manualLaunch = false;
            Launch();
        }

        if (!_isActive) return;

       

        if (!_blackbackGroundIsSet)
        {
            Color col = _blackBackGround.color;
            if (col.a <= 1)
            {
                col.a += (Time.deltaTime * 0.1f);
                _blackBackGround.color = col;
            }
            else
            {
                _actualGame.Ending(true);
                StaticObjects.BackgroundMusic.PauseMusic();
                _blackbackGroundIsSet = true;
            }
        }


        if (!_blackbackGroundIsSet && !_audioSource.isPlaying && _audioSource.clip == null)
            CustomPlay(_happyGuysSound);
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _happyGuysSound)
            StartCoroutine(WaitToSpeak(2, "Alright ... I can't do this anymore ...", _icantDoThisAnymoreSound, false));
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _icantDoThisAnymoreSound)
            StartCoroutine(WaitToSpeak(1, "And also, congratulations to this week's winner ...", _congratulationsSound, false));
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _congratulationsSound)
            StartCoroutine(WaitToSpeak(0, "", _drumRollSound, false));
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _drumRollSound)
        {
            BestPlayer();
            _winnerBackground.color = _colorBestPlayer;
            _bestScoreText.text = Mathf.RoundToInt(_bestScore).ToString();
            if (_bestPlayer == "Player 1")
                StartCoroutine(WaitToSpeak(0.5f, "Player 1 !", _player1WonSound, true));
            if (_bestPlayer == "Player 2")
                StartCoroutine(WaitToSpeak(0.5f, "Player 2 !", _player2WonSound, true));
            if (_bestPlayer == "Player 3")
                StartCoroutine(WaitToSpeak(0.5f, "Player 3 !", _player3WonSound, true));
            if (_bestPlayer == "Player 4")
                StartCoroutine(WaitToSpeak(0.5f, "Player 4 !", _player4WonSound, true));
        }
        if (!_audioSource.isPlaying && !_coroutineRunning && 
            (_audioSource.clip == _player1WonSound ||
            _audioSource.clip == _player2WonSound ||
            _audioSource.clip == _player3WonSound ||
            _audioSource.clip == _player4WonSound))
        {
            if (!_presentateurIsSet)
            {
                Color col = _presentateur.color;
                if (col.a <= 1)
                {
                    col.a += (Time.deltaTime * 0.2f);
                    _presentateur.color = col;
                }
                else
                    _presentateurIsSet = true;
            }
            else
                StartCoroutine(WaitToSpeak(1, "... See you next week", _seeYouNextWeekSound, false));
        }
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _seeYouNextWeekSound)
        {
            subtitlesObject.SetActive(false);
            Color col = _presentateur.color;
            if (col.a > 0)
            {
                col.a -= (Time.deltaTime * 0.2f);
                _presentateur.color = col;
            }
            else
                _presentateurIsSet = false;
        }
        if (!_audioSource.isPlaying && !_coroutineRunning && _audioSource.clip == _seeYouNextWeekSound && !_presentateurIsSet)
        {
            _actualGame.Ending(false);
            SceneManager.LoadScene("Menu");
        }
    }

    private void BestPlayer()
    {
        _bestPlayer = "Player 1";
        _bestScore = -1;
        foreach (Player player in _actualGame._players)
        {
            if (player.score > _bestScore)
            {
                _bestScore = player.score;
                _bestPlayer = "Player " + player.ID;
                _colorBestPlayer = player.color;
            }
        }
    }

    private IEnumerator WaitToSpeak(float wait, string sentence, AudioClip clip, bool victory)
    {
        _winner.SetActive(false);
        subtitlesObject.SetActive(false);
        _coroutineRunning = true;
        yield return new WaitForSeconds(wait);
        _winner.SetActive(victory);
        subtitlesObject.SetActive(true);
        _subTitleText.text = sentence;
        CustomPlay(clip);

        if (victory)
        {
            yield return new WaitForSeconds(wait * 3);
            _audioSource.PlayOneShot(_trumpetSound);
            yield return new WaitForSeconds(wait * 6);
            _winner.SetActive(false);
            subtitlesObject.SetActive(false);
            _coroutineRunning = false;
        }
        else
            _coroutineRunning = false;
    }
}
