using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JaugeManager : MonoBehaviour
{
    [SerializeField] private GameObject jaugeExample;
    [SerializeField] private Text textExample;
    
    private Vector3 _jaugeSize;
    private List<RectTransform> _scoreJauges;
    private List<Player> _players;
    private List<Text> _scoreBoxes;
    private RectTransform _rectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        _players = StaticObjects.GameController.GetPlayers(); 
        _jaugeSize = GetComponent<RectTransform>().sizeDelta;
        _rectTransform = GetComponent<RectTransform>();
        _scoreJauges = new List<RectTransform>();
        _scoreBoxes = new List<Text>();
        
        foreach (Player player in _players)
        {
            
            //create score gauge
            GameObject tempJauge = Instantiate(jaugeExample, new Vector3(0, 0, 0), Quaternion.identity);
            tempJauge.transform.SetParent(transform, false);

            _scoreJauges.Add(tempJauge.GetComponent<RectTransform>());
            tempJauge.GetComponent<RawImage>().color = player.color;
            _scoreJauges[_scoreJauges.Count - 1].sizeDelta = new Vector3((float)1 / _players.Count * _jaugeSize.x, _jaugeSize.y, _jaugeSize.z);
            
            // create scores number inside jauge
            Text tempTextBox = Instantiate(textExample, new Vector3(0, 0, 0), Quaternion.identity);
            tempTextBox.alignment = TextAnchor.MiddleCenter;
            tempTextBox.transform.SetParent(tempJauge.transform, false);
            tempTextBox.color = Color.white;
            tempTextBox.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            _scoreBoxes.Add(tempTextBox);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float totalScore = 0;
        float scoreToShare = 0;
        float nextCoordinate = 0;
        float numberOfNull = 0;
        bool even = false;

        foreach (Player player in _players)
        {
            totalScore += player.score;
        }

        if (totalScore != 0)
            foreach (Player player in _players)
                if (player.score / totalScore < 0.05f)
                {
                    scoreToShare += (player.score / totalScore) - 0.05f;
                    numberOfNull++;
                }

        float cumulate_scale = 0;
        if (!StaticObjects.MapTransitionController.platforming)
        {
            Vector3 jaugeLoc = _rectTransform.localPosition;
            if (jaugeLoc.y < 210)
            {
                jaugeLoc.y += 0.1f;
                _rectTransform.localPosition = jaugeLoc;
            }
        }
        else
        {
            Vector3 jaugeLoc = _rectTransform.localPosition;
            if (jaugeLoc.y > 200)
            {
                jaugeLoc.y -= 0.1f;
                _rectTransform.localPosition = jaugeLoc;
            }
        }

        for (int i = 0; i != _players.Count; ++i)
        {
            // update jauges size
            float scale;

            if (totalScore == 0)
                scale = (float) 1 / _players.Count;
            else
            {
                if ((float) _players[i].score / totalScore < 0.05f)
                    scale = 0.05f;
                else
                    scale = (_players[i].score / totalScore) + (scoreToShare / (_players.Count - numberOfNull));
                scale = ((float) (_scoreJauges[i].sizeDelta.x / _jaugeSize.x) +
                         (float) ((scale - (_scoreJauges[i].sizeDelta.x / _jaugeSize.x)) / 30));
            }

            cumulate_scale += scale;
            _scoreJauges[i].sizeDelta = new Vector3(scale * _jaugeSize.x, _jaugeSize.y, _jaugeSize.z);
            _scoreJauges[i].transform.localPosition =
                new Vector3(nextCoordinate + ((float) _jaugeSize.x * ((float) scale / 2)) - _jaugeSize.x / 2, 0, 0);
            nextCoordinate += ((float) _jaugeSize.x * (scale / 2)) * 2;

            // update score text
            _scoreBoxes[i].text = ((int) _players[i].score).ToString();
            _scoreBoxes[i].fontSize = (int) (4 * Mathf.Log(_scoreJauges[i].sizeDelta.x));
        }
    }
}
