using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private List<Player> _players;

    private GameObject _jauge;
    private GameObject _minigameUI;
    private List<PlayerUIManager> _persoUis;

    public GameObject persoUIExample;
    public RectTransform speaker;
    public RectTransform subtitles;
    public Text subtitlesText;
    public PauseUIManager pauseUI;
    public bool subtitlesActivated = true;
    public PollUIManager pollUIManager;
    public bool speakerActivated = false;
    public Text debugText;
    private bool _debug = false;
    
    private UIManager()
    {
        StaticObjects.UIManager = this;
    }

    void Start()
    {
        if (_debug)
            debugText.gameObject.SetActive(true);
        _jauge = transform.GetChild(1).gameObject;
        _minigameUI = transform.GetChild(2).gameObject;
        _players = StaticObjects.GameController.GetPlayers();
        _persoUis = new List<PlayerUIManager>();

        foreach (Player player in _players)
        {
            // create perso ui
            Vector3 persoUIPosition;
            Vector3 scale = new Vector3(1, 1, 1);
            if (player.ID > Mathf.CeilToInt((float) _players.Count / 2))
            {
                persoUIPosition = new Vector3(365 - (_players.Count - player.ID) * 60, 175, 0);
                scale.x = -1;
            }
            else
                persoUIPosition = new Vector3(-365 + 60 * (player.ID - 1), 175, 0);

            GameObject persoUI = Instantiate(persoUIExample, persoUIPosition, Quaternion.identity);
            persoUI.transform.localScale = scale;
            persoUI.transform.SetParent(transform, false);
            persoUI.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().color =
                player.color;
            _persoUis.Add(persoUI.GetComponent<PlayerUIManager>());
            _persoUis[_persoUis.Count - 1].SetPlayer(player);
        }
    }

    void Update()
    {
        Vector3 speakerPosition = speaker.localPosition;
        if (speakerActivated && speakerPosition.x >= 350)
            speakerPosition.x -= 10f;
        else if (!speakerActivated && speakerPosition.x <= 450)
            speakerPosition.x += 10f;
        speaker.localPosition = speakerPosition;
    }

    public IEnumerator PrintSubtitles(string text, float time)
    {
        speakerActivated = true;
        if (subtitlesActivated && text.Length != 0)
        {
            if (subtitlesText.text.Equals(""))
            {
                subtitles.gameObject.SetActive(true);
                subtitlesText.text = text;
                yield return 0;
            }
            // update subtitles frame size
            Vector3 size = subtitles.sizeDelta;
            size.x = subtitlesText.GetComponent<RectTransform>().sizeDelta.x + 20;
            subtitles.sizeDelta = size;
        }
        yield return 0;
    }

    public void HideSubtitles()
    {
        speakerActivated = false;
        subtitlesText.text = "";
        subtitles.gameObject.SetActive(false);
    }
}