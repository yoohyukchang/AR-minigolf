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
    public bool showCeiling = true;

    [Header("Update Settings")]
    [Tooltip("How often to check for room updates (in seconds). Set to 0 to only send once.")]
    public float updateInterval = 60f;

    private float _lastUpdateTime;
    private bool _roomDataSent = false;

    // Cache room data for late joiners
    private MRUKRoom _cachedRoom;

    private void Start()
    {
        if (mruk == null)
        {
            mruk = FindFirstObjectByType<MRUK>();
            _cachedRoom = 
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

        string roomJson = _cachedRoom.SaveToJSON();
        photonView.RPC("ReceiveRoomData", RpcTarget.Others, roomJson);

        Debug.Log($"[NetworkedRoomScan] Sent room data in JSON: {roomJson}");
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

        string roomJson = _cachedRoom.SaveToJSON();
        photonView.RPC("ReceiveRoomData", targetPlayer, roomJson);

        Debug.Log($"[NetworkedRoomScan] Sent room data to player in JSON: {roomJson}");
    }

    [PunRPC]
    void ReceiveRoomData(string roomJson) {
        MRUKRoom existingRoom = MRUK.Instance.GetCurrentRoom();
        if (existingRoom != null) {
            MRUK.Instance.DestroyRoom(existingRoom);
        }
        
        MRUKRoom newRoom = MRUKRoom.LoadFromJson(roomJson);

        MRUKEffectMesh effectMesh = FindObjectOfType<MRUKEffectMesh>();
        if (effectMesh != null) {
            effectMesh.ClearMesh();
            effectMesh.CreateMesh();
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
}
