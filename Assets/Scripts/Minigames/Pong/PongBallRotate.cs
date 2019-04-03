using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBallRotate : MonoBehaviour
{
    private float zRotation;
    [SerializeField]private float _rotationSpeed;

    private void Start()
    {
        zRotation = 0.0f;
    }

    public void UpdateRotation()
    { 
        zRotation += _rotationSpeed;

        if (zRotation > 360.0f)
        {
            zRotation = 0.0f;
        }

        transform.localRotation = Quaternion.Euler(0,0, zRotation);
    }
}
