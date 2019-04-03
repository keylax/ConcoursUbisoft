using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpashScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Text welcomeText;
    private float _welcomeTime = 0;
    private int _durationWelcome = 4;
    private float _colorTransp = 0;
    
    void Start()
    {
        _welcomeTime = 0;
        MySetActive(true);
    }

    void MySetActive(bool active)
    {
        for (int i = 0; i != transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _welcomeTime += Time.deltaTime;
        if (_welcomeTime < _durationWelcome)
        {
            _colorTransp += 0.005f;
            showIntro();
        }
        else
        {
            MySetActive(false);
        }
    }
    
    public void showIntro()
    {
        welcomeText.color = new Color(1, 1, 1, _colorTransp);
    }
}
