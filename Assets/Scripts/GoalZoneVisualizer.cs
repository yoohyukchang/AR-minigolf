using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SphereCollider))]
public class GoalZoneVisualizer : MonoBehaviour
{
    public Transform visual; // assign GoalZoneVisual
    private SphereCollider _sphere;

    private void OnEnable()
    {
        _sphere = GetComponent<SphereCollider>();
        UpdateVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (!visual || !_sphere) return;

        // Place visual at collider center
        visual.localPosition = _sphere.center;
        visual.localRotation = Quaternion.identity;

        // Scale visual to match collider diameter
        float diameter = _sphere.radius * 2f;

        // Visual sphere mesh has diameter 1 by default
        visual.localScale = new Vector3(diameter, diameter, diameter);
    }
}
