using UnityEngine;
using Photon.Pun;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Synchronizes MRUK room scan data from the VR player to observers.
/// Captures room geometry (floor, walls, ceiling) and sends it over the network
/// so observers can see the physical play space.
/// </summary>
public class NetworkedRoomScan : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public MRUK mruk;

    [Header("Visualization Settings")]
    [Tooltip("Material to use for visualizing room surfaces on observer side")]
    public Material roomVisualizationMaterial;

    [Tooltip("Show floor plane on observer side")]
    public bool showFloor = true;

    [Tooltip("Show walls on observer side")]
    public bool showWalls = true;

    [Tooltip("Show ceiling on observer side")]
    public bool showCeiling = false;

    [Header("Update Settings")]
    [Tooltip("How often to check for room updates (in seconds). Set to 0 to only send once.")]
    public float updateInterval = 0f;

    private float _lastUpdateTime;
    private bool _roomDataSent = false;
    private Dictionary<string, GameObject> _visualizedObjects = new Dictionary<string, GameObject>();

    // Cache room data for late joiners
    private MRUKRoom _cachedRoom;

    private void Start()
    {
        if (mruk == null)
        {
            mruk = FindFirstObjectByType<MRUK>();
        }

        // Subscribe to MRUK room events
        if (mruk != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnRoomLoaded);
        }
    }

    private void OnDestroy()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.DeregisterSceneLoadedCallback(OnRoomLoaded);
        }
    }

    // Called when a new player (observer) joins the room
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Only the main player sends room data
        if (PhotonNetwork.IsMasterClient && _cachedRoom != null)
        {
            Debug.Log($"[NetworkedRoomScan] New observer joined, sending room data to player {newPlayer.ActorNumber}");
            SendRoomDataToSpecificPlayer(newPlayer);
        }
    }

    private void OnRoomLoaded()
    {
        Debug.Log("[NetworkedRoomScan] Room loaded event received");

        // Only the main player sends room data
        if (PhotonNetwork.IsMasterClient)
        {
            SendRoomDataToObservers();
        }
    }

    private void Update()
    {
        // Only the main player sends updates
        if (!PhotonNetwork.IsMasterClient) return;
        if (mruk == null || !mruk.IsInitialized) return;

        // Send initial room data or periodic updates
        if (!_roomDataSent || (updateInterval > 0 && Time.time - _lastUpdateTime > updateInterval))
        {
            SendRoomDataToObservers();
            _lastUpdateTime = Time.time;
            _roomDataSent = true;
        }
    }

    private void SendRoomDataToObservers()
    {
        var room = mruk.Rooms.FirstOrDefault();
        if (room == null)
        {
            Debug.LogWarning("[NetworkedRoomScan] No room found to send");
            return;
        }

        // Cache the room for late joiners
        _cachedRoom = room;

        Debug.Log("[NetworkedRoomScan] Sending room data to all observers...");

        // First, tell all observers to clear their old visualizations
        photonView.RPC("RPC_ClearVisualizations", RpcTarget.Others);

        // Send floor data
        var floor = room.GetFloorAnchor();
        if (floor != null && showFloor)
        {
            SendAnchorData("Floor", floor);
        }

        // Send wall data
        var walls = room.WallAnchors;
        if (walls != null && showWalls)
        {
            for (int i = 0; i < walls.Count; i++)
            {
                SendAnchorData($"Wall_{i}", walls[i]);
            }
        }

        // Send ceiling data
        var ceiling = room.GetCeilingAnchor();
        if (ceiling != null && showCeiling)
        {
            SendAnchorData("Ceiling", ceiling);
        }

        Debug.Log($"[NetworkedRoomScan] Sent room data: Floor={floor != null}, Walls={walls?.Count ?? 0}, Ceiling={ceiling != null}");
    }

    // Send room data to a specific player (for late joiners)
    private void SendRoomDataToSpecificPlayer(Photon.Realtime.Player targetPlayer)
    {
        if (_cachedRoom == null)
        {
            Debug.LogWarning("[NetworkedRoomScan] No cached room data to send");
            return;
        }

        Debug.Log($"[NetworkedRoomScan] Sending room data to player {targetPlayer.ActorNumber}...");

        // First, tell the new observer to clear any old visualizations
        photonView.RPC("RPC_ClearVisualizations", targetPlayer);

        // Send floor data
        var floor = _cachedRoom.GetFloorAnchor();
        if (floor != null && showFloor)
        {
            SendAnchorData("Floor", floor, targetPlayer);
        }

        // Send wall data
        var walls = _cachedRoom.WallAnchors;
        if (walls != null && showWalls)
        {
            for (int i = 0; i < walls.Count; i++)
            {
                SendAnchorData($"Wall_{i}", walls[i], targetPlayer);
            }
        }

        // Send ceiling data
        var ceiling = _cachedRoom.GetCeilingAnchor();
        if (ceiling != null && showCeiling)
        {
            SendAnchorData("Ceiling", ceiling, targetPlayer);
        }

        Debug.Log($"[NetworkedRoomScan] Sent room data to player {targetPlayer.ActorNumber}: Floor={floor != null}, Walls={walls?.Count ?? 0}, Ceiling={ceiling != null}");
    }

    private void SendAnchorData(string anchorId, MRUKAnchor anchor, Photon.Realtime.Player targetPlayer = null)
    {
        if (anchor == null) return;

        // Get plane boundary (for floor, walls, ceiling)
        var planeBoundary = anchor.PlaneBoundary2D;
        if (planeBoundary == null || planeBoundary.Count == 0)
        {
            Debug.LogWarning($"[NetworkedRoomScan] No plane boundary for {anchorId}");
            return;
        }

        // Convert plane boundary to serializable format
        Vector3[] boundaryPoints = planeBoundary.Select(v => anchor.transform.TransformPoint(new Vector3(v.x, 0, v.y))).ToArray();

        // Get transform data
        Vector3 position = anchor.transform.position;
        Quaternion rotation = anchor.transform.rotation;
        Vector3 scale = anchor.transform.localScale;

        // Send via RPC - either to a specific player or to all observers
        if (targetPlayer != null)
        {
            // Send to specific player (for late joiners)
            photonView.RPC("ReceiveAnchorData", targetPlayer,
                anchorId,
                SerializeVector3Array(boundaryPoints),
                position,
                rotation,
                scale);
        }
        else
        {
            // Send to all observers
            photonView.RPC("ReceiveAnchorData", RpcTarget.Others,
                anchorId,
                SerializeVector3Array(boundaryPoints),
                position,
                rotation,
                scale);
        }
    }

    [PunRPC]
    private void ReceiveAnchorData(string anchorId, string serializedBoundary, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Debug.Log($"[NetworkedRoomScan] Received anchor data: {anchorId}");

        Vector3[] boundaryPoints = DeserializeVector3Array(serializedBoundary);
        if (boundaryPoints == null || boundaryPoints.Length < 3)
        {
            Debug.LogWarning($"[NetworkedRoomScan] Invalid boundary data for {anchorId}");
            return;
        }

        // Create or update visualization
        VisualizeAnchor(anchorId, boundaryPoints, position, rotation, scale);
    }

    [PunRPC]
    private void RPC_ClearVisualizations()
    {
        Debug.Log("[NetworkedRoomScan] Clearing all visualizations (received RPC)");
        ClearVisualizations();
    }

    private void VisualizeAnchor(string anchorId, Vector3[] boundaryPoints, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        // Check if we already have a visualization for this anchor
        if (_visualizedObjects.ContainsKey(anchorId))
        {
            Destroy(_visualizedObjects[anchorId]);
        }

        // Create a new GameObject for this anchor
        GameObject anchorObj = new GameObject($"Visualized_{anchorId}");
        anchorObj.transform.position = position;
        anchorObj.transform.rotation = rotation;
        anchorObj.transform.localScale = scale;

        // Create mesh from boundary points
        Mesh mesh = CreateMeshFromBoundary(boundaryPoints, position);

        // Add mesh components
        MeshFilter meshFilter = anchorObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = anchorObj.AddComponent<MeshRenderer>();
        if (roomVisualizationMaterial != null)
        {
            meshRenderer.material = roomVisualizationMaterial;
        }
        else
        {
            // Create a default semi-transparent material
            Material defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            defaultMat.SetFloat("_Mode", 3); // Transparent mode
            defaultMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            defaultMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            defaultMat.SetInt("_ZWrite", 0);
            defaultMat.DisableKeyword("_ALPHATEST_ON");
            defaultMat.EnableKeyword("_ALPHABLEND_ON");
            defaultMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            defaultMat.renderQueue = 3000;
            meshRenderer.material = defaultMat;
        }

        _visualizedObjects[anchorId] = anchorObj;
        Debug.Log($"[NetworkedRoomScan] Created visualization for {anchorId} with {boundaryPoints.Length} boundary points");
    }

    private Mesh CreateMeshFromBoundary(Vector3[] boundaryPoints, Vector3 centerPosition)
    {
        Mesh mesh = new Mesh();
        mesh.name = "RoomAnchorMesh";

        // Convert world space boundary points to local space relative to center
        Vector3[] localPoints = new Vector3[boundaryPoints.Length];
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            localPoints[i] = boundaryPoints[i] - centerPosition;
        }

        // Create vertices
        List<Vector3> vertices = new List<Vector3>(localPoints);

        // Triangulate the boundary using a simple fan triangulation
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // Helper methods to serialize/deserialize Vector3 arrays for RPC
    private string SerializeVector3Array(Vector3[] points)
    {
        List<float> values = new List<float>();
        foreach (var point in points)
        {
            values.Add(point.x);
            values.Add(point.y);
            values.Add(point.z);
        }
        return string.Join(",", values);
    }

    private Vector3[] DeserializeVector3Array(string serialized)
    {
        try
        {
            string[] parts = serialized.Split(',');
            if (parts.Length % 3 != 0) return null;

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < parts.Length; i += 3)
            {
                points.Add(new Vector3(
                    float.Parse(parts[i]),
                    float.Parse(parts[i + 1]),
                    float.Parse(parts[i + 2])
                ));
            }
            return points.ToArray();
        }
        catch
        {
            Debug.LogError("[NetworkedRoomScan] Failed to deserialize Vector3 array");
            return null;
        }
    }

    // Public method to manually trigger room data send
    public void ForceUpdateRoomData()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _roomDataSent = false;
            SendRoomDataToObservers();
        }
    }

    // Clear visualizations on observer side
    public void ClearVisualizations()
    {
        foreach (var obj in _visualizedObjects.Values)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        _visualizedObjects.Clear();
        Debug.Log("[NetworkedRoomScan] Cleared all visualizations");
    }
}
