using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Musics")]
    [SerializeField] private GameObject _beginObject;
    [SerializeField] private GameObject _loopObject;

    private AudioSource _begin;
    private AudioSource _loop;
    private bool _endingGame;

    void Start()
    {
        _endingGame = false;
        _begin = _beginObject.GetComponent<AudioSource>();
        _loop = _loopObject.GetComponent<AudioSource>();
        StaticObjects.BackgroundMusic = this;
    }

    void Update()
    {
        if (_endingGame) return;

        if (!_begin.isPlaying && !_loop.isPlaying)
            _loop.Play();
    }

    public void PauseMusic()
    {
        _begin.Pause();
        _loop.Pause();
        _endingGame = true;
    }
}
