using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public int npcCount = 10;
    public Renderer groundRenderer;

    [Header("NPC Audio")]
    public AudioClip npcAudioClip;

    void Start()
    {
        GameObject grassParent = GameObject.Find("Grass");
        Renderer[] groundRenderers = grassParent.GetComponentsInChildren<Renderer>();

        Bounds combinedBounds = groundRenderers[0].bounds;
        for (int i = 1; i < groundRenderers.Length; i++)
        {
            combinedBounds.Encapsulate(groundRenderers[i].bounds);
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        Transform player = playerObj != null ? playerObj.transform : null;

        for (int i = 0; i < npcCount; i++)
        {
            Vector3 randomPos = GetRandomNavMeshPosition(combinedBounds, 5f);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                var npc = Instantiate(npcPrefab, hit.position, Quaternion.identity);
                var shooter = npc.GetComponent<NPCShooter>();
                if (shooter != null && player != null)
                    shooter.SetPlayer(player);

                if (npcAudioClip != null)
                {
                    var audioSource = npc.GetComponent<AudioSource>();
                    if (audioSource == null)
                        audioSource = npc.AddComponent<AudioSource>();

                    var npcAudio = npc.GetComponent<NPCAudio>();
                    if (npcAudio == null)
                        npcAudio = npc.AddComponent<NPCAudio>();

                    npcAudio.audioClip = npcAudioClip;
                    if (player != null)
                        npcAudio.SetPlayer(player);
                    
                    Debug.Log($"Added audio to NPC at position: {npc.transform.position}");
                }
            }
        }
    }

    Vector3 GetRandomNavMeshPosition(Bounds bounds, float searchRadius)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 randomPoint = new Vector3(x, bounds.center.y, z);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
                return hit.position;
        }
        return bounds.center; 
    }
}