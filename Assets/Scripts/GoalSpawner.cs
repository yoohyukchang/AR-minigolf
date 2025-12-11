using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GoalSpawner : MonoBehaviour
{
    [Header("References")]
    public MRUK mruk;
    public Transform playerHead;
    public GameObject goalPrefab;

    [Header("Placement Constraints")]
    public float minDistanceFromPlayer = 0.1f;
    public float maxDistanceFromPlayer = 1.5f;
    public int maxTries = 200;

    private bool _spawnRequested;
    private bool _spawned;
    private GameObject _spawnedGoal;

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
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            Vector3 samplePoint = headPos + new Vector3(
                Mathf.Cos(angle) * radius,
                0.5f,
                Mathf.Sin(angle) * radius
            );

            // Raycast downward into real-world floor
            if (Physics.Raycast(samplePoint, Vector3.down, out RaycastHit hit, 2f))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.8f)
                    continue;

                if (room.IsPositionInSceneVolume(hit.point, true, 0.05f))
                    continue;

                Vector3 spawnPos = hit.point + hit.normal * 0.28f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                _spawnedGoal = Instantiate(goalPrefab, spawnPos, rotation);
                _spawned = true;

                Debug.Log("[GoalSpawner] Spawned goal at: " + spawnPos);
                return;
            }
        }

        Debug.LogWarning("[GoalSpawner] Could not find a suitable goal position.");
    }

    public void ResetSpawner()
    {
        _spawned = false;
        _spawnRequested = true;

        if (_spawnedGoal != null)
            Destroy(_spawnedGoal);
    }
}
