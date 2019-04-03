using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisapearingPlatform : MonoBehaviour
{
    private float _shakeTime;
    private Rigidbody _rigidbody;
    private BoxCollider[] _collider;
    private Vector3 _originPosition;
    private LastZombieStandingMinigame _mgRef;

    private float _speed = 15f;
    private float _amount = .3f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponentsInChildren<BoxCollider>();
    }

    public void MGStart(LastZombieStandingMinigame mgRef)
    {
        _mgRef = mgRef;
        _originPosition = transform.position;
    }

    public void Drop()
    {
        _shakeTime = 6f;
        StartCoroutine(ShakeAndDrop());
    }

    private IEnumerator ShakeAndDrop()
    {
        do {
            _shakeTime -= Time.deltaTime;
            Vector3 newPosition = _originPosition;
            newPosition.y += Time.deltaTime * .5f;
            newPosition.x += Mathf.Sin(Time.time * _speed) * _amount;
            transform.position = newPosition;
            yield return null;
        } while (_shakeTime > 2f);

        if (!_mgRef.GetActive())
        {
            ResetPosition();
            yield break;
        }
        _mgRef.OnPlatformIsAboutToFall();
        yield return null;
        
        do {
            _shakeTime -= Time.deltaTime;
            Vector3 newPosition = _originPosition;
            newPosition.y += Time.deltaTime * .5f;
            newPosition.x += Mathf.Sin(Time.time * _speed * 3f) * _amount;
            transform.position = newPosition;
            yield return null;
        } while (_shakeTime > 0);
        
        if (!_mgRef.GetActive())
        {
            ResetPosition();
            yield break;
        }

        // Fall
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        foreach (BoxCollider c in _collider)
            c.enabled = false;
        
        yield return new WaitForSeconds(6f);
        
        // Stop Fall
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        foreach (BoxCollider c in _collider)
            c.enabled = true;
        
        if (!_mgRef.GetActive())
        {
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        // Just in case the platform was falling when this is called
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        foreach (BoxCollider c in _collider)
            c.enabled = true;
        
        transform.position = _originPosition;
    }
}
