using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameUIManager : MonoBehaviour
{
    private List<GameObject> _persoMinigameScores;
    private List<Player> _players;
    private MinigameName _displayName;
    private bool _nameDisplayed;
    [SerializeField] private GameObject persoMinigameScoreExample;
    [SerializeField] private Sprite FirstPlaceImage;

    // Start is called before the first frame update
    void Start()
    {
        _nameDisplayed = false;
        _displayName = this.GetComponent<MinigameName>();
        _players = StaticObjects.GameController.GetPlayers();
        _persoMinigameScores = new List<GameObject>();
        foreach (Player player in _players)
        {
            // create perso minigame score
            Vector3 persoMGSPosition;
            if (player.ID > Mathf.CeilToInt((float)_players.Count / 2))
                persoMGSPosition = new Vector3(175 - (_players.Count - player.ID) * 50, 0, 0);
            else
                persoMGSPosition = new Vector3(-175 + 50 * (player.ID - 1), 0, 0);
            GameObject persoMinigameScore = Instantiate(persoMinigameScoreExample, persoMGSPosition, Quaternion.identity); // just to put them left and right of the minigame timer
            persoMinigameScore.transform.SetParent(transform, false);
            persoMinigameScore.GetComponentInChildren<Text>().color = player.color;
            _persoMinigameScores.Add(persoMinigameScore);
            persoMinigameScore.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = FirstPlaceImage;
        }
    }

    void MySetActive(bool active)
    {
        for (int i = 0; i != transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    private void DisplayName(bool active)
    {
        if (active && _nameDisplayed == false)
        {
            _nameDisplayed = true;
            if (!StaticObjects.MapCreationController.IsBossMap()){
                _displayName.Init(StaticObjects.MapCreationController.GetActiveMinigame().GetName());
            } else
            {
                _displayName.Init("Surprise!");
            }
            _displayName.Launch();
        }
    }

    void Update()
    {
        if (!StaticObjects.MapTransitionController.platforming && StaticObjects.MapCreationController.GetActiveMinigame() != null && !StaticObjects.CameraController._cameraIsMovingToNewPosition && _nameDisplayed == false)
        {
            DisplayName(true);
            _nameDisplayed = true;
        }
        if (StaticObjects.MapTransitionController.platforming)
            _nameDisplayed = false;
        if (!StaticObjects.MapTransitionController.platforming && StaticObjects.MapCreationController.GetActiveMinigame() != null && StaticObjects.MapCreationController.GetActiveMinigame().GetIsActive())
        {
            int[] minigameScores = null;
            Player best_minigame_player = null;

            MySetActive(true);
            float time = (StaticObjects.MapCreationController.GetActiveMinigame().maxMiniGameDuration - (Time.time - StaticObjects.MapCreationController.GetActiveMinigame().startTime));
            Text timer = gameObject.GetComponentInChildren<Text>();
            if (time < StaticObjects.MapCreationController.GetActiveMinigame().maxMiniGameDuration * 0.1)
                timer.color = Color.red;
            else
                timer.color = Color.white;
            string minutes = ((int)(time / 60)).ToString();
            string seconds = Mathf.CeilToInt((float)time % 60).ToString();
            if (seconds.Length == 1)
                seconds = "0" + seconds;
            if (Mathf.CeilToInt((float)time % 60) == 60)
            {
                minutes = ((int)(time / 60) + 1).ToString();
                seconds = "00";
            }
            if (minutes.Length == 1)
                minutes = "0" + minutes;
            timer.text = minutes + " : " + seconds;

            minigameScores = StaticObjects.MapCreationController.GetActiveMinigame().GetScores();
            for (int i = 0; i != minigameScores.Length; i++)
            {
                if (best_minigame_player == null || minigameScores[i] > minigameScores[_players.IndexOf(best_minigame_player)])
                    best_minigame_player = _players[i];
            }
            bool even2 = false;
            if (best_minigame_player != null)
            {
                for (int i = 0; i != minigameScores.Length; i++)
                {
                    if (i != _players.IndexOf(best_minigame_player) && minigameScores[_players.IndexOf(best_minigame_player)] == minigameScores[i])
                        even2 = true;
                }
            }
            if (even2)
                best_minigame_player = null;
            for (int i = 0; i != _players.Count; ++i)
            {
                // set minigame score
                Text persoMinigameScore = _persoMinigameScores[i].GetComponentInChildren<Text>();
                if (StaticObjects.MapTransitionController.platforming)
                {
                    _persoMinigameScores[i].SetActive(false);
                }
                else if (StaticObjects.MapCreationController.GetActiveMinigame() != null)
                {
                    if (minigameScores != null)
                    {
                        _persoMinigameScores[i].SetActive(true);
                        if (_players[i].Equals(best_minigame_player)) // give medal
                            _persoMinigameScores[i].transform.GetChild(2).gameObject.SetActive(true);
                        else
                            _persoMinigameScores[i].transform.GetChild(2).gameObject.SetActive(false);
                        persoMinigameScore.text = minigameScores[i].ToString();
                    }
                }
            }
        }
        else
        {
            MySetActive(false);
        }
    }
}
