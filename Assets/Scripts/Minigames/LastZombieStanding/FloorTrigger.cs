using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloorTrigger : MonoBehaviour
{
    public delegate void FallInLavaAction(Player player);
    public FallInLavaAction OnFallInLava;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Utility.PLAYER_TAG) && OnFallInLava != null)
            OnFallInLava(other.gameObject.GetComponent<Player>());
    }
}
