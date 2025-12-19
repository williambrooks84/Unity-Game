using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The audio clip to play (e.g., zombie sound)")]
    public AudioClip audioClip;

    [Tooltip("The player/camera transform to calculate distance from")]
    public Transform player;

    [Header("Distance-based Volume")]
    [Tooltip("Distance at which audio starts fading in")]
    public float maxDistance = 20f;

    [Tooltip("Distance at which audio reaches maximum volume")]
    public float minDistance = 2f;

    [Tooltip("Maximum volume when player is close")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Tooltip("Minimum volume when player is far")]
    [Range(0f, 1f)]
    public float minVolume = 0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = maxVolume;
    
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (audioClip != null)
        {
            audioSource.Play();
            Debug.Log($"NPCAudio started on {gameObject.name} at position {transform.position}, SpatialBlend: {audioSource.spatialBlend}");
        }
    }

    void Update()
    {
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}
