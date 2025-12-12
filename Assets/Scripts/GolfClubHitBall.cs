using UnityEngine;

/// <summary>
/// Attach this to the club mesh object (with a trigger collider).
/// Uses ClubStats so each club (iron, putter, etc.) can have different behavior.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GolfClubHitBall : MonoBehaviour
{
    [Header("References")]
    public GolfClubSwingTracker swingTracker;   // assign GolfClubRoot here
    public string golfBallTag = "GolfBall";

    [Header("Club Settings")]
    public ClubStats clubStats;                 // assign IronStats, PutterStats, etc.

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

        if (clubStats == null)
        {
            Debug.LogWarning("[GolfClubHitBall] No ClubStats assigned.");
            return;
        }

        Rigidbody ballRb = other.attachedRigidbody;
        if (ballRb == null)
            return;

        // 1. Get swing velocity / effective speed
        Vector3 clubVel = swingTracker != null ? swingTracker.SwingVelocity : transform.forward;
        float speed = clubVel.magnitude;
        float effectiveSpeed = Mathf.Max(speed, clubStats.minEffectiveSpeed);

        // 2. Horizontal direction on the floor
        Vector3 horiz = Vector3.ProjectOnPlane(clubVel, Vector3.up);

        if (horiz.sqrMagnitude < 1e-4f)
        {
            // Fallback: club → ball direction (flattened to floor)
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

        // 3. Build launch direction using clubStats.launchAngleDeg
        float angleRad = clubStats.launchAngleDeg * Mathf.Deg2Rad;
        Vector3 launchDir =
            (Mathf.Cos(angleRad) * horizDir + Mathf.Sin(angleRad) * Vector3.up).normalized;

        // 4. Impulse magnitude from clubStats
        float impulseMagnitude =
            clubStats.baseImpulse + effectiveSpeed * clubStats.swingSpeedMultiplier;
        impulseMagnitude = Mathf.Min(impulseMagnitude, clubStats.maxImpulse);

        float levelMul = (GameManager.Instance != null) ? GameManager.Instance.shotImpulseMultiplier : 1f;
        impulseMagnitude *= levelMul;

        // 5. Apply impulse
        ballRb.AddForce(launchDir * impulseMagnitude, ForceMode.Impulse);
    }
}
