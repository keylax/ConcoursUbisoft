using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Base : MonoBehaviour
{
    private BoxCollider _collider;

    private Arrow _arrowManager;
    // On player drop flag event
    public delegate void DropAction(Transform instigator);
    public event DropAction OnDropFlag;
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _arrowManager = GetComponentInChildren<Arrow>();
        _collider.isTrigger = true;
        _collider.enabled = false;
    }

    public void SetActive(bool active)
    {
        _collider.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.transform.CompareTag(Utility.PLAYER_TAG) && OnDropFlag != null)
            OnDropFlag(other.transform);
    }

    public void SetArrowActive(bool active)
    {
        _arrowManager.gameObject.SetActive(active);
    }
}
