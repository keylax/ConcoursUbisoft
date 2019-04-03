using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameName : MonoBehaviour
{
    public GameObject nameObject;
    private Text _text;
    private string _textContent;
    private bool _isActive;
    private WaitForSeconds _timeFade;
    private bool _coroutineRunning;
    private bool _fade;

    // Manual Start
    public bool manualLaunch;
    //

    public void Init(string nameAsked)
    {
        _textContent = nameAsked;
    }

    void Start()
    {
        _fade = true;
        _text = nameObject.GetComponent<Text>();
        _textContent = "Minion Wave";
        _isActive = false;
        _timeFade = new WaitForSeconds(4f);
        _coroutineRunning = false;
    }

    void Update()
    {
        Color col = new Color(_text.color.r, _text.color.g, _text.color.b, _text.color.a);
        if (manualLaunch)
        {
            Launch();
            manualLaunch = false;
        }
        if (_isActive && _fade && _coroutineRunning)
            col.a += Time.deltaTime / 3;
        if (_isActive && _fade == false && _coroutineRunning)
            col.a -= Time.deltaTime / 3;
        if (_isActive && _coroutineRunning)
            _text.color = col;

        if (!_isActive)
            col.a = 0;
    }

    public void Launch()
    {
        _isActive = true;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        _text.text = _textContent;
        _fade = true;
        _coroutineRunning = true;
        yield return _timeFade;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        _fade = false;
        yield return _timeFade;
        _isActive = false;
        _coroutineRunning = false;
    }
}
