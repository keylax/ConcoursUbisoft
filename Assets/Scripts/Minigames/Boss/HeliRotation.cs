using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliRotation : MonoBehaviour
{
    private float yRotation;
    [SerializeField] private float _rotationSpeed;

    private void Start()
    {
    }

    public void Update()
    {

        transform.localRotation *= Quaternion.AngleAxis(_rotationSpeed * Time.deltaTime, Vector3.up);
    }
}
