using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 180f;

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.15f;
    public Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);

    private Rigidbody rb;
    private Animator animator;

    private bool isGrounded;
    private bool jumpPressed;

    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- Input ---
        moveInput = 0f;
        turnInput = 0f;

        if (Keyboard.current != null)
        {
            // WASD / ZQSD
            if (Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed) turnInput -= 1f;
            if (Keyboard.current.dKey.isPressed) turnInput += 1f;

            // Arrow keys
            if (Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.leftArrowKey.isPressed) turnInput -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) turnInput += 1f;

            // Jump input (only registers if grounded)
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
                jumpPressed = true;
        }

        // --- Animator updates ---
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("Grounded", isGrounded);
    }

    void FixedUpdate()
    {
        // --- Ground check ---
        isGrounded = CheckGrounded();

        // --- Movement ---
        Vector3 forwardMove = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        float rotation = turnInput * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));

        // --- Jump ---
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }

        // --- Ground stick (optional) ---
        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
        }
    }

    private bool CheckGrounded()
    {
        Vector3 spherePosition = transform.position + groundCheckOffset;
        Collider[] hits = Physics.OverlapSphere(spherePosition, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            // Ignore self
            if (hits[i].attachedRigidbody == rb) continue;
            if (hits[i].gameObject == gameObject) continue;
            if (hits[i].isTrigger) continue;

            return true; // touching ground
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}
