using UnityEngine;

/// <summary>
/// Attach to the golf ball to continuously track its position and velocity.
/// Updates GameStateManager for observer synchronization.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallStateTracker : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _lastPosition;
    private float _updateInterval = 0.05f; // 20Hz updates
    private float _nextUpdateTime;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (Time.time < _nextUpdateTime) return;
        _nextUpdateTime = Time.time + _updateInterval;

        // Only update if ball has moved significantly
        Vector3 currentPos = transform.position;
        if (Vector3.Distance(currentPos, _lastPosition) > 0.001f)
        {
            _lastPosition = currentPos;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.NotifyBallMoved(currentPos, _rigidbody.linearVelocity);
            }
        }
    }
}
