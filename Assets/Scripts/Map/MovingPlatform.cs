using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private bool moveOnSpawn;
    [SerializeField] private Transform[] limiters;
    [SerializeField] private float speed = 3;

    private int _limiterIndex;
    private Transform _selectedLimiter;

    private MovingPlatform()
    {
        _limiterIndex = 0;
    }

    private void Awake()
    {
        enabled = moveOnSpawn;
        _selectedLimiter = limiters[0];
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, _selectedLimiter.position, Time.fixedDeltaTime * speed);

        if (transform.position == _selectedLimiter.position)
            SelectNextLimiter();
    }

    private void SelectNextLimiter()
    {
        if (++_limiterIndex == limiters.Length)
            _limiterIndex = 0;

        _selectedLimiter = limiters[_limiterIndex];
    }

    public void EnablePlatformMovement()
    {
        enabled = true;
    }

    public void DisablePlatformMovement()
    {
        enabled = false;
    }
}