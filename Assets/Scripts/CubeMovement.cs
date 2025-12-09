using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeMovement : MonoBehaviour
{
    Rigidbody rb;

    [Header("MovePosition (kinematic-like positional movement)")]
    public Vector3 targetPosition;
    public float moveSpeed = 5f;
    public bool useMovePosition = true;

    [Header("AddForce (physics)")]
    public Vector3 continuousForce = Vector3.zero;
    public float forceMultiplier = 10f;
    public ForceMode forceMode = ForceMode.Force;
    public bool applyForce = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (useMovePosition)
        {
            Vector3 next = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(next);
        }

        if (applyForce)
        {
            rb.AddForce(continuousForce * forceMultiplier, forceMode);
        }
    }
}