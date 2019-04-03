using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject[] menuBasicButtons;
    public GameObject[] menuQuitConfirmation;
    public GameObject[] menuLoading;
    private float _loadingTime = 0;
    private bool _lauchLoading = false;

    void Start()
    {
        unActiveList(menuQuitConfirmation);
    }

    void Update()
    {

        if (_lauchLoading == true)
        {
            activeList(menuLoading);
            _loadingTime += Time.deltaTime;
            GameObject.Find("LoadingSlider").GetComponent<Slider>().value += 0.01f;
            if (_loadingTime > 3)
            {
                _lauchLoading = false;
                unActiveList(menuLoading);
                SceneManager.LoadScene("erwan");
            }
        }
        else if (Input.GetKeyDown("escape"))
            quit();
    }

    public void quit()
    {
        unActiveList(menuBasicButtons);
        activeList(menuQuitConfirmation);
    }

    public void quitYes()
    {
        Application.Quit();
    }

    public void quitNo()
    {
        unActiveList(menuQuitConfirmation);
        activeList(menuBasicButtons);
    }

    public void unActiveList(GameObject[] tmp)
    {
        for (int i = 0; i < tmp.Length; i++)
            tmp[i].SetActive(false);
    }

    public void activeList(GameObject[] tmp)
    {
        for (int i = 0; i < tmp.Length; i++)
            tmp[i].SetActive(true);
    }

    public void lauchGame(string tmp)
    {
        _lauchLoading = true;
        _loadingTime = 0;
    }
}