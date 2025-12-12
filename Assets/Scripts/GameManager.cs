using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject goalUIRoot; // Assign GoalUICanvas here (NOT GoalScreen)

    [Header("Gameplay")]
    public GoalSpawner goalSpawner;
    public Transform ballTransform;
    private Vector3 ballStartPos;

    public ObstacleSpawner obstacleSpawner;
    public StartGameManager startGameManager;

    [System.Serializable]
    public struct LevelBallTuning
    {
        [Tooltip("Multiplier applied to club impulse force.")]
        public float shotImpulseMultiplier;

        [Tooltip("Maximum linear velocity for the ball (0 = no limit).")]
        public float maxBallSpeed;
    }

    [Header("Level Ball Tuning (index 0 = Level1, 1 = Level2, 2 = Level3)")]
    public LevelBallTuning[] levelBallTunings = new LevelBallTuning[3]
    {
        new LevelBallTuning { shotImpulseMultiplier = 0.85f, maxBallSpeed = 2.5f }, // Level 1
        new LevelBallTuning { shotImpulseMultiplier = 1.00f, maxBallSpeed = 3.5f }, // Level 2
        new LevelBallTuning { shotImpulseMultiplier = 1.10f, maxBallSpeed = 4.5f }  // Level 3
    };

    [Header("Runtime Values")]
    public int currentLevel = 1;
    public float shotImpulseMultiplier = 1f;
    public float maxBallSpeed = 0f;

    public void ApplyLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);
        var tune = levelBallTunings[currentLevel - 1];
        shotImpulseMultiplier = Mathf.Max(0f, tune.shotImpulseMultiplier);
        maxBallSpeed = Mathf.Max(0f, tune.maxBallSpeed);

        Debug.Log($"[GameManager] Level {currentLevel} | Impulse x{shotImpulseMultiplier} | MaxSpeed={maxBallSpeed}");
    }

    private void FixedUpdate()
    {
        if (ballTransform == null) return;

        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) return;

        // Clamp top speed each physics tick
        if (maxBallSpeed > 0f)
        {
            Vector3 v = rb.linearVelocity;
            float speed = v.magnitude;
            if (speed > maxBallSpeed)
                rb.linearVelocity = v * (maxBallSpeed / speed);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (ballTransform != null)
            ballStartPos = ballTransform.position;

        // Hide goal UI root at boot
        if (goalUIRoot != null)
            goalUIRoot.SetActive(false);
    }

    public void OnGoal()
    {
        // Show goal UI root (not just a child panel)
        if (goalUIRoot != null)
            goalUIRoot.SetActive(true);

        // Freeze ball
        var rb = ballTransform != null ? ballTransform.GetComponent<Rigidbody>() : null;
        if (rb) rb.isKinematic = true;
    }

    public void RestartGame()
    {
        // Hide goal UI root so it stops blocking ray interactions
        if (goalUIRoot != null)
            goalUIRoot.SetActive(false);

        // Reset ball physics
        var rb = ballTransform != null ? ballTransform.GetComponent<Rigidbody>() : null;
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset ball position
        if (ballTransform != null)
            ballTransform.position = ballStartPos;

        // Force new goal spawn
        if (goalSpawner != null)
            goalSpawner.ResetSpawner();

        // Reset obstacles
        if (obstacleSpawner != null)
            obstacleSpawner.ResetObstacles();

        // Hide ball until StartGameManager triggers BeginGame again
        if (ballTransform != null)
            ballTransform.gameObject.SetActive(false);

        // Show level selection again
        if (startGameManager != null)
            startGameManager.ShowStartUI();
    }
}
