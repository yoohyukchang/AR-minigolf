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

    // LEVEL BUTTONS should call this (pass 1 / 2 / 3)
    public void StartLevel(int level)
    {
        selectedLevel = Mathf.Clamp(level, 1, 3);
        StartGame();
    }

    // Optional: keep a Start button too
    public void OnStartButtonPressed()
    {
        StartGame();
    }

    // Called by GameManager when restarting back to menu
    public void ShowStartUI()
    {
        hasStarted = false;
        if (startUIRoot != null)
            startUIRoot.SetActive(true);
    }

    private void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Apply level tuning (impulse multiplier etc.)
        if (GameManager.Instance != null)
            GameManager.Instance.ApplyLevel(selectedLevel);

        // Hide menu
        if (startUIRoot != null)
            startUIRoot.SetActive(false);

        // Spawn counts by level
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

        // Begin gameplay
        if (ballController != null)
        {
            switch (selectedLevel)
            {
                case 2:
                    ballController.ballTransform.GetComponent<Renderer>().material.color = Color.yellow; // changed color
                    break;
                case 3:
                    ballController.ballTransform.GetComponent<Renderer>().material.color = Color.red;
                    break;
            }
            ballController.BeginGame();
        }
        if (goalSpawner != null) goalSpawner.BeginGame();
        if (obstacleSpawner != null) obstacleSpawner.BeginGame();

        Debug.Log($"[StartGameManager] Game started! Level={selectedLevel}");
    }
}
