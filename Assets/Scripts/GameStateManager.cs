using System;
using UnityEngine;

/// <summary>
/// Centralized game state manager that broadcasts events to observers.
/// Follows functional core principle - holds state and notifies listeners.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameStarted { get; private set; }
    public int StrokeCount { get; private set; }
    public Vector3 LastBallPosition { get; private set; }
    public Vector3 LastBallVelocity { get; private set; }
    public Vector3 LastClubVelocity { get; private set; }
    public float LastSwingSpeed { get; private set; }

    // Events for observers to subscribe to
    public event Action OnGameStarted;
    public event Action<Vector3, Quaternion> OnBallSpawned;
    public event Action<Vector3, Vector3, Quaternion> OnBallHit; // position, velocity, rotation
    public event Action<Vector3, Vector3> OnBallMoved; // position, velocity
    public event Action<Vector3, float> OnClubSwing; // velocity, speed
    public event Action<int> OnStrokeCountChanged;
    public event Action<Vector3> OnGoalPositionSet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        IsGameStarted = true;
        StrokeCount = 0;
        Debug.Log("[GameStateManager] Game started");
        OnGameStarted?.Invoke();
    }

    public void NotifyBallSpawned(Vector3 position, Quaternion rotation)
    {
        LastBallPosition = position;
        Debug.Log($"[GameStateManager] Ball spawned at {position}");
        OnBallSpawned?.Invoke(position, rotation);
    }

    public void NotifyBallHit(Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 clubVelocity, float swingSpeed)
    {
        StrokeCount++;
        LastBallPosition = position;
        LastBallVelocity = velocity;
        LastClubVelocity = clubVelocity;
        LastSwingSpeed = swingSpeed;

        Debug.Log($"[GameStateManager] Ball hit! Stroke #{StrokeCount}, Speed: {swingSpeed:F2} m/s");

        OnBallHit?.Invoke(position, velocity, rotation);
        OnStrokeCountChanged?.Invoke(StrokeCount);
    }

    public void NotifyBallMoved(Vector3 position, Vector3 velocity)
    {
        LastBallPosition = position;
        LastBallVelocity = velocity;
        OnBallMoved?.Invoke(position, velocity);
    }

    public void NotifyClubSwing(Vector3 velocity, float speed)
    {
        LastClubVelocity = velocity;
        LastSwingSpeed = speed;
        OnClubSwing?.Invoke(velocity, speed);
    }

    public void SetGoalPosition(Vector3 position)
    {
        Debug.Log($"[GameStateManager] Goal position set to {position}");
        OnGoalPositionSet?.Invoke(position);
    }

    public void ResetGame()
    {
        IsGameStarted = false;
        StrokeCount = 0;
        LastBallPosition = Vector3.zero;
        LastBallVelocity = Vector3.zero;
        LastClubVelocity = Vector3.zero;
        LastSwingSpeed = 0f;
        Debug.Log("[GameStateManager] Game reset");
    }
}
