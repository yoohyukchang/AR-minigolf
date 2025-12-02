using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TMP_InputField roomCodeInput;
    public Button connectButton;

    [Header("Camera Control Buttons")]
    public Button povButton;
    public Button topDownButton;
    public Button ballTrackingButton;
    public Button statsOverlayButton;

    [Header("References")]
    public ObserverCameraController cameraController;

    private Vector3 _goalPosition;
    private bool _isConnected = false;

    private void Start()
    {
        // Subscribe to GameStateManager events
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStrokeCountChanged += UpdateStrokeCount;
            GameStateManager.Instance.OnBallMoved += UpdateBallInfo;
            GameStateManager.Instance.OnClubSwing += UpdateSwingInfo;
            GameStateManager.Instance.OnGoalPositionSet += UpdateGoalPosition;
        }

        // Setup button listeners
        SetupButtons();

        // Show connection panel initially
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(true);
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

    private void SetupButtons()
    {
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectButtonClicked);
        }

        if (povButton != null)
        {
            povButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.POVMirror));
        }

        if (topDownButton != null)
        {
            topDownButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.TopDown));
        }

        if (ballTrackingButton != null)
        {
            ballTrackingButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.BallTracking));
        }

        if (statsOverlayButton != null)
        {
            statsOverlayButton.onClick.AddListener(() => SwitchCameraMode(ObserverCameraController.CameraMode.StatsOverlay));
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
        Debug.Log($"[ObserverUI] Connecting to room: {roomCode}");

        // Connect via PhotonNetworkManager
        if (PhotonNetworkManager.Instance != null)
        {
            PhotonNetworkManager.Instance.JoinRoomAsObserver(roomCode);
        }

        _isConnected = true;
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(false);
        }

        if (roomCodeText != null)
        {
            roomCodeText.text = $"Room: {roomCode}";
        }
    }

    private void SwitchCameraMode(ObserverCameraController.CameraMode mode)
    {
        if (cameraController != null)
        {
            cameraController.SetCameraMode(mode);
        }

        if (cameraModeLabelText != null)
        {
            cameraModeLabelText.text = $"View: {mode}";
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
        _isConnected = true;

        if (connectionPanel != null)
        {
            connectionPanel.SetActive(false);
        }

        if (roomCodeText != null)
        {
            roomCodeText.text = $"Room: {roomCode}";
        }

        if (cameraModeLabelText != null)
        {
            cameraModeLabelText.text = "View: POV Mirror";
        }
    }
}
