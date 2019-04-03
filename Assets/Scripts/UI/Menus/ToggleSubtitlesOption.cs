using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSubtitlesOption : AMenuOption
{
    public override void Validate()
    {
        bool value = GetComponent<Toggle>().isOn;
        StaticObjects.UIManager.subtitlesActivated = value;
    }
}
