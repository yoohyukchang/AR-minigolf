using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Manages Photon networking, room creation, and observer connections.
/// Uses a 6-digit room code system for observers to join.
/// </summary>
public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    [Header("Settings")]
    public string gameVersion = "1.0";
    public int maxObserversPerRoom = 10;

    [Header("State")]
    public string CurrentRoomCode { get; private set; }
    public bool IsMainPlayer { get; private set; }
    public bool IsConnected { get; private set; }

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

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        ConnectToPhoton();
    }

    // Connect to Photon Cloud
    public void ConnectToPhoton()
    {
        Debug.Log("[PhotonNetworkManager] Connecting to Photon Cloud...");
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // Create a room as main player (VR headset)
    public void CreateRoomAsMainPlayer()
    {
        IsMainPlayer = true;
        CurrentRoomCode = GenerateRoomCode();

        Debug.Log($"[PhotonNetworkManager] Creating room with code: {CurrentRoomCode}");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)(maxObserversPerRoom + 1), // +1 for main player
            IsVisible = false, // Hidden from lobby list
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(CurrentRoomCode, roomOptions);
    }

    // Join a room as observer (mobile device)
    public void JoinRoomAsObserver(string roomCode)
    {
        IsMainPlayer = false;
        CurrentRoomCode = roomCode.ToUpper();

        Debug.Log($"[PhotonNetworkManager] Joining room: {CurrentRoomCode}");

        PhotonNetwork.JoinRoom(CurrentRoomCode);
    }

    // Generate a random 6-character room code
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars
        char[] code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(code);
    }

    // Photon Callbacks
    public override void OnConnectedToMaster()
    {
        IsConnected = true;
        Debug.Log("[PhotonNetworkManager] Connected to Photon Master Server");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PhotonNetworkManager] Joined room: {CurrentRoomCode}");
        Debug.Log($"[PhotonNetworkManager] Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");

        if (IsMainPlayer)
        {
            // Spawn networked game objects
            SpawnNetworkedObjects();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonNetworkManager] Failed to create room: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonNetworkManager] Failed to join room: {message}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[PhotonNetworkManager] Observer joined: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[PhotonNetworkManager] Observer left: {otherPlayer.NickName}");
    }

    private void SpawnNetworkedObjects()
    {
        // This will be called by the main player to instantiate networked objects
        Debug.Log("[PhotonNetworkManager] Spawning networked objects...");
        // TODO: Add implementation after Photon installation
    }

    // Helper to check if we're the main player
    public bool IsMainPlayerInRoom()
    {
        return PhotonNetwork.IsMasterClient;
    }
}
