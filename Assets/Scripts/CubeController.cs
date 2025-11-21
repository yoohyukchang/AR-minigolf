using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class CubeController : MonoBehaviour
{

    public MRUK mruk;
    public Transform playerHead;
    public Transform cubeTransform;

    private bool _placed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!mruk)
        {
            mruk = FindObjectOfType<MRUK>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_placed || mruk == null) return;

        // Wait until MRUK has loaded the room
        if (!mruk.IsInitialized) return;

        PlaceCube();
        _placed = true;
    }

    void PlaceCube()
    {
        var room = mruk.Rooms.FirstOrDefault();
        if (room == null)
        {
            Debug.LogWarning("MRUK: No room found.");
            return;
        }

        var floor = room.GetFloorAnchor();
        if (floor == null)
        {
            Debug.LogWarning("MRUK: No floor anchor found.");
            return;
        }

        float floorY = floor.transform.position.y;

        Vector3 headPos = playerHead.position;
        Vector3 forwardFlat = Vector3.ProjectOnPlane(playerHead.forward, Vector3.up).normalized;
        if (forwardFlat.sqrMagnitude < 1e-4f)
        {
            forwardFlat = Vector3.forward;
        }

        // Put cube 0.5m in front of user and 1m above the floor
        Vector3 userOnFloor = new Vector3(headPos.x, floorY, headPos.z);
        Vector3 cubePos = userOnFloor + forwardFlat * 0.5f + Vector3.up * 1.0f;

        cubeTransform.position = cubePos;
        cubeTransform.rotation = Quaternion.identity;

        // Reset velocities if there is a Rigidbody
        var rb = cubeTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("MRUK: Cube placed above floor.");
    }
}
