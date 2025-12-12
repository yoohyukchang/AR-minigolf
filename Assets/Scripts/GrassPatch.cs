using UnityEngine;

public class GrassPatch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GolfBall")) return;

        BallController bc = other.GetComponentInParent<BallController>();
        if (bc != null)
            bc.EnterGrass();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("GolfBall")) return;

        BallController bc = other.GetComponentInParent<BallController>();
        if (bc != null)
            bc.ExitGrass();
    }
}
