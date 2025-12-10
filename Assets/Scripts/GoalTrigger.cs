using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public float maxBallSpeed = 2f;
    public float confirmTime = 0.15f;

    private bool goalPending = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GolfBall")) return;
        if (goalPending) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        if (rb.linearVelocity.magnitude > maxBallSpeed)
            return;

        StartCoroutine(ConfirmGoal(other.gameObject));
    }

    private System.Collections.IEnumerator ConfirmGoal(GameObject ball)
    {
        goalPending = true;
        yield return new WaitForSeconds(confirmTime);

        Collider trigger = GetComponent<Collider>();
        Collider ballCol = ball.GetComponent<Collider>();

        if (trigger.bounds.Intersects(ballCol.bounds))
        {
            Debug.Log("GOAL SCORED!");

            // TODO: Notify GameManager here
            // GameManager.Instance.OnGoalScored();
        }

        goalPending = false;
    }
}
