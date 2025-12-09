using UnityEngine;

// First-person camera that follows a target at a head offset and rotates by mouse look.
// Mirrors your third-person script: optional legacy Input, pitch clamp, smoothing.
public class FirstPersonView : MonoBehaviour
{
    [Tooltip("The character / object the camera follows (usually the player root)")]
    public Transform target;

    [Tooltip("Offset from target in local space. Example: (0, 1.6, 0) for head height.")]
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    [Header("Smoothing")]
    public float followSmoothTime = 0.05f;   // position smoothing
    public float rotationSmoothSpeed = 20f;  // rotation smoothing

    [Header("Mouse Look (legacy Input)")]
    public bool enableMouseLook = true;
    public float lookSensitivity = 150f;
    public float minPitch = -75f;
    public float maxPitch = 85f;
    public bool lockCursorOnStart = true;

    [Header("Player Rotation")]
    public bool rotatePlayerWithMouse = true; // rotates target yaw when looking left/right horizontally

    Vector3 _vel;
    float _yaw;
    float _pitch;

    void Start()
    {
        if (target == null) Debug.LogWarning("FirstPersonView: assign target in inspector.");
        var ang = transform.eulerAngles;
        // Anchor initial yaw to target when rotating player with mouse to avoid snap
        if (rotatePlayerWithMouse && target != null)
            _yaw = target.eulerAngles.y;
        else
            _yaw = ang.y;
        _pitch = ang.x;

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse look: supports both legacy Input axes and new Input System Mouse.delta when available
        if (enableMouseLook)
        {
            float mx = 0f, my = 0f;
            bool gotDelta = false;
            // New Input System read
            try
            {
                if (UnityEngine.InputSystem.Mouse.current != null)
                {
                    var d = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
                    mx = d.x;
                    my = d.y;
                    gotDelta = Mathf.Abs(mx) > 0.0001f || Mathf.Abs(my) > 0.0001f;
                }
            }
            catch {}

            // Legacy Input fallback
            if (!gotDelta)
            {
                try
                {
                    mx = Input.GetAxis("Mouse X");
                    my = Input.GetAxis("Mouse Y");
                    gotDelta = Mathf.Abs(mx) > 0.0001f || Mathf.Abs(my) > 0.0001f;
                }
                catch {}
            }

            if (gotDelta)
            {
                // Compute deltas
                float yawDelta = mx * lookSensitivity * Time.unscaledDeltaTime;
                float pitchDelta = -my * lookSensitivity * Time.unscaledDeltaTime; // invert mouse Y for pitch

                // Apply pitch locally
                _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);

                // If we should rotate the player with mouse, apply yaw to the target and anchor _yaw to target's world yaw.
                if (rotatePlayerWithMouse && target != null)
                {
                    target.Rotate(0f, yawDelta, 0f, Space.World);
                    _yaw = target.eulerAngles.y;
                }
                else
                {
                    // Camera-only yaw
                    _yaw += yawDelta;
                }
            }
        }

        // Desired rotation: use the computed yaw and pitch (yaw already anchored to target when appropriate)
        Quaternion desiredRot = Quaternion.Euler(_pitch, _yaw, 0f);

        // Desired position: target + local offset (head)
        Vector3 desiredPos = target.TransformPoint(offset);

        // Smooth position & rotation
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, followSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmoothSpeed * Time.deltaTime);
    }

    // Optional helper to toggle cursor lock
    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // New Input System hook: call this from an Input Action (Vector2) for look
    public void ApplyLook(UnityEngine.Vector2 delta)
    {
        float mx = delta.x;
        float my = delta.y;
        float yawDelta = mx * lookSensitivity * Time.unscaledDeltaTime;
        float pitchDelta = -my * lookSensitivity * Time.unscaledDeltaTime;

        _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);
        if (rotatePlayerWithMouse && target != null)
        {
            target.Rotate(0f, yawDelta, 0f, Space.World);
            _yaw = target.eulerAngles.y;
        }
        else
        {
            _yaw += yawDelta;
        }
    }
}
