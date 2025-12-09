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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>(); // get Animator from child if not on root
        if (goals != null && goals.Length > 0)
        {
            agent.destination = goals[0].position;
        }
    }

    void Update()
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

        // --- Animation logic ---
        if (animator != null)
        {
            bool isWalking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("Walk", isWalking);
            animator.SetFloat("Speed", agent.velocity.magnitude);

            animator.SetBool("Grounded", true); 
        }
    }
}