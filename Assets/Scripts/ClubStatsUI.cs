using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages the club statistics overlay UI that displays on the controller hand.
/// Toggles visibility with the B button and updates when clubs are switched.
/// </summary>
public class ClubStatsUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the ClubSwitcher to get current club stats")]
    public ClubSwitcher clubSwitcher;

    [Header("UI Elements")]
    [Tooltip("The root GameObject to show/hide (e.g., StatsPanel)")]
    public GameObject uiRoot;
    public TextMeshProUGUI clubNameText;
    public TextMeshProUGUI launchAngleText;
    public TextMeshProUGUI minSpeedText;
    public TextMeshProUGUI baseImpulseText;
    public TextMeshProUGUI speedMultiplierText;
    public TextMeshProUGUI maxImpulseText;

    [Header("Input")]
    [Tooltip("Input action for toggling the stats display (B button)")]
    public InputAction toggleStatsAction;

    [Header("Display Settings")]
    [Tooltip("Should the UI start visible?")]
    public bool startVisible = false;

    private bool _isVisible;
    private ClubStats _lastDisplayedStats;

    private void OnEnable()
    {
        // Subscribe to B button input
        if (toggleStatsAction != null)
        {
            toggleStatsAction.performed += OnToggleStats;
            toggleStatsAction.Enable();
        }

        // Subscribe to club change events
        if (clubSwitcher != null)
        {
            clubSwitcher.OnClubChanged += OnClubChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from B button input
        if (toggleStatsAction != null)
        {
            toggleStatsAction.performed -= OnToggleStats;
            toggleStatsAction.Disable();
        }

        // Unsubscribe from club change events
        if (clubSwitcher != null)
        {
            clubSwitcher.OnClubChanged -= OnClubChanged;
        }
    }

    private void Start()
    {
        // Initialize visibility state
        _isVisible = startVisible;
        if (uiRoot != null)
        {
            uiRoot.SetActive(_isVisible);
        }

        // Display initial stats if visible
        if (_isVisible)
        {
            UpdateStatsDisplay();
        }
    }

    /// <summary>
    /// Called when the B button is pressed to toggle the stats display.
    /// </summary>
    private void OnToggleStats(InputAction.CallbackContext ctx)
    {
        _isVisible = !_isVisible;

        if (uiRoot != null)
        {
            uiRoot.SetActive(_isVisible);
        }

        // Update display when toggling on
        if (_isVisible)
        {
            UpdateStatsDisplay();
        }

        Debug.Log($"[ClubStatsUI] Stats display toggled: {(_isVisible ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Called when the active club changes via ClubSwitcher event.
    /// </summary>
    private void OnClubChanged()
    {
        // Only update if visible
        if (_isVisible)
        {
            UpdateStatsDisplay();
        }
    }

    /// <summary>
    /// Updates all stat text displays with data from the current club.
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (clubSwitcher == null)
        {
            Debug.LogWarning("[ClubStatsUI] No ClubSwitcher reference assigned.");
            return;
        }

        ClubStats stats = clubSwitcher.GetCurrentClubStats();

        if (stats == null)
        {
            Debug.LogWarning("[ClubStatsUI] Could not get current club stats.");
            return;
        }

        _lastDisplayedStats = stats;

        // Update club name with both display name and club entry name
        if (clubNameText != null)
        {
            string clubName = clubSwitcher.GetCurrentClubName();
            clubNameText.text = $"<b>{stats.displayName}</b> ({clubName})";
        }

        // Update all stat fields with proper formatting
        if (launchAngleText != null)
            launchAngleText.text = $"Launch Angle: {stats.launchAngleDeg:F1}°";

        if (minSpeedText != null)
            minSpeedText.text = $"Min Speed: {stats.minEffectiveSpeed:F2} m/s";

        if (baseImpulseText != null)
            baseImpulseText.text = $"Base Impulse: {stats.baseImpulse:F2}";

        if (speedMultiplierText != null)
            speedMultiplierText.text = $"Speed Multiplier: {stats.swingSpeedMultiplier:F2}";

        if (maxImpulseText != null)
            maxImpulseText.text = $"Max Impulse: {stats.maxImpulse:F2}";

        Debug.Log($"[ClubStatsUI] Updated display for: {stats.displayName}");
    }

    /// <summary>
    /// Polling fallback to detect club changes even without event.
    /// Updates display if the club stats reference has changed.
    /// </summary>
    private void Update()
    {
        // Only poll if visible
        if (!_isVisible) return;

        if (clubSwitcher != null)
        {
            ClubStats currentStats = clubSwitcher.GetCurrentClubStats();

            // Only update if stats reference changed
            if (currentStats != _lastDisplayedStats)
            {
                UpdateStatsDisplay();
            }
        }
    }
}
