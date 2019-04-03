using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    private AudioSource _audioSource;
    public AudioClip click;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = this.GetComponent<AudioSource>();
    }

    public void ExitGameAction()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void LaunchGameAction()
    {
        _audioSource.PlayOneShot(click);
        SceneManager.LoadScene("pierre");
    }

    public void GoToMenuAction()
    {
        if (_audioSource && click)
            _audioSource.PlayOneShot(click);
        StaticObjects.GameController.isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void ToggleSubtitles()
    {
        bool value = GetComponent<Toggle>().isOn;
        StaticObjects.UIManager.subtitlesActivated = value;
    }
    
    public void enterRoomName(string roomName)
    {
        StaticObjects.RoomName = roomName;
    }
}
