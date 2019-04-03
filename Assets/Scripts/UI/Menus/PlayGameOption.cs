using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGameOption : AMenuOption
{   
    public override void Validate()
    {
        SceneManager.LoadScene("pierre");
    }
}
