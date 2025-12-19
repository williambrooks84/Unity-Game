using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovementMouse : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 720f; 

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.15f;
    public Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);
    public float groundedRayDistance = 0.2f;

    private Rigidbody rb;
    private Animator animator;
    private int _hashGrounded;
    private int _hashSpeed;
    private int _hashIsJumping;

    private bool isGrounded;
    private bool jumpPressed;

    private float moveInput;
    private float strafeInput;
    private bool zKeyPressed; 

    private Vector3 mouseWorldPos;

    [Header("Shooting")]
    public Transform muzzle; 
    public GameObject projectilePrefab;
    public float fireCooldown = 0.15f; 
    private float _lastFireTime;
    private bool _firePressed;

    [Header("Audio")]
    public AudioClip shotSound;  
    
    [Range(0f, 1f)]
    public float shotVolume = 0.5f;

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
        moveInput = 0f;
        strafeInput = 0f;
        zKeyPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) strafeInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) strafeInput += 1f;
            
            if (Keyboard.current.zKey.isPressed)
            {
                zKeyPressed = true;
                moveInput = 0f;
                strafeInput = 0f;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpPressed = true;
        }

        _firePressed = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            _firePressed = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        }

        mouseWorldPos = transform.position + transform.forward * 10f; 
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
                mouseWorldPos = ray.GetPoint(50f);
            }
        }

    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        Vector3 moveDir = Vector3.zero;
        
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            moveDir = (camForward * moveInput + camRight * strafeInput).normalized;
        }
        
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        if (animator != null)
        {
            float speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            animator.SetFloat(_hashSpeed, speed);
            animator.SetBool(_hashGrounded, isGrounded);
        }

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
            if (animator != null && AnimatorHasParameter(animator, _hashIsJumping)) animator.SetBool(_hashIsJumping, true);
        }

        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
            if (animator != null && AnimatorHasParameter(animator, _hashIsJumping)) animator.SetBool(_hashIsJumping, false);
        }

        if (_firePressed && projectilePrefab != null && muzzle != null)
        {
            if (Time.time - _lastFireTime >= fireCooldown)
            {
                Vector3 dir = (mouseWorldPos - muzzle.position);
                if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
                dir.y = 0f;
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

                var projCol = go.GetComponent<Collider>();
                if (projCol != null)
                {
                    foreach (var col in GetComponentsInChildren<Collider>())
                    {
                        if (col != null) Physics.IgnoreCollision(projCol, col, true);
                    }
                }

                if (shotSound != null && muzzle != null)
                {
                    GameObject tempAudio = new GameObject("PlayerShot");
                    tempAudio.transform.position = muzzle.position;
                    AudioSource audioSrc = tempAudio.AddComponent<AudioSource>();
                    audioSrc.clip = shotSound;
                    audioSrc.volume = shotVolume;
                    audioSrc.pitch = Random.Range(0.9f, 1.1f);
                    audioSrc.spatialBlend = 1f;
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
        Vector3 spherePosition = transform.position + groundCheckOffset;
        Collider[] hits = Physics.OverlapSphere(spherePosition, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody == rb) continue;
            if (hits[i].gameObject == gameObject) continue;
            if (hits[i].isTrigger) continue;
            return true;
        }

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
