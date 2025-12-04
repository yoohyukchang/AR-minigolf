using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Controls the observer mobile UI showing game stats and camera controls.
/// Displays stroke count, ball velocity, swing speed, and camera mode switcher.
/// </summary>
public class ObserverUIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI strokeCountText;
    public TextMeshProUGUI ballVelocityText;
    public TextMeshProUGUI swingSpeedText;
    public TextMeshProUGUI distanceToGoalText;
    public TextMeshProUGUI cameraModeLabelText;
    public TextMeshProUGUI roomCodeText;
    public GameObject connectionPanel;
    public GameObject statsPanel;
    public GameObject cameraControlsPanel;
    public TMP_InputField roomCodeInput;
    public Button connectButton;

    [Header("Camera Control Buttons")]
    public Button povButton;
    public Button topDownButton;
    public Button ballTrackingButton;
    public Button statsOverlayButton;

    [Header("References")]
    public ObserverCameraController cameraController;

    [Header("Debug")]
    [Tooltip("Force show UI panels on start (for testing without connection)")]
    public bool debugShowUIOnStart = false;

    private Vector3 _goalPosition;
    private bool _isConnected = false;
    private float _connectionCheckTimer = 0f;

    private void Start()
    {
        Debug.Log("[ObserverUI] Starting ObserverUIController...");

        // Try to find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<ObserverCameraController>();
            if (cameraController != null)
            {
                Debug.Log("[ObserverUI] Found ObserverCameraController automatically");
            }
            else
            {
                Debug.LogWarning("[ObserverUI] ObserverCameraController not found! Perspective switching won't work.");
            }
        }

        // Subscribe to GameStateManager events
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStrokeCountChanged += UpdateStrokeCount;
            GameStateManager.Instance.OnBallMoved += UpdateBallInfo;
            GameStateManager.Instance.OnClubSwing += UpdateSwingInfo;
            GameStateManager.Instance.OnGoalPositionSet += UpdateGoalPosition;
            Debug.Log("[ObserverUI] Subscribed to GameStateManager events");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] GameStateManager not found! Stats won't update.");
        }

        // Setup button listeners
        SetupButtons();

        // Show connection panel initially, hide stats and controls
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(true);
            Debug.Log("[ObserverUI] Connection panel shown");
        }
        else
        {
            Debug.LogError("[ObserverUI] Connection panel reference is missing!");
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[ObserverUI] Stats panel reference is missing!");
        }

        if (cameraControlsPanel != null)
        {
            cameraControlsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[ObserverUI] Camera controls panel reference is missing!");
        }

        // Debug mode - show UI immediately
        if (debugShowUIOnStart)
        {
            Debug.Log("[ObserverUI] Debug mode: Showing UI panels immediately");
            ShowGameUI("DEBUG");
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStrokeCountChanged -= UpdateStrokeCount;
            GameStateManager.Instance.OnBallMoved -= UpdateBallInfo;
            GameStateManager.Instance.OnClubSwing -= UpdateSwingInfo;
            GameStateManager.Instance.OnGoalPositionSet -= UpdateGoalPosition;
        }
    }

    private void Update()
    {
        // Auto-check for connection state and show UI if needed
        if (!_isConnected && PhotonNetwork.InRoom)
        {
            _connectionCheckTimer += Time.deltaTime;
            if (_connectionCheckTimer > 2f) // Wait 2 seconds after joining room
            {
                Debug.Log("[ObserverUI] Connected to room but UI not shown, triggering UI update");
                OnConnectedToRoom(PhotonNetwork.CurrentRoom?.Name ?? "UNKNOWN");
                _connectionCheckTimer = 0f;
            }
        }
    }

    private void SetupButtons()
    {
        Debug.Log("[ObserverUI] Setting up button listeners...");

        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectButtonClicked);
            Debug.Log("[ObserverUI] Connect button listener added");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Connect button reference is missing!");
        }

        if (povButton != null)
        {
            povButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.POVMirror));
            Debug.Log("[ObserverUI] POV button listener added");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] POV button reference is missing!");
        }

        if (topDownButton != null)
        {
            topDownButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.TopDown));
            Debug.Log("[ObserverUI] Top-Down button listener added");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Top-Down button reference is missing!");
        }

        if (ballTrackingButton != null)
        {
            ballTrackingButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.BallTracking));
            Debug.Log("[ObserverUI] Ball Tracking button listener added");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Ball Tracking button reference is missing!");
        }

        if (statsOverlayButton != null)
        {
            statsOverlayButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.StatsOverlay));
            Debug.Log("[ObserverUI] Stats Overlay button listener added");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Stats Overlay button reference is missing!");
        }
    }

    private void OnConnectButtonClicked()
    {
        if (roomCodeInput == null || string.IsNullOrEmpty(roomCodeInput.text))
        {
            Debug.LogWarning("[ObserverUI] Please enter a room code");
            return;
        }

        string roomCode = roomCodeInput.text.ToUpper();
        Debug.Log($"[ObserverUI] Attempting to connect to room: {roomCode}");

        // Connect via PhotonNetworkManager
        if (PhotonNetworkManager.Instance != null)
        {
            PhotonNetworkManager.Instance.JoinRoomAsObserver(roomCode);
            // Don't set _isConnected here - wait for OnConnectedToRoom callback
        }
        else
        {
            Debug.LogError("[ObserverUI] PhotonNetworkManager instance not found!");
        }
    }

    private void SwitchCameraMode(ObserverCameraController.CameraMode mode)
    {
        Debug.Log($"[ObserverUI] Switching camera mode to: {mode}");

        if (cameraController != null)
        {
            cameraController.SetCameraMode(mode);
            Debug.Log($"[ObserverUI] Camera mode switched successfully to: {mode}");
        }
        else
        {
            Debug.LogError("[ObserverUI] Cannot switch camera mode - cameraController is null!");
            // Try to find it again
            cameraController = FindFirstObjectByType<ObserverCameraController>();
            if (cameraController != null)
            {
                Debug.Log("[ObserverUI] Found camera controller, retrying mode switch");
                cameraController.SetCameraMode(mode);
            }
        }

        if (cameraModeLabelText != null)
        {
            cameraModeLabelText.text = $"View: {mode}";
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Camera mode label text is null!");
        }
    }

    private void UpdateStrokeCount(int count)
    {
        if (strokeCountText != null)
        {
            strokeCountText.text = $"Strokes: {count}";
        }
    }

    private void UpdateBallInfo(Vector3 position, Vector3 velocity)
    {
        if (ballVelocityText != null)
        {
            float speed = velocity.magnitude;
            ballVelocityText.text = $"Ball Speed: {speed:F2} m/s";
        }

        // Update distance to goal if goal is set
        if (distanceToGoalText != null && _goalPosition != Vector3.zero)
        {
            float distance = Vector3.Distance(position, _goalPosition);
            distanceToGoalText.text = $"Distance: {distance:F2} m";
        }
    }

    private void UpdateSwingInfo(Vector3 velocity, float speed)
    {
        if (swingSpeedText != null)
        {
            swingSpeedText.text = $"Swing Speed: {speed:F2} m/s";
        }
    }

    private void UpdateGoalPosition(Vector3 position)
    {
        _goalPosition = position;
    }

    // Called externally when successfully connected
    public void OnConnectedToRoom(string roomCode)
    {
        Debug.Log($"[ObserverUI] OnConnectedToRoom called with room code: {roomCode}");
        ShowGameUI(roomCode);
    }

    // Public method to manually show the game UI (for testing or manual triggering)
    public void ShowGameUI(string roomCode = "")
    {
        Debug.Log($"[ObserverUI] ShowGameUI called, showing stats and controls");

        _isConnected = true;
        _connectionCheckTimer = 0f;

        // Hide connection panel, show stats and camera controls
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(false);
            Debug.Log("[ObserverUI] Connection panel hidden");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Cannot hide connection panel - reference is null");
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            Debug.Log("[ObserverUI] Stats panel shown");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Cannot show stats panel - reference is null");
        }

        if (cameraControlsPanel != null)
        {
            cameraControlsPanel.SetActive(true);
            Debug.Log("[ObserverUI] Camera controls panel shown");
        }
        else
        {
            Debug.LogWarning("[ObserverUI] Cannot show camera controls panel - reference is null");
        }

        if (roomCodeText != null && !string.IsNullOrEmpty(roomCode))
        {
            roomCodeText.text = $"Room: {roomCode}";
        }

        if (cameraModeLabelText != null)
        {
            cameraModeLabelText.text = "View: POV Mirror";
        }

        Debug.Log("[ObserverUI] UI panels updated successfully - stats and controls should now be visible");
    }

    // Public method to hide game UI and show connection panel
    public void ShowConnectionUI()
    {
        Debug.Log("[ObserverUI] Showing connection UI");

        _isConnected = false;

        if (connectionPanel != null)
        {
            connectionPanel.SetActive(true);
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }

        if (cameraControlsPanel != null)
        {
            cameraControlsPanel.SetActive(false);
        }
    }
}
