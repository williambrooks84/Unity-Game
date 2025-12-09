using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA; 
    public Transform pointB; 
    public float speed = 2f;
    public float waitTime = 0.5f;

    private Vector3 target;
    private bool movingToB = true;
    private float waitTimer = 0f;

    void Start()
    {
        if (pointA != null && pointB != null)
            target = pointB.position;
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            movingToB = !movingToB;
            target = movingToB ? pointB.position : pointA.position;
            waitTimer = waitTime;
        }
    }

    Vector3 lastPosition;

    void LateUpdate()
    {
        lastPosition = transform.position;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 platformVelocity = (transform.position - lastPosition) / Time.deltaTime;
                if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
                {
                    rb.linearVelocity += platformVelocity;
                }
            }
        }
    }
}