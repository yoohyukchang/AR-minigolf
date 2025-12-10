using UnityEngine;

/// <summary>
/// Controls observer camera with four viewing modes:
/// 1. POV Mirror - Mirrors main player's camera view
/// 2. Top-Down - Bird's eye view of the course
/// 3. Ball Tracking - Follows the golf ball
/// 4. Stats Overlay - Fixed camera with game statistics
/// </summary>
public class ObserverCameraController : MonoBehaviour
{
    public enum CameraMode
    {
        POVMirror,
        TopDown,
        BallTracking,
        StatsOverlay
    }

    [Header("References")]
    public Camera observerCamera;
    public Transform ballTransform;
    public NetworkedPlayerState playerState;
    public NetworkedRoomScan roomScan;

    [Header("Current Mode")]
    public CameraMode currentMode = CameraMode.POVMirror;

    [Header("POV Mirror Settings")]
    public float povLerpSpeed = 15f;

    [Header("Top-Down Settings")]
    public float topDownHeight = 10f;
    public float topDownAngle = 75f; // Degrees from horizontal

    [Header("Ball Tracking Settings")]
    public Vector3 ballTrackingOffset = new Vector3(0, 2f, -3f);
    public float ballTrackingSpeed = 8f;

    [Header("Stats Overlay Settings")]
    public Vector3 statsFixedPosition = new Vector3(0, 3f, -5f);
    public Vector3 statsLookAtOffset = new Vector3(0, 0, 0);

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _smoothVelocity;

    private void Start()
    {
        if (observerCamera == null)
        {
            observerCamera = GetComponent<Camera>();
        }

        // Subscribe to GameStateManager events
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnBallSpawned += OnBallSpawned;
        }

        // Try to find ball immediately if it already exists
        TryFindBall();

        // Try to find NetworkedPlayerState if not assigned
        TryFindPlayerState();

        // Try to find NetworkedRoomScan if not assigned
        TryFindRoomScan();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnBallSpawned -= OnBallSpawned;
        }
    }

    private void OnBallSpawned(Vector3 position, Quaternion rotation)
    {
        // Find ball reference when it spawns
        TryFindBall();
        Debug.Log($"[ObserverCamera] Ball spawned event received, ball found: {ballTransform != null}");
    }

    private void TryFindBall()
    {
        if (ballTransform == null)
        {
            GameObject ball = GameObject.FindGameObjectWithTag("GolfBall");
            if (ball != null)
            {
                ballTransform = ball.transform;
                Debug.Log($"[ObserverCamera] Ball reference found: {ball.name}");
            }
        }
    }

    private void TryFindPlayerState()
    {
        if (playerState == null)
        {
            playerState = FindFirstObjectByType<NetworkedPlayerState>();
            if (playerState != null)
            {
                Debug.Log($"[ObserverCamera] NetworkedPlayerState reference found");
            }
            else
            {
                Debug.LogWarning($"[ObserverCamera] NetworkedPlayerState not found in scene");
            }
        }
    }

    private void TryFindRoomScan()
    {
        if (roomScan == null)
        {
            roomScan = FindFirstObjectByType<NetworkedRoomScan>();
            if (roomScan != null)
            {
                Debug.Log($"[ObserverCamera] NetworkedRoomScan reference found");
            }
            else
            {
                Debug.LogWarning($"[ObserverCamera] NetworkedRoomScan not found in scene");
            }
        }
    }

    private Vector3 CalculateRoomCenter()
    {
        // Find all visualized room objects
        GameObject[] roomObjects = GameObject.FindGameObjectsWithTag("Untagged");
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (GameObject obj in roomObjects)
        {
            if (obj.name.StartsWith("Visualized_"))
            {
                sum += obj.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            return sum / count;
        }

        // Fallback: use ball position if room not found
        if (ballTransform != null)
        {
            return ballTransform.position;
        }

        return Vector3.zero;
    }

    private void LateUpdate()
    {
        if (observerCamera == null) return;

        // Periodically try to find ball if not yet found (for late-joining observers)
        if (ballTransform == null)
        {
            TryFindBall();
        }

        // Periodically try to find player state if not yet found
        if (playerState == null)
        {
            TryFindPlayerState();
        }

        // Periodically try to find room scan if not yet found
        if (roomScan == null)
        {
            TryFindRoomScan();
        }

        switch (currentMode)
        {
            case CameraMode.POVMirror:
                UpdatePOVMirror();
                break;
            case CameraMode.TopDown:
                UpdateTopDown();
                break;
            case CameraMode.BallTracking:
                UpdateBallTracking();
                break;
            case CameraMode.StatsOverlay:
                UpdateStatsOverlay();
                break;
        }

        // Smoothly interpolate to target
        observerCamera.transform.position = Vector3.SmoothDamp(
            observerCamera.transform.position,
            _targetPosition,
            ref _smoothVelocity,
            0.1f
        );
        observerCamera.transform.rotation = Quaternion.RotateTowards(
            observerCamera.transform.rotation,
            _targetRotation,
            povLerpSpeed * Time.deltaTime
        );
    }

    private void UpdatePOVMirror()
    {
        if (playerState != null)
        {
            _targetPosition = playerState.GetCameraPosition();
            _targetRotation = playerState.GetCameraRotation();
        }
    }

    private void UpdateTopDown()
    {
        // Calculate the center of the room scan
        Vector3 roomCenter = CalculateRoomCenter();

        // Position above the room center
        _targetPosition = roomCenter + Vector3.up * topDownHeight;

        // Look down at the room center
        Vector3 lookPoint = roomCenter;
        Vector3 direction = (lookPoint - _targetPosition).normalized;
        _targetRotation = Quaternion.LookRotation(direction);
    }

    private void UpdateBallTracking()
    {
        if (ballTransform == null) return;

        // Follow ball with offset
        Vector3 ballPos = ballTransform.position;
        _targetPosition = ballPos + ballTrackingOffset;

        // Always look at the ball
        Vector3 direction = (ballPos - _targetPosition).normalized;
        _targetRotation = Quaternion.LookRotation(direction);
    }

    private void UpdateStatsOverlay()
    {
        // Fixed position, looking at center of play area
        _targetPosition = statsFixedPosition;

        if (ballTransform != null)
        {
            Vector3 lookPoint = ballTransform.position + statsLookAtOffset;
            Vector3 direction = (lookPoint - _targetPosition).normalized;
            _targetRotation = Quaternion.LookRotation(direction);
        }
        else
        {
            _targetRotation = Quaternion.Euler(15f, 0f, 0f);
        }
    }

    // Switch camera mode (called from UI)
    public void SetCameraMode(int modeIndex)
    {
        currentMode = (CameraMode)modeIndex;
        Debug.Log($"[ObserverCamera] Switched to {currentMode} mode");
    }

    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;
        Debug.Log($"[ObserverCamera] Switched to {currentMode} mode");
    }

    // Cycle through modes
    public void CycleMode()
    {
        int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
        SetCameraMode(nextMode);
    }
}
