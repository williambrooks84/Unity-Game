using UnityEngine;

public class NPCShooter : MonoBehaviour
{
    public Transform player;
    public GameObject projectilePrefab;
    public Transform muzzle;
    public float detectRadius = 10f;
    public float fireCooldown = 1f;
    public float projectileSpeed = 20f;

    float lastFireTime = -999f;

    void Update()
    {
        if (player == null || projectilePrefab == null || muzzle == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectRadius)
        {
            // Face the player
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 720f * Time.deltaTime);
            }

            if (Time.time - lastFireTime >= fireCooldown)
            {
                FireAtPlayer();
                lastFireTime = Time.time;
            }
        }
    }

    void FireAtPlayer()
    {
        Vector3 dir = (player.position - muzzle.position).normalized;
        var go = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.owner = transform;
            proj.Launch(dir * projectileSpeed);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = dir * projectileSpeed;
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}