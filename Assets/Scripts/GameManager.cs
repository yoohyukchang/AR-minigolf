using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject goalScreen;

    [Header("Gameplay")]
    public GoalSpawner goalSpawner;
    public Transform ballTransform;
    private Vector3 ballStartPos;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ballStartPos = ballTransform.position; // Save starting point
        goalScreen.SetActive(false);
    }

    public void OnGoal()
    {
        // Show UI
        goalScreen.SetActive(true);

        // Freeze ball
        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void RestartGame()
    {
        // Hide UI
        goalScreen.SetActive(false);

        // Reset ball
        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ballTransform.position = ballStartPos;

        // Force new goal spawn
        goalSpawner.ResetSpawner();
    }
}
