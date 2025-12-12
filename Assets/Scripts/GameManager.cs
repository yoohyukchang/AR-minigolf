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
        ballStartPos = ballTransform.position;
        goalScreen.SetActive(false);
    }

    public void OnGoal()
    {
        goalScreen.SetActive(true);

        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void RestartGame()
    {
        goalScreen.SetActive(false);

        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ballTransform.position = ballStartPos;

        goalSpawner.ResetSpawner();
    }
}
