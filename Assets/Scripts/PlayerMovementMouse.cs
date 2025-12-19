using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovementMouse : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 720f; // degrees per second

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.15f;
    public Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);
    public float groundedRayDistance = 0.2f; // extra raycast distance below feet

    private Rigidbody rb;
    private Animator animator;
    private int _hashGrounded;
    private int _hashSpeed;
    private int _hashIsJumping;

    private bool isGrounded;
    private bool jumpPressed;

    private float moveInput;
    private float strafeInput;
    private bool zKeyPressed; // camera-forward movement flag

    private Vector3 mouseWorldPos;

    [Header("Shooting")]
    public Transform muzzle;                  // where projectiles spawn
    public GameObject projectilePrefab;       // assign your projectile prefab
    public float fireCooldown = 0.15f;        // time between shots
    private float _lastFireTime;
    private bool _firePressed;

    [Header("Audio")]
    public AudioClip shotSound;               // player shot sound

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            _hashGrounded = Animator.StringToHash("Grounded");
            _hashSpeed = Animator.StringToHash("Speed");
            _hashIsJumping = Animator.StringToHash("IsJumping");
        }
        
        // Initialize fire time to prevent accidental shot at start
        _lastFireTime = Time.time;
    }

    private bool AnimatorHasParameter(Animator anim, int paramHash)
    {
        if (anim == null) return false;
        foreach (var p in anim.parameters)
        {
            if (p.nameHash == paramHash) return true;
        }
        return false;
    }

    void Update()
    {
        // --- Input ---
        moveInput = 0f;
        strafeInput = 0f;
        zKeyPressed = false;

        if (Keyboard.current != null)
        {
            // WASD / Arrow keys
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) strafeInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) strafeInput += 1f;
            
            // Z key: walk toward camera forward (overrides other movement)
            if (Keyboard.current.zKey.isPressed)
            {
                zKeyPressed = true;
                moveInput = 0f;
                strafeInput = 0f;
            }

            // Jump input (check space even if not grounded yet, will apply in FixedUpdate)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpPressed = true;
        }

        // --- Fire input ---
        _firePressed = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            // isPressed allows holding the button down (not just one frame)
            _firePressed = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        }

        // --- Mouse aiming ---
        mouseWorldPos = transform.position + transform.forward * 10f; // fallback
        if (Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, groundMask))
            {
                mouseWorldPos = hit.point;
            }
            else
            {
                // Fallback: aim far away in the mouse direction (e.g. 50 units)
                mouseWorldPos = ray.GetPoint(50f);
            }
        }

        // Animator updates moved to FixedUpdate to reflect physics-grounded state
    }

    void FixedUpdate()
    {
        // --- Ground check ---
        isGrounded = CheckGrounded();

        // --- Movement: always relative to camera direction ---
        Vector3 moveDir = Vector3.zero;
        
        if (Camera.main != null)
        {
            // Get camera forward and right on horizontal plane
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            // Movement relative to camera direction (W goes camera-forward, A/D strafe camera-relative)
            moveDir = (camForward * moveInput + camRight * strafeInput).normalized;
        }
        
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // --- Rotation handled by FirstPersonView when rotatePlayerWithMouse = true ---
        // (Player yaw automatically follows camera horizontal look)

        // --- Animator updates (reflect physics state) ---
        if (animator != null)
        {
            float speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            animator.SetFloat(_hashSpeed, speed);
            animator.SetBool(_hashGrounded, isGrounded);
        }

        // --- Jump ---
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
            if (animator != null && AnimatorHasParameter(animator, _hashIsJumping)) animator.SetBool(_hashIsJumping, true);
        }

        // --- Ground stick (optional) ---
        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
            if (animator != null && AnimatorHasParameter(animator, _hashIsJumping)) animator.SetBool(_hashIsJumping, false);
        }

        // --- Shooting ---
        if (_firePressed && projectilePrefab != null && muzzle != null)
        {
            if (Time.time - _lastFireTime >= fireCooldown)
            {
                Vector3 dir = (mouseWorldPos - muzzle.position);
                if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
                dir.y = 0f; // keep level if you prefer horizontal shots
                dir.Normalize();

                var go = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
                var proj = go.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.owner = transform;
                    proj.Launch(dir * proj.speed);
                }
                else
                {
                    var rbp = go.GetComponent<Rigidbody>();
                    if (rbp != null) rbp.linearVelocity = dir * 25f;
                }

                // prevent self-collision
                var projCol = go.GetComponent<Collider>();
                if (projCol != null)
                {
                    foreach (var col in GetComponentsInChildren<Collider>())
                    {
                        if (col != null) Physics.IgnoreCollision(projCol, col, true);
                    }
                }

                // Play shot sound - create new AudioSource for each shot so they can overlap
                if (shotSound != null && muzzle != null)
                {
                    GameObject tempAudio = new GameObject("PlayerShot");
                    tempAudio.transform.position = muzzle.position;
                    AudioSource audioSrc = tempAudio.AddComponent<AudioSource>();
                    audioSrc.clip = shotSound;
                    audioSrc.volume = 0.8f;
                    audioSrc.pitch = Random.Range(0.9f, 1.1f);
                    audioSrc.spatialBlend = 1f; // 3D audio
                    audioSrc.rolloffMode = AudioRolloffMode.Linear;
                    audioSrc.minDistance = 1f;
                    audioSrc.maxDistance = 30f;
                    audioSrc.Play();
                    Destroy(tempAudio, shotSound.length);
                }

                _lastFireTime = Time.time;
            }
        }
    }

    private bool CheckGrounded()
    {
        // 1) OverlapSphere near feet
        Vector3 spherePosition = transform.position + groundCheckOffset;
        Collider[] hits = Physics.OverlapSphere(spherePosition, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody == rb) continue;
            if (hits[i].gameObject == gameObject) continue;
            if (hits[i].isTrigger) continue;
            return true;
        }

        // 2) Capsule-based spherecast straight down from feet for robustness
        var cap = GetComponent<CapsuleCollider>();
        if (cap != null)
        {
            Vector3 center = transform.TransformPoint(cap.center);
            float bottom = center.y - (cap.height * 0.5f) + cap.radius;
            Vector3 origin = new Vector3(center.x, bottom + 0.05f, center.z);
            Ray ray = new Ray(origin, Vector3.down);
            if (Physics.SphereCast(ray, cap.radius * 0.95f, groundedRayDistance, groundMask, QueryTriggerInteraction.Ignore))
                return true;
        }

        // 3) Simple raycast down a short distance
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayOrigin, Vector3.down, 0.25f, groundMask, QueryTriggerInteraction.Ignore))
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}
