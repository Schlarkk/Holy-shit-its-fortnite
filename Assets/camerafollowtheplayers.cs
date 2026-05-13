using UnityEngine;

public class DualTargetCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform targetA;
    public Transform targetB;

    [Header("Offset & Angle")]
    [Tooltip("Height above the midpoint.")]
    public float heightOffset = 5f;
    [Tooltip("How far back from the midpoint (base zoom distance).")]
    public float distanceBehind = 8f;
    [Tooltip("Fixed tilt angle (degrees) looking down at the action.")]
    [Range(10f, 80f)]
    public float tiltAngle = 30f;

    [Header("Zoom")]
    [Tooltip("Extra distance added per unit of separation between targets.")]
    public float zoomPerUnit = 0.6f;
    [Tooltip("Minimum and maximum camera distance from midpoint.")]
    public float minDistance = 5f;
    public float maxDistance = 30f;

    [Header("Smoothing")]
    [Tooltip("How fast the camera tracks position (lower = smoother/laggier).")]
    public float positionSmoothing = 5f;
    [Tooltip("How fast the camera tracks rotation.")]
    public float rotationSmoothing = 4f;

    [Header("Camera Axis")]
    [Tooltip("Lock the camera to one axis (e.g. side-scroller or top-down sports).")]
    public bool lockXAxis = false;   // Lock horizontal drift
    public bool lockZAxis = false;   // Lock depth drift

    // ── private state ────────────────────────────────────────────────────────
    private Vector3  _desiredPosition;
    private Quaternion _desiredRotation;

    void LateUpdate()
    {
        if (targetA == null || targetB == null) return;

        // ── 1. Midpoint between the two targets ──────────────────────────────
        Vector3 midpoint = (targetA.position + targetB.position) * 0.5f;

        // ── 2. Separation-based zoom ─────────────────────────────────────────
        float separation = Vector3.Distance(targetA.position, targetB.position);
        float dynamicDistance = Mathf.Clamp(
            distanceBehind + separation * zoomPerUnit,
            minDistance,
            maxDistance
        );

        // ── 3. Camera position: pull back and up from midpoint ───────────────
        //    Direction we "pull back" from: invert the camera's current look
        //    direction projected on the ground plane so we orbit smoothly.
        Vector3 pullDirection = -transform.forward;
        pullDirection.y = 0f;
        if (pullDirection == Vector3.zero) pullDirection = Vector3.back; // fallback
        pullDirection.Normalize();

        Vector3 targetPos = midpoint
            + pullDirection  * dynamicDistance
            + Vector3.up     * heightOffset;

        // Optional axis locks
        if (lockXAxis) targetPos.x = transform.position.x;
        if (lockZAxis) targetPos.z = transform.position.z;

        _desiredPosition = targetPos;

        // ── 4. Look at midpoint with tilt ────────────────────────────────────
        Vector3 lookTarget = midpoint;
        Quaternion lookRot  = Quaternion.LookRotation(lookTarget - targetPos);
        // Apply fixed downward tilt
        _desiredRotation = lookRot * Quaternion.Euler(tiltAngle, 0f, 0f);

        // ── 5. Smooth and apply ───────────────────────────────────────────────
        transform.position = Vector3.Lerp(
            transform.position,
            _desiredPosition,
            Time.deltaTime * positionSmoothing
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _desiredRotation,
            Time.deltaTime * rotationSmoothing
        );
    }

    // ── Editor helper: draw gizmos so you can see the rig in Scene view ──────
    void OnDrawGizmosSelected()
    {
        if (targetA == null || targetB == null) return;

        Vector3 mid = (targetA.position + targetB.position) * 0.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetA.position, targetB.position);
        Gizmos.DrawWireSphere(mid, 0.25f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, mid);
    }
}