using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{
    public Transform[] goals;
    public float arriveThreshold = 0.5f;

    [Header("Detection")]
    public Transform player;
    public float detectRadius = 8f; 

    NavMeshAgent agent;
    Animator animator;
    int currentIndex = 0;
    bool chasingPlayer = false;

    public bool randomMovementEnabled = false;
    public float randomMoveRadius = 10f;
    private Vector3 randomDestination;
    private float randomArriveThreshold = 1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (goals != null && goals.Length > 0)
        {
            agent.destination = goals[0].position;
        }
    }

    void Update()
    {
        if (randomMovementEnabled)
        {
            if (!agent.pathPending && agent.remainingDistance <= randomArriveThreshold)
            {
                Vector2 randCircle = Random.insideUnitCircle * randomMoveRadius;
                Vector3 randPos = transform.position + new Vector3(randCircle.x, 0, randCircle.y);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randPos, out hit, randomMoveRadius, NavMesh.AllAreas))
                {
                    randomDestination = hit.position;
                    agent.destination = randomDestination;
                }
            }
        }
        else
        {
            if (player != null && Vector3.Distance(transform.position, player.position) < detectRadius)
            {
                chasingPlayer = true;
            }
            else
            {
                chasingPlayer = false;
            }

            if (chasingPlayer && player != null)
            {
                agent.destination = player.position;
            }
            else
            {
                if (goals == null || goals.Length == 0) return;
                if (!agent.pathPending && agent.remainingDistance <= arriveThreshold)
                {
                    currentIndex = (currentIndex + 1) % goals.Length;
                    agent.destination = goals[currentIndex].position;
                }
            }
        }

        if (animator != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("Walk", isWalking);
            animator.SetFloat("Speed", agent.velocity.magnitude);

            animator.SetBool("Grounded", true); 
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}