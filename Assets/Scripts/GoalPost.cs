using UnityEngine;

/// <summary>
/// Goal post that detects when the golf ball enters.
/// Handles win condition and network synchronization.
/// </summary>
public class GoalPost : MonoBehaviour
{
    [Header("Settings")]
    public string ballTag = "GolfBall";
    public float goalRadius = 0.2f;

    [Header("Visual Feedback")]
    public GameObject visualIndicator;
    public Color goalColor = Color.green;
    public Color defaultColor = Color.white;

    private bool _ballInGoal = false;
    private Renderer _renderer;

    private void Start()
    {
        // Notify GameStateManager of goal position
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetGoalPosition(transform.position);
        }

        // Setup visual indicator
        if (visualIndicator != null)
        {
            _renderer = visualIndicator.GetComponent<Renderer>();
            if (_renderer != null)
            {
                _renderer.material.color = defaultColor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ballTag) && !_ballInGoal)
        {
            _ballInGoal = true;
            OnGoalReached();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            _ballInGoal = false;
            if (_renderer != null)
            {
                _renderer.material.color = defaultColor;
            }
        }
    }

    private void OnGoalReached()
    {
        Debug.Log($"[GoalPost] Ball entered goal! Strokes: {GameStateManager.Instance?.StrokeCount ?? 0}");

        // Visual feedback
        if (_renderer != null)
        {
            _renderer.material.color = goalColor;
        }

        // TODO: Trigger win condition, show score, etc.
        // This can be expanded to show a win screen or reset the game
    }

    private void OnDrawGizmosSelected()
    {
        // Draw goal radius in editor
        Gizmos.color = goalColor;
        Gizmos.DrawWireSphere(transform.position, goalRadius);
    }

    // Helper method to create a simple goal post cylinder
    public static GameObject CreateSimpleGoalPost(Vector3 position)
    {
        GameObject goalPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalPost.name = "GoalPost";
        goalPost.transform.position = position;
        goalPost.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f); // Flat cylinder
        goalPost.tag = "GoalPost";

        // Make it a trigger
        Collider collider = goalPost.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Set green material
        Renderer renderer = goalPost.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        }

        // Add GoalPost component
        goalPost.AddComponent<GoalPost>();

        return goalPost;
    }
}
