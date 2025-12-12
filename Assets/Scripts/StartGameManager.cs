using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject startUIRoot;

    [Header("Gameplay")]
    public BallController ballController;
    public GoalSpawner goalSpawner;
    private bool hasStarted = false;
    public ObstacleSpawner obstacleSpawner;

    // Called by the Start button
    public void OnStartButtonPressed()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Hide the start menu UI
        if (startUIRoot != null)
        {
            startUIRoot.SetActive(false);
        }

        // Tell ballController to spawn & drop the ball when MRUK is ready
        if (ballController != null)
        {
            ballController.BeginGame();
        }

        if (goalSpawner != null)
        {
            goalSpawner.BeginGame();
        }

        if (obstacleSpawner != null) {
            obstacleSpawner.BeginGame();
        }


        Debug.Log("[StartGameManager] Game started!");
    }
}
