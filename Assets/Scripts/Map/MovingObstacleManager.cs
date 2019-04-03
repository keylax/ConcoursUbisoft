using System.Collections;
using UnityEngine;

public class MovingObstacleManager : MonoBehaviour
{
    [SerializeField] private float obstacleYMovementOnTrigger;
    [Range(0, 10)] [SerializeField] private float obstacleMovementSpeed = 10;
    private AudioSource _audioSource;
    public AudioClip clip;


    private Vector3 _targetPosition;

    private void Awake()
    {
        _targetPosition = transform.position + Vector3.up * obstacleYMovementOnTrigger;
        _audioSource = this.GetComponent<AudioSource>();
    }

    public void MoveWall()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        _audioSource.PlayOneShot(clip);
        while (transform.position != _targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, obstacleMovementSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
