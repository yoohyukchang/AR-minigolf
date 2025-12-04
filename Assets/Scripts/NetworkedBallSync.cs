using UnityEngine;
using Photon.Pun;

/// <summary>
/// Synchronizes golf ball position, velocity, and rotation across the network.
/// Main player owns the physics simulation, observers receive updates.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class NetworkedBallSync : MonoBehaviourPun, IPunObservable
{
    private Rigidbody _rigidbody;
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private Vector3 _networkVelocity;
    private Vector3 _networkAngularVelocity;

    [Header("Interpolation Settings")]
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 10f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Subscribe to GameStateManager events
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnBallSpawned += OnBallSpawned;
            GameStateManager.Instance.OnBallHit += OnBallHit;
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnBallSpawned -= OnBallSpawned;
            GameStateManager.Instance.OnBallHit -= OnBallHit;
        }
    }

    private void Update()
    {
        // If not owned by this client (observer), interpolate to network position
        if (photonView != null && !photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * positionLerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * rotationLerpSpeed);
        
            // Apply network velocities for realistic motion
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = _networkVelocity;
                _rigidbody.angularVelocity = _networkAngularVelocity;
            }
        }
    }

    // Called when ball is spawned
    private void OnBallSpawned(Vector3 position, Quaternion rotation)
    {
        if (PhotonNetwork.IsMasterClient && photonView != null && photonView.ViewID != 0)
        {
            // Send RPC to all observers
            photonView.RPC("RPC_BallSpawned", RpcTarget.Others, position, rotation);
        }
        else if (photonView == null || photonView.ViewID == 0)
        {
            Debug.LogWarning("[NetworkedBallSync] Cannot send RPC_BallSpawned - PhotonView not initialized");
        }
    }

    // Called when ball is hit
    private void OnBallHit(Vector3 position, Vector3 velocity, Quaternion rotation)
    {
        if (PhotonNetwork.IsMasterClient && photonView != null && photonView.ViewID != 0)
        {
            // Send RPC to all observers
            photonView.RPC("RPC_BallHit", RpcTarget.Others, position, velocity, rotation);
        }
        else if (photonView == null || photonView.ViewID == 0)
        {
            Debug.LogWarning("[NetworkedBallSync] Cannot send RPC_BallHit - PhotonView not initialized");
        }
    }

    // Photon serialization
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Main player sends data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(_rigidbody.linearVelocity);
            stream.SendNext(_rigidbody.angularVelocity);
        }
        else
        {
            // Observers receive data
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _networkVelocity = (Vector3)stream.ReceiveNext();
            _networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    private void RPC_BallSpawned(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        gameObject.SetActive(true);
        Debug.Log("[NetworkedBallSync] Ball spawned via RPC");
    }

    [PunRPC]
    private void RPC_BallHit(Vector3 position, Vector3 velocity, Quaternion rotation)
    {
        _networkPosition = position;
        _networkRotation = rotation;
        _networkVelocity = velocity;
        Debug.Log($"[NetworkedBallSync] Ball hit via RPC, velocity: {velocity.magnitude:F2}");
    }
}
