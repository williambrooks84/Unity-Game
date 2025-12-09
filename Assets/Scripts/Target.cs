using UnityEngine;
using UnityEngine.InputSystem;

public class Target : MonoBehaviour
{
    [Header("VFX")]
    public GameObject hitVFX;
    public GameObject deathVFX;

    [Header("Movement on hit")]
    public float hitPush = 2f;
    public float deathPush = 4f;
    public float fallbackMoveDistance = 0.5f;
    public float fallbackMoveTime = 0.15f;

    [Header("Options")]
    public bool destroyOnDeath = true;
    public float deathDestroyDelay = 0.5f;

    [Header("Optional built-in shooter (left-click)")]
    public GameObject projectilePrefab; 
    public Transform muzzle; 
    public float fireCooldown = 0.2f;

    Health health; 
    Rigidbody rb;
    float lastFireTime = -999f;

    void Awake()
    {
        health = GetComponent<Health>(); 
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.onDamaged?.AddListener(OnDamaged);
            health.onDeath?.AddListener(OnDeath);
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.onDamaged?.RemoveListener(OnDamaged);
            health.onDeath?.RemoveListener(OnDeath);
        }
    }

    void OnDamaged(int amount)
    {
        if (hitVFX != null) Instantiate(hitVFX, transform.position, Quaternion.identity);
    }

    void OnDeath()
    {
        if (deathVFX != null) Instantiate(deathVFX, transform.position, Quaternion.identity);
        if (rb != null)
        {
            Vector3 upBack = (Vector3.up * 0.6f + Vector3.back * 0.4f).normalized;
            rb.AddForce(upBack * deathPush, ForceMode.Impulse);
        }
        if (destroyOnDeath) Destroy(gameObject, deathDestroyDelay);
    }

    void OnCollisionEnter(Collision collision)
    {
        var proj = collision.collider.GetComponentInParent<Projectile>();
        if (proj != null)
        {
            if (proj.owner != null && proj.owner.root == transform.root) return;
            Vector3 sourcePos = proj.transform.position;
            ApplyKnockbackFromSource(sourcePos, hitPush);
            if (hitVFX != null && collision.contacts.Length > 0)
                Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var proj = other.GetComponentInParent<Projectile>();
        if (proj != null)
        {
            if (proj.owner != null && proj.owner.root == transform.root) return;
            Vector3 sourcePos = proj.transform.position;
            ApplyKnockbackFromSource(sourcePos, hitPush);
            if (hitVFX != null)
                Instantiate(hitVFX, other.ClosestPoint(transform.position), Quaternion.identity);
        }
    }

    public void ApplyDamage(int amount)
    {
        if (health != null) health.TakeDamage(amount);
    }

    void ApplyKnockbackFromSource(Vector3 sourcePosition, float force)
    {
        Vector3 dir = (transform.position - sourcePosition).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.up + transform.forward * 0.2f;
        if (rb != null) rb.AddForce(dir * force, ForceMode.Impulse);
        else
        {
            Vector3 endPos = transform.position + dir * fallbackMoveDistance;
            StopAllCoroutines();
            StartCoroutine(NudgePosition(transform.position, endPos, fallbackMoveTime));
        }
    }

    System.Collections.IEnumerator NudgePosition(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.position = to;
    }

    void Update()
    {
        if (projectilePrefab == null || muzzle == null) return;

        bool fire = false;
        if (Mouse.current != null)
            fire = Mouse.current.leftButton.wasPressedThisFrame;
        else
        {
            try { fire = UnityEngine.Input.GetMouseButtonDown(0); } catch { fire = false; }
        }

        if (fire && Time.time - lastFireTime >= fireCooldown)
        {
            FireProjectile();
            lastFireTime = Time.time;
        }
    }

    void FireProjectile()
    {
        Vector3 screenPos;
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
        }
        else
        {
            try { screenPos = UnityEngine.Input.mousePosition; }
            catch { screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f); }
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Camera.current;
            if (cam == null)
            {
                return;
            }
        }

        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 targetPoint;
        RaycastHit hit;
        const float fallbackDistance = 50f;
        if (Physics.Raycast(ray, out hit, 1000f))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(fallbackDistance);

        Vector3 dir = (targetPoint - muzzle.position);
        if (dir.sqrMagnitude < 0.0001f) dir = muzzle.forward;
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
    }
}
