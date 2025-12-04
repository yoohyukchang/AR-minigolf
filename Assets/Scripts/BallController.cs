using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Photon.Pun;

public class BallController : MonoBehaviour
{
    public MRUK mruk;
    public Transform playerHead;

    [Header("Network Settings")]
    [Tooltip("Name of the ball prefab in Resources folder")]
    public string ballPrefabName = "NetworkedBall";

    private Transform _ballTransform;
    private bool _placed;
    private bool _startRequested;

    void Start()
    {
        if (!mruk)
        {
            mruk = FindObjectOfType<MRUK>();
        }
    }

    // Called by the Start button (via StartGameManager)
    public void BeginGame()
    {
        _startRequested = true;
        _placed = false;

        // Only the main player spawns the ball
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[BallController] Main player - will spawn ball when MRUK is ready");
        }
        else
        {
            Debug.Log("[BallController] Observer - waiting for ball to be spawned by main player");
        }
    }

    void Update()
    {
        // Only the main player handles ball spawning
        if (!PhotonNetwork.IsMasterClient) return;

        // Do nothing until:
        //  - Start has been requested
        //  - MRUK is initialized
        //  - We haven't already placed the ball
        if (!_startRequested || _placed || mruk == null) return;
        if (!mruk.IsInitialized) return;

        SpawnAndPlaceBall();
        _placed = true;
    }

    void SpawnAndPlaceBall()
    {
        var room = mruk.Rooms.FirstOrDefault();
        if (room == null)
        {
            Debug.LogWarning("[BallController] MRUK: No room found.");
            return;
        }

        var floor = room.GetFloorAnchor();
        if (floor == null)
        {
            Debug.LogWarning("[BallController] MRUK: No floor anchor found.");
            return;
        }

        float floorY = floor.transform.position.y;

        Vector3 headPos = playerHead.position;
        Vector3 forwardFlat = Vector3.ProjectOnPlane(playerHead.forward, Vector3.up).normalized;
        if (forwardFlat.sqrMagnitude < 1e-4f)
        {
            forwardFlat = Vector3.forward;
        }

        // Calculate spawn position: 0.5m in front of user and 1m above the floor
        Vector3 userOnFloor = new Vector3(headPos.x, floorY, headPos.z);
        Vector3 ballPos = userOnFloor + forwardFlat * 0.5f + Vector3.up * 1.0f;

        // Spawn ball over network - Photon will instantiate it for all clients
        GameObject ballObj = PhotonNetwork.Instantiate(ballPrefabName, ballPos, Quaternion.identity);
        if (ballObj != null)
        {
            _ballTransform = ballObj.transform;

            var rb = ballObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[BallController] Ball spawned over network at {ballPos}");

            // Notify GameStateManager that ball has been spawned
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.NotifyBallSpawned(ballPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError($"[BallController] Failed to spawn ball prefab '{ballPrefabName}'. Make sure it exists in Resources folder.");
        }
    }

    /// <summary>
    /// Get the current ball transform (useful for other scripts)
    /// </summary>
    public Transform GetBallTransform()
    {
        if (_ballTransform == null)
        {
            // Try to find it if not yet assigned
            GameObject ball = GameObject.FindGameObjectWithTag("GolfBall");
            if (ball != null)
            {
                _ballTransform = ball.transform;
            }
        }
        return _ballTransform;
    }
}
