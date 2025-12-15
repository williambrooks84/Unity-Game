using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 20;

    [Header("Flight")]
    public float speed = 25f;
    public float lifetime = 5f;
    public bool useGravity = false;

    [Header("Collision / Effects")]
    public LayerMask hitLayers = ~0; 
    public GameObject impactPrefab; 

    [Header("Owner")]
    public Transform owner; 

    Rigidbody rb;
    bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;

        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        HandleHit(collision.collider, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        Vector3 hitPoint;
        if (other is BoxCollider || other is SphereCollider || other is CapsuleCollider || (other is MeshCollider meshCol && meshCol.convex))
            hitPoint = other.ClosestPoint(transform.position);
        else
            hitPoint = transform.position;
        HandleHit(other, hitPoint);
    }

    void HandleHit(Collider col, Vector3 hitPoint)
    {
        if (owner != null && col.transform.root == owner.root) return;

        if (((1 << col.gameObject.layer) & hitLayers) == 0) return;
        var health = col.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage, owner != null ? owner.root.gameObject : null);

            bool playerOwned = (owner != null && owner.root.CompareTag("Player"));
            if (playerOwned)
            {
                var menu = Object.FindObjectOfType<Menu>();
                // if (menu != null) menu.AddScore(1);
            }
        }

        if (impactPrefab != null)
        {
            Instantiate(impactPrefab, hitPoint, Quaternion.identity);
        }

        hasHit = true;
        Destroy(gameObject);
    }
    
    public void Launch(Vector3 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.linearVelocity = velocity;
    }
}
