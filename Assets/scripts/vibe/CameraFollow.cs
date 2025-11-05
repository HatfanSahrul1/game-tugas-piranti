using UnityEngine;

/// <summary>
/// Simple smooth camera follow.
/// Keeps a fixed offset from the target and smoothly damps position changes.
/// This version is intentionally minimal: no dead zones, no look-ahead, no speed-based offsets.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField, Tooltip("Time it takes to smooth to the target. Lower is snappier.")]
    private float smoothTime = 0.15f;

    // Internal velocity used by SmoothDamp
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        if (target == null)
        {
            target = FindObjectOfType<PlayerController>()?.transform;
        }

        // Snap to initial position
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// Set the camera follow target at runtime.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Set the follow offset.
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    /// <summary>
    /// Immediately snap the camera to the target+offset (no smoothing).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        velocity = Vector3.zero;
    }
}