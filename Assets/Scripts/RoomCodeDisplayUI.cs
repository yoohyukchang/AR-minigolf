using UnityEngine;
using TMPro;

/// <summary>
/// Displays the 6-digit room code in VR for observers to see and connect.
/// Shows the code prominently in the player's view.
/// </summary>
public class RoomCodeDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomCodeText;
    public TextMeshProUGUI instructionText;
    public GameObject roomCodePanel;

    [Header("Settings")]
    public bool autoShowOnGameStart = true;
    public float displayDuration = 10f; // Show for 10 seconds, then fade

    private bool _isShowing = false;
    private float _hideTime;

    private void Start()
    {
        // Subscribe to game events
        if (GameStateManager.Instance != null && autoShowOnGameStart)
        {
            GameStateManager.Instance.OnGameStarted += OnGameStarted;
        }

        // Hide initially
        if (roomCodePanel != null)
        {
            roomCodePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameStarted -= OnGameStarted;
        }
    }

    private void Update()
    {
        // Auto-hide after duration
        if (_isShowing && displayDuration > 0 && Time.time >= _hideTime)
        {
            HideRoomCode();
        }
    }

    private void OnGameStarted()
    {
        // When game starts, create room and show code
        if (PhotonNetworkManager.Instance != null)
        {
            PhotonNetworkManager.Instance.CreateRoomAsMainPlayer();

            // Wait a frame for room to be created, then show code
            StartCoroutine(ShowRoomCodeDelayed());
        }
    }

    private System.Collections.IEnumerator ShowRoomCodeDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (PhotonNetworkManager.Instance != null)
        {
            string roomCode = PhotonNetworkManager.Instance.CurrentRoomCode;
            ShowRoomCode(roomCode);
        }
    }

    public void ShowRoomCode(string code)
    {
        if (roomCodePanel != null)
        {
            roomCodePanel.SetActive(true);
        }

        if (roomCodeText != null)
        {
            // Format code for readability: ABC-DEF
            string formattedCode = code.Length == 6
                ? $"{code.Substring(0, 3)}-{code.Substring(3, 3)}"
                : code;

            roomCodeText.text = formattedCode;
        }

        if (instructionText != null)
        {
            instructionText.text = "Observers: Enter this code on your mobile device";
        }

        _isShowing = true;
        _hideTime = Time.time + displayDuration;

        Debug.Log($"[RoomCodeDisplay] Showing room code: {code}");
    }

    public void HideRoomCode()
    {
        if (roomCodePanel != null)
        {
            roomCodePanel.SetActive(false);
        }

        _isShowing = false;
        Debug.Log("[RoomCodeDisplay] Room code hidden");
    }

    // Toggle visibility (can be called from button)
    public void ToggleRoomCode()
    {
        if (_isShowing)
        {
            HideRoomCode();
        }
        else if (PhotonNetworkManager.Instance != null)
        {
            ShowRoomCode(PhotonNetworkManager.Instance.CurrentRoomCode);
        }
    }

    // Keep showing (disable auto-hide)
    public void KeepShowing()
    {
        displayDuration = -1f;
    }
}
