using UnityEngine;

/// <summary>
/// Attach this to the GolfClub child that has the CapsuleCollider (isTrigger = true).
/// Whenever the club passes through the ball, apply a *very small* impulse
/// based on swing speed, always at a 45-degree launch angle.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GolfClubHitBall : MonoBehaviour
{
    [Header("References")]
    public GolfClubSwingTracker swingTracker;   // assign GolfClubRoot here
    public string golfBallTag = "GolfBall";

    [Header("Hit Tuning (all very small)")]
    [Tooltip("Even the slowest tap is treated as at least this speed.")]
    public float minEffectiveSpeed = 0.1f;

    [Tooltip("Base impulse applied even for slow taps.")]
    public float baseImpulse = 0.1f;

    [Tooltip("Extra impulse per m/s of swing speed.")]
    public float swingSpeedMultiplier = 0.1f;

    [Tooltip("Clamp so shots never get too strong.")]
    public float maxImpulse = 0.8f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to the golf ball
        if (!other.CompareTag(golfBallTag))
            return;

        Rigidbody ballRb = other.attachedRigidbody;
        if (ballRb == null)
            return;

        // --- 1. Get swing velocity / speed ---
        Vector3 clubVel = swingTracker != null ? swingTracker.SwingVelocity : transform.forward;
        float speed = clubVel.magnitude;

        // Even very slow motion gets at least this speed
        float effectiveSpeed = Mathf.Max(speed, minEffectiveSpeed);

        // --- 2. Compute launch direction with 45° angle ---

        // Use swing direction projected onto the floor as "forward"
        Vector3 horiz = Vector3.ProjectOnPlane(clubVel, Vector3.up);

        // If swing velocity is tiny or vertical, fall back to club->ball direction
        if (horiz.sqrMagnitude < 1e-4f)
        {
            Vector3 clubPos = transform.position;
            Vector3 ballPos = other.transform.position;
            horiz = Vector3.ProjectOnPlane(ballPos - clubPos, Vector3.up);
        }

        if (horiz.sqrMagnitude < 1e-4f)
        {
            // Last resort
            horiz = Vector3.forward;
        }

        Vector3 horizDir = horiz.normalized;
        Vector3 launchDir = (horizDir + Vector3.up).normalized; // 45° up

        // --- 3. Compute impulse magnitude (very small overall) ---
        float impulseMagnitude = baseImpulse + effectiveSpeed * swingSpeedMultiplier;
        impulseMagnitude = Mathf.Min(impulseMagnitude, maxImpulse);

        // --- 4. Apply the impulse ---
        ballRb.AddForce(launchDir * impulseMagnitude, ForceMode.Impulse);

        // --- 5. Notify GameStateManager ---
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.NotifyBallHit(
                other.transform.position,
                ballRb.linearVelocity,
                other.transform.rotation,
                clubVel,
                speed
            );
        }
    }
}
