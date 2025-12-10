using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GoalSpawner : MonoBehaviour
{
    [Header("References")]
    public MRUK mruk;
    public Transform playerHead;       // CenterEyeAnchor
    public GameObject goalPrefab;      // HoleFlag prefab

    [Header("Placement Constraints")]
    [Tooltip("Minimum horizontal distance from the player to the goal (meters).")]
    public float minDistanceFromPlayer = 2.0f;

    [Tooltip("How far from floor edges/corners to keep the goal (meters).")]
    public float minDistanceToEdge = 0.3f;

    [Tooltip("Maximum number of attempts to find a valid spawn point.")]
    public int maxTries = 20;

    private bool _spawnRequested;
    private bool _spawned;
    private GameObject _spawnedGoal;

    void Start()
    {
        if (!mruk)
        {
            mruk = FindObjectOfType<MRUK>();
        }
    }

    /// <summary>
    /// Call this when the game starts (e.g., from StartGameManager).
    /// </summary>
    public void BeginGame()
    {
        _spawnRequested = true;
    }

    void Update()
    {
        if (!_spawnRequested || _spawned) return;
        if (mruk == null || !mruk.IsInitialized) return;

        TrySpawnGoal();
    }

    private void TrySpawnGoal()
    {
        var room = mruk.Rooms.FirstOrDefault();
        if (room == null)
        {
            Debug.LogWarning("[GoalSpawner] No MRUK room found.");
            return;
        }

        if (goalPrefab == null)
        {
            Debug.LogWarning("[GoalSpawner] No goal prefab assigned.");
            return;
        }

        var floorFilter = LabelFilter.Included(MRUKAnchor.SceneLabels.FLOOR);

        Vector2 playerXZ = new Vector2(playerHead.position.x, playerHead.position.z);

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 pos;
            Vector3 normal;

            bool found = room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minDistanceToEdge,
                floorFilter,
                out pos,
                out normal
            );

            if (!found)
            {
                // No valid point this iteration, try again.
                continue;
            }

            // Make sure we're not too close to the player horizontally
            Vector2 goalXZ = new Vector2(pos.x, pos.z);
            float dist = Vector2.Distance(playerXZ, goalXZ);
            if (dist < minDistanceFromPlayer)
            {
                // Too close; try another sample
                continue;
            }

            // Optional: avoid placing inside volumes (so you don't spawn inside couches, etc.)
            if (room.IsPositionInSceneVolume(pos, testVerticalBounds: true, distanceBuffer: 0.05f))
            {
                // Position overlaps a scene volume, skip
                continue;
            }

            // Align flag 'up' with surface normal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);

            Vector3 offsetPos = pos + normal * 0.28f;

            _spawnedGoal = Instantiate(goalPrefab, offsetPos, rotation);
            _spawned = true;

            Debug.Log($"[GoalSpawner] Spawned goal at {pos}, normal {normal}");
            return;
        }

        Debug.LogWarning("[GoalSpawner] Could not find a suitable goal position.");
    }
}
