using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUIManager : MonoBehaviour
{
    
    [SerializeField] private Text controllerMissingText;
    private AudioSource _audioSource;
    public AudioClip clip;

    void Start()
    {
        _audioSource = this.GetComponent<AudioSource>();        
    }

    void Update()
    {
        
    }
    
    void MySetActive(bool active)
    {
        for (int i = 0; i != transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }
    
    public void Pause(bool controllerMissing)
    {
        MySetActive(true);
        // _audioSource.PlayOneShot(clip);
        StaticObjects.UIManager.debugText.text += "pause ui manager = " + controllerMissing;
        if (controllerMissing)
        {
            controllerMissingText.gameObject.SetActive(true);
            Debug.Log("controller missing");
        }
        else
        {
            controllerMissingText.gameObject.SetActive(false);
        }
    }
    
    public void UnPause()
    {
        MySetActive(false);
        controllerMissingText.gameObject.SetActive(false);
    }
}
