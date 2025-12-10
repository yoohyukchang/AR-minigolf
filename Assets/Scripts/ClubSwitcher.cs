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
    }
}
