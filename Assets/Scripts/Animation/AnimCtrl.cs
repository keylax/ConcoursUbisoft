using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCtrl : MonoBehaviour
{
    private Animator _animator;
    private int attackHash = Animator.StringToHash("attack");
    private int jumpHash = Animator.StringToHash("jump");
    private int batHash = Animator.StringToHash("bat");
    private int teddyHash = Animator.StringToHash("teddy");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateMove(float speed)
    {
        _animator.SetFloat("speed", speed);
    }

    public void UpdateGrounded(bool grounded)
    {
        _animator.SetBool("grounded", grounded);
    }

    public void TriggerJump()
    {
        _animator.SetBool("grounded", false);
        _animator.SetTrigger(jumpHash);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(attackHash);
    }

    public void Flip()
    {
        transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
    }

    public void UseBat()
    {
        _animator.SetTrigger(batHash);
    }
    
    public void UseTeddy()
    {
        _animator.SetTrigger(teddyHash);
    }
}
