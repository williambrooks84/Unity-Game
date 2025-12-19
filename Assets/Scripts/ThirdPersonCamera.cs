using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("The character / object the camera follows")]
    public Transform target;

    [Tooltip("Default offset from the target in local space (x,right ; y,up ; z,back)")]
    public Vector3 offset = new Vector3(0f, 1.8f, -4f);

    public float followSmoothTime = 0.15f;
    public float rotationSmoothSpeed = 10f;

    [Header("Optional mouse orbit (legacy Input). If you use the new Input System, wire controls instead)")]
    public bool enableMouseOrbit = true;
    public float orbitSpeed = 120f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    Vector3 velocity = Vector3.zero;
    float yaw;
    float pitch;

    void Start()
    {
        if (target == null) Debug.LogWarning("ThirdPersonCamera: assign target in inspector.");
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (enableMouseOrbit)
        {
            try
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(mx) > 0.0001f || Mathf.Abs(my) > 0.0001f)
                {
                    yaw += mx * orbitSpeed * Time.unscaledDeltaTime;
                    pitch -= my * orbitSpeed * Time.unscaledDeltaTime;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }
            }
            catch (System.Exception)
            {
            }
        }

        Quaternion yawOnly = Quaternion.Euler(0f, yaw + target.eulerAngles.y, 0f);
        Vector3 horizontalOffset = yawOnly * new Vector3(offset.x, 0f, offset.z);
        Vector3 desiredPosition = target.position + Vector3.up * offset.y + horizontalOffset;
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);
        
        Vector3 lookAtPos = target.position + Vector3.up * 1.25f;
        Quaternion lookRot = Quaternion.LookRotation(lookAtPos - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSmoothSpeed * Time.deltaTime);
    }
}