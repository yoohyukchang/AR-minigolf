using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BallController : MonoBehaviour
{
    public MRUK mruk;
    public Transform playerHead;
    public Transform ballTransform;

    private bool _placed;
    private bool _startRequested;

    // -----------------------------
    // GRASS SLOWDOWN SYSTEM
    // -----------------------------
    [Header("Grass Slowdown Settings")]
    public float dragInGrass = 1.0f;
    public float angularDragInGrass = 1.0f;

    private float _baseDrag;
    private float _baseAngularDrag;
    private int _grassCount = 0; // handles overlapping grass patches

    // -----------------------------
    // RESPAWN SYSTEM
    // -----------------------------
    private Vector3 _originalSpawnPos;
    private bool _spawnPosSet = false;

    public void RegisterOriginalSpawn()
    {
        _originalSpawnPos = ballTransform.position;
        _spawnPosSet = true;
    }

    public void RespawnBall()
    {
        if (!_spawnPosSet || ballTransform == null) return;

        Rigidbody rb = ballTransform.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ballTransform.position = _originalSpawnPos;
        ballTransform.gameObject.SetActive(true);
    }

    // ---------- GRASS API ----------
    public void EnterGrass()
    {
        _grassCount++;
        ApplyDragState();
    }

    public void ExitGrass()
    {
        _grassCount = Mathf.Max(0, _grassCount - 1);
        ApplyDragState();
    }

    private void ApplyDragState()
    {
        Rigidbody rb = ballTransform.GetComponent<Rigidbody>();

        if (_grassCount > 0)
        {
            rb.linearDamping = dragInGrass;
            rb.angularDamping = angularDragInGrass;
        }
        else
        {
            rb.linearDamping = _baseDrag;
            rb.angularDamping = _baseAngularDrag;
        }
    }

    void Start()
    {
        if (!mruk)
        {
            mruk = FindObjectOfType<MRUK>();
        }

        // Make sure the ball is hidden until the game actually starts
        if (ballTransform != null)
        {
            ballTransform.gameObject.SetActive(false);
        }

        Rigidbody rb = ballTransform.GetComponent<Rigidbody>();
        _baseDrag = rb.linearDamping;
        _baseAngularDrag = rb.angularDamping;
    }

    // Called by the Start button (via StartGameManager)
    public void BeginGame()
    {
        _startRequested = true;
        _placed = false;

        if (ballTransform != null)
        {
            // Show the ball so physics can act on it
            ballTransform.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Do nothing until:
        //  - Start has been requested
        //  - MRUK is initialized
        //  - We haven't already placed the ball
        if (!_startRequested || _placed || mruk == null) return;
        if (!mruk.IsInitialized) return;

        PlaceBall();
        _placed = true;
    }

    void PlaceBall()
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

        // Put cuballbe 0.5m in front of user and 1m above the floor
        Vector3 userOnFloor = new Vector3(headPos.x, floorY, headPos.z);
        Vector3 ballPos = userOnFloor + forwardFlat * 0.5f + Vector3.up * 1.0f;

        ballTransform.position = ballPos;
        ballTransform.rotation = Quaternion.identity;

        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        RegisterOriginalSpawn();
        Debug.Log("MRUK: Ball placed above floor.");
    }

    public void OnManholeHit()
    {
        if (!_spawnPosSet) return;

        StopAllCoroutines();
        StartCoroutine(ManholeRespawnRoutine());
    }

    private System.Collections.IEnumerator ManholeRespawnRoutine()
    {
        Rigidbody rb = ballTransform.GetComponent<Rigidbody>();

        // Freeze physics immediately
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Hide ball
        ballTransform.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Restore
        ballTransform.position = _originalSpawnPos;
        ballTransform.gameObject.SetActive(true);

        rb.isKinematic = false;
    }

}
