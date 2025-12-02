using UnityEngine;
using Photon.Pun;

/// <summary>
/// Synchronizes the main player's camera/head position and rotation to observers.
/// Allows observers to see the player's POV.
/// </summary>
public class NetworkedPlayerState : MonoBehaviour, MonoBehaviourPun, IPunObservable
{
    [Header("References")]
    public Transform mainCameraTransform; // Assign CenterEyeAnchor in inspector

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    [Header("Interpolation")]
    public float lerpSpeed = 15f;

    private void Start()
    {
        // If mainCameraTransform not assigned, try to find it
        if (mainCameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCameraTransform = mainCam.transform;
            }
        }
    }

    private void Update()
    {
        // If not owned by this client (observer), interpolate to network transform
        if (photonView != null && !photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * lerpSpeed);
        }
    }

    // Photon serialization
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Main player sends camera transform
            if (mainCameraTransform != null)
            {
                stream.SendNext(mainCameraTransform.position);
                stream.SendNext(mainCameraTransform.rotation);
            }
            else
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
        }
        else
        {
            // Observers receive camera transform
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    // Get current camera position for observers
    public Vector3 GetCameraPosition()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return _networkPosition;
        }

        return mainCameraTransform != null ? mainCameraTransform.position : transform.position;
    }

    // Get current camera rotation for observers
    public Quaternion GetCameraRotation()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return _networkRotation;
        }

        return mainCameraTransform != null ? mainCameraTransform.rotation : transform.rotation;
    }
}
