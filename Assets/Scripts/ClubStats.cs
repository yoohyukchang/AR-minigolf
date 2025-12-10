using UnityEngine;

[CreateAssetMenu(fileName = "ClubStats", menuName = "MiniGolf/Club Stats")]
public class ClubStats : ScriptableObject
{
    [Header("Meta")]
    public string displayName = "Iron";

    [Header("Launch Shape")]
    [Tooltip("Launch angle in degrees above horizontal (0 = flat, 45 = high arc).")]
    [Range(0f, 80f)]
    public float launchAngleDeg = 30f;

    [Header("Hit Tuning")]
    [Tooltip("Even the slowest tap is treated as at least this speed.")]
    public float minEffectiveSpeed = 0.1f;

    [Tooltip("Base impulse applied even for slow taps.")]
    public float baseImpulse = 0.1f;

    [Tooltip("Extra impulse per m/s of swing speed.")]
    public float swingSpeedMultiplier = 0.1f;

    [Tooltip("Clamp so shots never get too strong.")]
    public float maxImpulse = 0.8f;
}
