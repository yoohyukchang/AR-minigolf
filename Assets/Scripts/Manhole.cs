using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Manhole : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GolfBall")) return;

        BallController bc = FindObjectOfType<BallController>();
        if (bc != null)
        {
            bc.OnManholeHit();
        }
    }
}
