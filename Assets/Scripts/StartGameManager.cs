using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject startUIRoot;

    [Header("Gameplay")]
    public BallController ballController;
    public GoalSpawner goalSpawner;
    public ObstacleSpawner obstacleSpawner;

    private bool hasStarted = false;

    [Header("Level Selection (1-3)")]
    [SerializeField] private int selectedLevel = 1;


    // Wire your Level1/2/3 buttons to this with parameter 1/2/3
    public void StartLevel(int level)
    {
        if (hasStarted) return;
        hasStarted = true;

        selectedLevel = Mathf.Clamp(level, 1, 3);

        // Hide the start menu UI
        if (startUIRoot != null)
            startUIRoot.SetActive(false);

        // Configure obstacle counts by level BEFORE spawning
        if (obstacleSpawner != null)
        {
            switch (selectedLevel)
            {
                case 1:
                    obstacleSpawner.grassCount = 0;
                    obstacleSpawner.manholeCount = 0;
                    break;

                case 2:
                    obstacleSpawner.grassCount = 1;
                    obstacleSpawner.manholeCount = 1;
                    break;

                case 3:
                    obstacleSpawner.grassCount = 3;
                    obstacleSpawner.manholeCount = 3;
                    break;
            }
        }

        // Start game flow
        if (ballController != null)
            ballController.BeginGame();

        if (goalSpawner != null)
            goalSpawner.BeginGame();

        if (obstacleSpawner != null)
            obstacleSpawner.BeginGame();

        Debug.Log($"[StartGameManager] Game started! Level={selectedLevel}");
    }
}
