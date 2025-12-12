using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public MRUK mruk;
    public Transform playerHead;

    [Header("Prefabs")]
    public GameObject grassPrefab;
    public GameObject manholePrefab;

    [Header("Counts")]
    public int grassCount = 3;
    public int manholeCount = 1;

    [Header("Placement Constraints")]
    public float minDistanceFromPlayer = 0.5f;
    public float maxDistanceFromPlayer = 2.5f;
    public int maxTriesPerObstacle = 40;

    private bool _spawnRequested;
    private bool _spawned;
    private readonly List<GameObject> _spawnedObstacles = new List<GameObject>();

    void Start()
    {
        if (!mruk)
            mruk = FindObjectOfType<MRUK>();
    }

    public void BeginGame()
    {
        _spawnRequested = true;
    }

    void Update()
    {
        if (!_spawnRequested || _spawned) return;
        if (mruk == null || !mruk.IsInitialized) return;

        TrySpawnObstacles();
    }

    private void TrySpawnObstacles()
    {
        var room = mruk.Rooms.FirstOrDefault();
        if (room == null)
        {
            Debug.LogWarning("[ObstacleSpawner] No MRUK room found.");
            return;
        }

        Vector3 headPos = playerHead.position;

        // Spawn grass patches
        for (int i = 0; i < grassCount; i++)
        {
            TrySpawnSingle(room, headPos, grassPrefab);
        }

        // Spawn manholes
        for (int i = 0; i < manholeCount; i++)
        {
            TrySpawnSingle(room, headPos, manholePrefab);
        }

        _spawned = true;
        Debug.Log("[ObstacleSpawner] Spawned obstacles.");
    }

    private void TrySpawnSingle(MRUKRoom room, Vector3 headPos, GameObject prefab)
    {
        if (prefab == null) return;

        for (int t = 0; t < maxTriesPerObstacle; t++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            Vector3 samplePoint = headPos + new Vector3(
                Mathf.Cos(angle) * radius,
                0.5f,
                Mathf.Sin(angle) * radius
            );

            if (Physics.Raycast(samplePoint, Vector3.down, out RaycastHit hit, 2f))
            {
                // Must be reasonably flat & inside the tracked room
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.9f) continue;
                if (!room.IsPositionInSceneVolume(hit.point, true, 0.05f)) continue;

                Vector3 spawnPos = hit.point;
                Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                GameObject obj = Instantiate(prefab, spawnPos, spawnRot);
                _spawnedObstacles.Add(obj);
                return;
            }
        }

        Debug.LogWarning("[ObstacleSpawner] Failed to place obstacle after many tries.");
    }

    public void ResetObstacles()
    {
        _spawned = false;
        _spawnRequested = false;

        foreach (var obj in _spawnedObstacles)
        {
            if (obj != null) Destroy(obj);
        }
        _spawnedObstacles.Clear();
    }
}
