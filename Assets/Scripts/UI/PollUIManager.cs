using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using UnityEngine.UI;

public class PollUIManager : MonoBehaviour
{
    private Poll _currentPoll;
    private int[] _currentResults;
    public Text pollNameText;
    public RectTransform pollPresentation;
    public Text pollPresentationText;
    public Text textExample;
    public RectTransform roomName;
    public GameObject jaugeExample;
    public RectTransform jaugeRectTransform;
    public Image leftImage;
    public Image rightImage;
    private List<RectTransform> _pollOptions;
    private List<Text> _pollOptionsText;
    private List<Text> _pollOptionsJaugeText;
    private RectTransform _rectTransform;
    private List<Color> _colors;
    private float _jaugeMaxSize = 10;
    private Vector3 _jaugeSize;
    public int numberOfOptions = 2;

    void Start()
    {
        _colors = new List<Color>
        {
            // Color.red,
            new Color(0.89f, 0.4628f, 0.8f),
            new Color(0.8431f, 0.6980f, 0.4235f)
            //Color.blue
        };
        StaticObjects.CrowdClient.VoteStatusNotifyMethod = UpdatePollUI;
        _pollOptions = new List<RectTransform>();
        _pollOptionsText = new List<Text>();
        _pollOptionsJaugeText = new List<Text>();
        _rectTransform = GetComponent<RectTransform>();
        _jaugeSize = jaugeRectTransform.sizeDelta;
        
        for (int i = 0; i != numberOfOptions; ++i)
        {
            //create score gauge
            GameObject tempJauge = Instantiate(jaugeExample, new Vector3(0, 0, 0), Quaternion.identity);
            tempJauge.transform.SetParent(transform, false);

            _pollOptions.Add(tempJauge.GetComponent<RectTransform>());
            tempJauge.GetComponent<RawImage>().color = _colors[i];
            _pollOptions[_pollOptions.Count - 1].sizeDelta = new Vector3((float)1 / numberOfOptions * _jaugeSize.x, 20, _jaugeSize.z);
            
            // create scores number inside jauge
            Text tempTextBox = Instantiate(textExample, new Vector3(0, 0, 0), Quaternion.identity);
            tempTextBox.alignment = TextAnchor.MiddleCenter;
            tempTextBox.transform.SetParent(tempJauge.transform, false);
            tempTextBox.color = Color.white;
            tempTextBox.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            _pollOptionsJaugeText.Add(tempTextBox);
        }
        _rectTransform.localPosition = new Vector3(0, -250, 0);
    }

    
    public void SetRoomName(string roomNameString)
    {
        roomName.GetComponentInChildren<Text>().text = "Room name : " + roomNameString;
    }
    
    private bool SetImages()
    {
        List<MiniGame> minigames = null;
        List<PlatformingEvent> platformingEvents = null;
        minigames = StaticObjects.MapCreationController.GetCurrentPollMiniGames();
        platformingEvents = StaticObjects.MapCreationController.GetCurrentPollPlatformingEvents();
        Vector3 size = pollPresentation.sizeDelta;
        Vector3 position = pollPresentation.transform.localPosition;
        if (minigames != null && minigames.Count >= numberOfOptions)
        {
            leftImage.sprite = minigames[0].logo;
            rightImage.sprite = minigames[1].logo;
            pollNameText.text = minigames[0].GetName() + " VS " + minigames[1].GetName();
            size.x = 190;
            position.x = -300;
        }
        else if (platformingEvents != null && platformingEvents.Count >= numberOfOptions)
        {
            leftImage.sprite = platformingEvents[0].logo;
            rightImage.sprite = platformingEvents[1].logo;
            pollNameText.text = platformingEvents[0].GetName() + " VS " + platformingEvents[1].GetName();
            position.x = -330;
            size.x = 240;
        }
        else
            return false;
        pollPresentation.sizeDelta = size;
        pollPresentation.transform.localPosition = position;
        return true;
    }
    

    // Update is called once per frame
    void Update()
    {
        _currentPoll = StaticObjects.CrowdClient.currentPoll;
        if (_currentPoll == null && !SetImages())
        {
            // pollNameText.text = "No poll ongoing";
            if (_currentResults == null || _currentResults.Length == 0)
                _currentResults = new int[numberOfOptions];
            for (int i = 0; i != numberOfOptions; ++i)
                _currentResults[i] = 0;
            Vector3 jaugeLoc = _rectTransform.localPosition;
            if (jaugeLoc.y > -250)
            {
                jaugeLoc.y -= 1f;
                _rectTransform.localPosition = jaugeLoc;
            }
        }
        else if (SetImages())
        {
            // update location
            float y_location;
            float jaugeScale;
            if (StaticObjects.MapTransitionController.platforming)
            {
                y_location = -205;
                jaugeScale = 1;
                // put subtitles higher
                StaticObjects.UIManager.subtitles.localPosition = new Vector3(0, -180, 0);
            }
            else
            {
                y_location = -209;
                jaugeScale = 0.75f;
                StaticObjects.UIManager.subtitles.localPosition = new Vector3(0, -185, 0);
            }
            Vector3 jaugeLoc = _rectTransform.localPosition;
            if (jaugeLoc.y < y_location)
            {
                jaugeLoc.y += 1f;
                _rectTransform.localPosition = jaugeLoc;
            }
            _rectTransform.localScale = new Vector3(jaugeScale, jaugeScale, 1);
            if (_currentResults == null || _currentResults.Length == 0)
            {
                _currentResults = new int[numberOfOptions];
                _currentResults.Initialize();
            }

            pollPresentationText.text = StaticObjects.MapTransitionController.platforming ? "Vote now for the next minigame!" : "Vote now for the next platforming event!";
            // SetImages();   
            // machin
            int totalVoters = 0;
            float scoreToShare = 0;
            float nextCoordinate = 0;
            float numberOfNull = 0;
            bool even = false;
    
            for (int i = 0; i != numberOfOptions; ++i)
                totalVoters += _currentResults[i];
    
            if (totalVoters != 0)
                for (int i = 0; i != numberOfOptions; ++i)
                    if ((float) _currentResults[i] / totalVoters < 0.05f)
                    {
                        scoreToShare += ((float) _currentResults[i] / totalVoters) - 0.05f;
                        numberOfNull++;
                    }
            float cumulate_scale = 0;
            for (int i = 0; i != numberOfOptions; ++i)
            {
                // update jauges size
                float scale;
    
                if (totalVoters == 0)
                    scale = (float) 1 / numberOfOptions;
                else
                {
                    if ((float) _currentResults[i] / totalVoters < 0.05f)
                        scale = 0.05f;
                    else
                        scale = ((float) _currentResults[i] / totalVoters) + (scoreToShare / (numberOfOptions - numberOfNull));
                    scale = ((float) (_pollOptions[i].sizeDelta.x / _jaugeSize.x) +
                             (float) ((scale - (_pollOptions[i].sizeDelta.x / _jaugeSize.x)) / 30));
                }
    
                cumulate_scale += scale;
                _pollOptions[i].sizeDelta = new Vector3(scale * _jaugeSize.x, _jaugeSize.y, _jaugeSize.z);
                _pollOptions[i].transform.localPosition =
                    new Vector3(nextCoordinate + ((float) _jaugeSize.x * ((float) scale / 2)) - _jaugeSize.x / 2, jaugeRectTransform.transform.localPosition.y, 0);
                nextCoordinate += ((float) _jaugeSize.x * (scale / 2)) * 2;
    
                // update score text
                if (totalVoters != 0)
                    _pollOptionsJaugeText[i].text = (100 * _currentResults[i] / totalVoters).ToString() + "%";
                else
                    _pollOptionsJaugeText[i].text = (0).ToString() + "%";
                _pollOptionsJaugeText[i].fontSize = (int) (4 * Mathf.Log(_pollOptions[i].sizeDelta.x));
            }
                    
        }
    }

    void UpdatePollUI(int[] results)
    {
        _currentResults = results;
    }
}
