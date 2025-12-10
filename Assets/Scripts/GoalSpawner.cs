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
    public float minDistanceFromPlayer = 0.1f;

    [Tooltip("How far from floor edges/corners to keep the goal (meters).")]
    public float minDistanceToEdge = 0.3f;

    [Tooltip("Maximum horizontal distance from player for goal spawn.")]
    public float maxDistanceFromPlayer = 1.5f;

    [Tooltip("Maximum number of attempts to find a valid spawn point.")]
    public int maxTries = 200;

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

    Vector3 headPos = playerHead.position;

    for (int i = 0; i < maxTries; i++)
    {
        // Pick a random direction + distance around player
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        Vector3 samplePoint = headPos + new Vector3(
            Mathf.Cos(angle) * radius,
            0.5f,
            Mathf.Sin(angle) * radius
        );

        // Raycast DOWNWARD into the real-world (EffectMesh colliders)
        if (Physics.Raycast(samplePoint, Vector3.down, out RaycastHit hit, 2f))
        {
            // Must be floor-like:
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.8f)
                continue;

            // Must not be inside trash volumes
            if (room.IsPositionInSceneVolume(hit.point, true, 0.05f))
                continue;

            // Position + rotation
            Vector3 spawnPos = hit.point + hit.normal * 0.28f;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);

            Instantiate(goalPrefab, spawnPos, rotation);
            _spawned = true;
            Debug.Log("[GoalSpawner] Spawned goal at " + spawnPos);
            return;
        }
    }

    Debug.LogWarning("[GoalSpawner] Could not find a suitable goal position.");
}

}
