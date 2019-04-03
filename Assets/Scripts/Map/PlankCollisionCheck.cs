using System.Collections.Generic;
using UnityEngine;

public class PlankCollisionCheck : MonoBehaviour
{
    [SerializeField] private Collider plankHitbox;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) return;

        Physics.IgnoreCollision(plankHitbox, other);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) return;


        Physics.IgnoreCollision(plankHitbox, other, false);
    }
}
