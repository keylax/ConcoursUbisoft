using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    private AMenuOption[] _options;
    private bool _moving = false;
    private float _timeBeforeResetMove = 0.3f;
    private float _lastMove;
    
    // Start is called before the first frame update
    void Start()
    {
        _options = GetComponentsInChildren<AMenuOption>(true);
        _options[0].isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("ButtonA_1"))
            Validate();
        if (_moving)
        {
            if (Time.time - _lastMove > _timeBeforeResetMove)
                _moving = false;
            return;
        }

        if (Input.GetAxisRaw("Vertical_1") < -0.1f)
        {
            GoDown();
            _moving = true;
            _lastMove = Time.time;
        }
        else if (Input.GetAxisRaw("Vertical_1") > 0.1f)
        {
            GoUp();
            _moving = true;
            _lastMove = Time.time;
        }
    }

    void GoDown()
    {
        for (int i = 0; i != _options.Length; ++i)
        {
            if (_options[i].isActive && i + 1 < _options.Length)
            {
                _options[i].isActive = false;
                _options[i + 1].isActive = true;
                return;
            }
        }
    }

    void GoUp()
    {
        for (int i = 0; i != _options.Length; ++i)
        {
            if (_options[i].isActive && i != 0)
            {
                _options[i].isActive = false;
                _options[i - 1].isActive = true;
                return;
            }
        }
    }

    void Validate()
    {
        foreach (AMenuOption o in _options)
            if (o.isActive)
                o.Validate();
    }
}
