using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInput : UnitInput
{
    public GameObject uiControllerDisconnected;

    private Dictionary<string, KeyCode> _inputKeys;
    
    public bool nerfedMovement;

    private Player _player;

    public override void UpdateInput()
    {
        _player.UnitMovement.Move((_player.firstInPlatforming || !StaticObjects.MapTransitionController.platforming ? 1 : 1 + _player.movementAdvantageIfBehind) * (nerfedMovement ? 1 - _player.remoteMovementHandicap : 1) *_horizontalMovement * Time.fixedDeltaTime, _jump, _jumpDown, _attack, _useItem, _interaction);
        _jump = false;
        _attack = false;
        _useItem = false;
        _interaction = false;
    }

    private void Start()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        if (StaticObjects.GameController._ending) return; // If endin game, do nothing

        //TODO: A lot of expensive methods used here, can probably clean it up
        string[] controllers = Input.GetJoystickNames();
        if (_inputKeys != null)
            ClassicInput(); // Clavier
        else if (controllers != null && controllers.Length >= _player.ID && controllers[_player.ID - 1] != "")
        {
            if (StaticObjects.GameController.isPaused && !StaticObjects.GameController.playerPaused)
                PauseGame(false);
            ControllerInput(); // Manette
        }
        else
        {
            // Debug.LogWarning("Player " + _player.ID + " doesn't have any input...");
            if (!StaticObjects.GameController.isPaused)
                PauseGame(false);
        }
    }

    private void ClassicInput()
    {
        _horizontalMovement = 0;

        if (Input.GetKey(_inputKeys["LEFT"]) && !Input.GetKey(_inputKeys["RIGHT"]))
        {
            _horizontalMovement = -_runSpeed;
        }
        else if (!Input.GetKey(_inputKeys["LEFT"]) && Input.GetKey(_inputKeys["RIGHT"]))
        {
            _horizontalMovement = _runSpeed;
        }

        if (Input.GetKeyDown(_inputKeys["UP"]))
        {
            _jump = true;
        }

        _jumpDown = Input.GetKey(_inputKeys["DOWN"]);

        if (Input.GetKeyDown(KeyCode.Escape) && _inputKeys["UP"].Equals(KeyCode.Space))
        {
            PauseGame(true);
        }

        if (Input.GetKeyDown(_inputKeys["ATTACK"]))
        {
            _attack = true;
        }

        if ((_player.jetpackActivation && Input.GetKey(_inputKeys["ITEM"])) || (!_player.jetpackActivation && Input.GetKeyDown(_inputKeys["ITEM"])))
        {
            _useItem = true;
        }

        if (Input.GetKeyDown(_inputKeys["INTERACTION"]))
        {
            _interaction = true;
        }
    }

    private void ControllerInput()
    {
        float horizontalController = Input.GetAxisRaw("Horizontal_" + _player.ID);
        float verticalController = Input.GetAxisRaw("Vertical_" + _player.ID);
        string jumpController = "ButtonA_" + _player.ID;
        string objectAttack = "ButtonB_" + _player.ID;
        string simpleAttack = "ButtonX_" + _player.ID;
        string interaction = "ButtonY_" + _player.ID;
        string pauseController = "Start_" + _player.ID;

        _horizontalMovement = 0;

        if (horizontalController < -0.1f || horizontalController > 0.1f)
        {
            _horizontalMovement = _runSpeed * horizontalController;
        }

        if (Input.GetButtonDown(jumpController))
        {
            _jump = true;
        }

        if (Input.GetButtonDown(pauseController))
        {
            PauseGame(true);
        }

        if (Input.GetButtonDown(simpleAttack))
        {
            _attack = true;
        }

        if ((_player.jetpackActivation && Input.GetButton(objectAttack)) || (!_player.jetpackActivation && Input.GetButtonDown(objectAttack)))
        {
            _useItem = true;
        }

        if (Input.GetButtonDown(interaction))
        {
            _interaction = true;
        }

        _jumpDown = verticalController < -0.6f && verticalController >= -1;
    }

    private bool CheckControllersConnected()
    {
        // TODO: Can this be extracted and obtained only once at runtime?
        string[] controllers = Input.GetJoystickNames();

        if (controllers.Length > 0 && controllers.Any(string.IsNullOrEmpty))
        {
            // PauseGame();
            DisplayControllerDisconnected(true);
            return false;
        }

        DisplayControllerDisconnected(false);
        return true;
    }

    private void DisplayControllerDisconnected(bool active)
    {
        if (uiControllerDisconnected)
            uiControllerDisconnected.SetActive(active);
    }

    private void PauseGame(bool wantedByPlayer)
    {
        Debug.Log("PAUSE");
        StaticObjects.GameController.SummonPause(wantedByPlayer);
    }

    public void SetInputKeys(Dictionary<string, KeyCode> inputKeys)
    {
        _inputKeys = inputKeys;
    }

    public Dictionary<string, KeyCode> GetInputKeys()
    {
        return _inputKeys;
    }
}
