using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject startUIRoot;

    [Header("Gameplay")]
    public BallController ballController;
    public GoalSpawner goalSpawner;
    public ObstacleSpawner obstacleSpawner;

    [Header("Level Select (1-3)")]
    [Range(1, 3)]
    public int selectedLevel = 1;

    private bool hasStarted = false;

    // Level buttons should call THIS (1/2/3)
    public void StartLevel(int level)
    {
        selectedLevel = Mathf.Clamp(level, 1, 3);
        StartGame(); // immediately start
    }

    private void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Apply level settings (impulse multiplier)
        if (GameManager.Instance != null)
            GameManager.Instance.ApplyLevel(selectedLevel); // uses your tuning array :contentReference[oaicite:1]{index=1}

        // Hide menu
        if (startUIRoot != null)
            startUIRoot.SetActive(false);

        // Begin gameplay
        if (ballController != null) ballController.BeginGame();
        if (goalSpawner != null) goalSpawner.BeginGame();
        if (obstacleSpawner != null) obstacleSpawner.BeginGame();

        Debug.Log($"[StartGameManager] Game started! Level={selectedLevel}");
    }
}
