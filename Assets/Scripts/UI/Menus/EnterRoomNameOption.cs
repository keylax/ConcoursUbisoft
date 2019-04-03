using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterRoomNameOption : AMenuOption
{
    private InputField _inputField;
    protected void Start()
    {
        base.Start();
        _inputField = GetComponent<InputField>();
    }

    public override void Validate()
    {
        StaticObjects.RoomName = _inputField.text;
    }

    private void EnterRoomName(string roomName)
    {
        StaticObjects.RoomName = roomName;   
    }
}
