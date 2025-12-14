using UnityEngine;
using UnityEngine.InputSystem;

public class ClubSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ClubEntry
    {
        public string name;
        public GameObject clubObject;
    }

    [Header("Clubs (order = switch order)")]
    public ClubEntry[] clubs;

    [Header("Input")]
    [Tooltip("Input action that triggers switching clubs (e.g. right-hand A button).")]
    public InputAction nextClubAction;

    private int _currentIndex = 0;

    // Event that fires when the active club changes
    public event System.Action OnClubChanged;

    private void OnEnable()
    {
        if (nextClubAction != null)
        {
            nextClubAction.performed += OnNextClub;
            nextClubAction.Enable();
        }

        SetActiveClub(_currentIndex);
    }

    private void OnDisable()
    {
        if (nextClubAction != null)
        {
            nextClubAction.performed -= OnNextClub;
            nextClubAction.Disable();
        }
    }

    private void OnNextClub(InputAction.CallbackContext ctx)
    {
        if (clubs == null || clubs.Length == 0) return;

        _currentIndex = (_currentIndex + 1) % clubs.Length;
        SetActiveClub(_currentIndex);
    }

    private void SetActiveClub(int index)
    {
        if (clubs == null || clubs.Length == 0) return;

        for (int i = 0; i < clubs.Length; i++)
        {
            if (clubs[i].clubObject != null)
            {
                clubs[i].clubObject.SetActive(i == index);
            }
        }

        if (index >= 0 && index < clubs.Length)
        {
            Debug.Log($"[ClubSwitcher] Active club: {clubs[index].name}");
        }

        // Notify listeners that the club changed
        OnClubChanged?.Invoke();
    }

    /// <summary>
    /// Gets the ClubStats of the currently active club.
    /// </summary>
    /// <returns>ClubStats of the active club, or null if not available.</returns>
    public ClubStats GetCurrentClubStats()
    {
        if (clubs == null || clubs.Length == 0 || _currentIndex < 0 || _currentIndex >= clubs.Length)
            return null;

        var clubObject = clubs[_currentIndex].clubObject;
        if (clubObject == null) return null;

        var hitBall = clubObject.GetComponent<GolfClubHitBall>();
        return hitBall != null ? hitBall.clubStats : null;
    }

    /// <summary>
    /// Gets the name of the currently active club.
    /// </summary>
    /// <returns>Name of the active club, or "Unknown" if not available.</returns>
    public string GetCurrentClubName()
    {
        if (clubs == null || clubs.Length == 0 || _currentIndex < 0 || _currentIndex >= clubs.Length)
            return "Unknown";

        return clubs[_currentIndex].name;
    }
}
