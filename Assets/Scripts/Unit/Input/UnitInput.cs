using UnityEngine;

public abstract class UnitInput : MonoBehaviour
{
    protected float _runSpeed;

    protected float _horizontalMovement;
    protected bool _jump;
    protected bool _jumpDown;
    protected bool _attack;
    protected bool _useItem;
    protected bool _interaction;

    public void Init(float runSpeed)
    {
        _runSpeed = runSpeed;
    }

    public abstract void UpdateInput();
}
