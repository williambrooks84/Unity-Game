using UnityEngine;

public class FirstPersonView : MonoBehaviour
{
    [Tooltip("The character / object the camera follows (usually the player root)")]
    public Transform target;

    [Tooltip("Offset from target in local space. Example: (0, 1.6, 0) for head height.")]
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    [Header("Smoothing")]
    public float followSmoothTime = 0.05f; 
    public float rotationSmoothSpeed = 20f;  

    [Header("Mouse Look (legacy Input)")]
    public bool enableMouseLook = true;
    public float lookSensitivity = 150f;
    public float minPitch = -75f;
    public float maxPitch = 85f;
    public bool lockCursorOnStart = true;

    [Header("Player Rotation")]
    public bool rotatePlayerWithMouse = true; 

    Vector3 _vel;
    float _yaw;
    float _pitch;

    void Start()
    {
        if (target == null) Debug.LogWarning("FirstPersonView: assign target in inspector.");
        var ang = transform.eulerAngles;
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

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        if (Cursor.lockState != CursorLockMode.Locked && Cursor.visible)
        {
            var menu = FindObjectOfType<Menu>();
            bool isGameOver = (menu != null && menu.wastedText != null && menu.wastedText.gameObject.activeInHierarchy) ||
                              (menu != null && menu.victoryText != null && menu.victoryText.gameObject.activeInHierarchy);
            if (!isGameOver)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (enableMouseLook)
        {
            float mx = 0f, my = 0f;
            bool gotDelta = false;
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
        Quaternion desiredRot = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 desiredPos = target.TransformPoint(offset);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, followSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmoothSpeed * Time.deltaTime);
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

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
