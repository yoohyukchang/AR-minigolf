using UnityEngine;

/// <summary>
/// Helper script to automatically setup the observer system at runtime.
/// Attach to an empty GameObject in your scene for quick setup.
/// </summary>
public class ObserverSystemSetup : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [Tooltip("Automatically find and add BallStateTracker to golf ball")]
    public bool autoSetupBall = true;

    [Tooltip("Automatically create goal post at specified position")]
    public bool autoCreateGoalPost = true;

    [Tooltip("Position for the goal post (relative to world origin)")]
    public Vector3 goalPostPosition = new Vector3(0, 0, 5);

    [Header("References (Optional - will auto-find if not set)")]
    public GameObject golfBall;
    public Transform playerHead;

    private void Start()
    {
        Debug.Log("[ObserverSystemSetup] Starting automatic setup...");

        // Find references if not set
        if (golfBall == null)
        {
            golfBall = GameObject.FindGameObjectWithTag("GolfBall");
        }

        if (playerHead == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                playerHead = mainCam.transform;
            }
        }

        // Setup ball tracking
        if (autoSetupBall && golfBall != null)
        {
            SetupBallTracking();
        }

        // Create goal post
        if (autoCreateGoalPost)
        {
            CreateGoalPost();
        }

        Debug.Log("[ObserverSystemSetup] Setup complete!");
    }

    private void SetupBallTracking()
    {
        // Add BallStateTracker if not present
        if (golfBall.GetComponent<BallStateTracker>() == null)
        {
            golfBall.AddComponent<BallStateTracker>();
            Debug.Log("[ObserverSystemSetup] Added BallStateTracker to golf ball");
        }

        // Ensure ball has Rigidbody
        if (golfBall.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = golfBall.AddComponent<Rigidbody>();
            rb.mass = 0.0459f; // Standard golf ball mass in kg
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.2f;
            Debug.Log("[ObserverSystemSetup] Added Rigidbody to golf ball");
        }

        // Ensure ball has correct tag
        if (!golfBall.CompareTag("GolfBall"))
        {
            Debug.LogWarning("[ObserverSystemSetup] Golf ball doesn't have 'GolfBall' tag! Please add it manually.");
        }
    }

    private void CreateGoalPost()
    {
        // Check if goal post already exists
        GameObject existingGoal = GameObject.FindGameObjectWithTag("GoalPost");
        if (existingGoal != null)
        {
            Debug.Log("[ObserverSystemSetup] Goal post already exists, skipping creation");
            return;
        }

        // Create goal post using helper method
        GameObject goalPost = GoalPost.CreateSimpleGoalPost(goalPostPosition);
        Debug.Log($"[ObserverSystemSetup] Created goal post at {goalPostPosition}");

        // Try to add tag if it doesn't exist
        try
        {
            goalPost.tag = "GoalPost";
        }
        catch
        {
            Debug.LogWarning("[ObserverSystemSetup] 'GoalPost' tag doesn't exist. Please add it in Tags & Layers.");
        }
    }

    // Called from Unity Editor or button
    public void ManualSetup()
    {
        Debug.Log("[ObserverSystemSetup] Running manual setup...");
        Start();
    }

    private void OnDrawGizmos()
    {
        // Visualize where goal post will be created
        if (autoCreateGoalPost)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalPostPosition, 0.2f);
            Gizmos.DrawLine(goalPostPosition + Vector3.up * 0.5f, goalPostPosition - Vector3.up * 0.5f);
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor to add a setup button in the Inspector
/// </summary>
[UnityEditor.CustomEditor(typeof(ObserverSystemSetup))]
public class ObserverSystemSetupEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObserverSystemSetup setup = (ObserverSystemSetup)target;

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.HelpBox(
            "This script will automatically setup the observer system when the scene starts.\n\n" +
            "To manually trigger setup, click the button below:",
            UnityEditor.MessageType.Info
        );

        if (GUILayout.Button("Run Setup Now", GUILayout.Height(30)))
        {
            setup.ManualSetup();
        }

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.HelpBox(
            "Next Steps:\n" +
            "1. Install Photon PUN 2 from Asset Store\n" +
            "2. Add PhotonView components to ball and player\n" +
            "3. Test in play mode",
            UnityEditor.MessageType.Warning
        );
    }
}
#endif
