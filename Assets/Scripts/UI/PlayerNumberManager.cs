using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNumberManager : MonoBehaviour
{
    private Scrollbar _slider;

    public String launchedScene;

    public Text numberOfPlayersText;

    public Text missControllerWarning;

    private AudioSource _audioSource;
    public AudioClip click;

    void Start()
    {
        _audioSource = this.GetComponent<AudioSource>();
        _slider = GetComponent<Scrollbar>();
        if (StaticObjects.PlayerNumber == 0)
        {
            StaticObjects.PlayerNumber = 2;
            numberOfPlayersText.text = "Number of players : " + StaticObjects.PlayerNumber;
        }

        _slider.value = StaticObjects.PlayerNumber == 2 ? 0 : 1;
    }

    void Update()
    {
        string[] controllers = Input.GetJoystickNames();
        numberOfPlayersText.text = "Number of players : " + StaticObjects.PlayerNumber;
        missControllerWarning.text =
            "You must connect at least 2 controllers to have 4 players ! You currently have " + (controllers.Length - controllers.Count(e => e == "")) + " connected.";
    }

    public void OnValueChange()
    {
        if (!_slider)
            return;
        string[] controllers = Input.GetJoystickNames();
        missControllerWarning.gameObject.SetActive(false);
        if (_slider.value < 0.5f)
            StaticObjects.PlayerNumber = 2;
        else if ((controllers.Length - controllers.Count(e => e == "")) + 2 >= 4)
        {
            StaticObjects.PlayerNumber = 4;
            _audioSource.PlayOneShot(click);
        }
        else
        {
            _slider.value = 0;
            missControllerWarning.gameObject.SetActive(true);
            missControllerWarning.text =
                "You must plug at least 2 controllers to have 4 players ! You currently have " + (controllers.Length - controllers.Count(e => e == "")) + " plugged.";
        }
    }
}
