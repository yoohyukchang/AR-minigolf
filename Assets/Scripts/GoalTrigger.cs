using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Confirm")]
    public float maxBallSpeed = 2f;
    public float confirmTime = 0.15f;

    [Header("Detection")]
    public string golfBallTag = "GolfBall";
    public LayerMask ballLayerMask = ~0;

    private SphereCollider _sphere;
    private bool goalPending = false;

    private void Awake()
    {
        _sphere = GetComponent<SphereCollider>();
        _sphere.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(golfBallTag)) return;
        if (goalPending) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        if (rb.linearVelocity.magnitude > maxBallSpeed)
            return;

        StartCoroutine(ConfirmGoal(other));
    }

    private IEnumerator ConfirmGoal(Collider ballCol)
    {
        goalPending = true;
        yield return new WaitForSeconds(confirmTime);

        // Compute world-space center + radius of this goal sphere
        Vector3 center = transform.TransformPoint(_sphere.center);

        // Handle non-uniform scaling safely:
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        float worldRadius = _sphere.radius * maxScale;

        // True overlap test: is the ball collider overlapping the sphere volume?
        // We test using the ball's closest point to the sphere center.
        Vector3 closest = ballCol.ClosestPoint(center);
        float distSq = (closest - center).sqrMagnitude;

        if (distSq <= worldRadius * worldRadius)
        {
            Debug.Log("GOAL SCORED!");
            GameManager.Instance.OnGoal();
        }

        goalPending = false;
    }
}
