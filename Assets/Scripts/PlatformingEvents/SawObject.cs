using UnityEngine;

public class SawObject : MonoBehaviour
{
    [SerializeField] private float knockBackHorizontalForce = 1000;
    [SerializeField] private float knockBackVerticalForce = 200;
    [SerializeField] private float stunDuration = 0.7f;
    private float damages = 15;
    
    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        Player player = other.gameObject.GetComponent<Player>();
        player.Rigidbody.AddForce(new Vector2(other.transform.position.x < transform.position.x ? -knockBackHorizontalForce : knockBackHorizontalForce, knockBackVerticalForce));
        player.UnitMovement.StunUnit(stunDuration);
        player.TakeAHitFromSawOrLaser(damages);
    }
}
