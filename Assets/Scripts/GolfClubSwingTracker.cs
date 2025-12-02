using UnityEngine;

/// <summary>
/// Attach this to GolfClubRoot (under RightHandAnchor).
/// It continuously tracks how fast the club is moving.
/// </summary>
public class GolfClubSwingTracker : MonoBehaviour
{
    public Vector3 SwingVelocity { get; private set; }
    public float SwingSpeed => SwingVelocity.magnitude;

    [Header("Smoothing")]
    [Tooltip("How strongly to smooth velocity (0 = none, 1 = very smooth).")]
    [Range(0f, 1f)]
    public float velocitySmoothing = 0.3f;

    private Vector3 _lastPosition;
    private bool _initialized = false;

    private void Update()
    {
        if (!_initialized)
        {
            _lastPosition = transform.position;
            SwingVelocity = Vector3.zero;
            _initialized = true;
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 rawVel = (transform.position - _lastPosition) / dt;
        _lastPosition = transform.position;

        // Simple smoothing so the value isn't super noisy
        SwingVelocity = Vector3.Lerp(SwingVelocity, rawVel, 1f - velocitySmoothing);
    }
}
