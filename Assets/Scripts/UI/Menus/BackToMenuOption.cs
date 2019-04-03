using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToMenuOption : AMenuOption
{
    public override void Validate()
    {
        if (!gameObject.activeSelf)
            return;
        StaticObjects.GameController.isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
