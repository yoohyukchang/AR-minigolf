using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Only count strokes when hit by a golf club
        if (other.GetComponent<GolfClubHitBall>() != null)
        {
            if (StrokeCounter.Instance != null)
            {
                StrokeCounter.Instance.IncrementStroke();
            }
        }
    }
}
