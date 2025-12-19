using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The engine/car sound to play")]
    public AudioClip engineClip;

    [Tooltip("The player/camera transform to calculate distance from")]
    public Transform player;

    [Header("Distance-based Volume")]
    [Tooltip("Distance at which audio starts fading in")]
    public float maxDistance = 25f;

    [Tooltip("Distance at which audio reaches maximum volume")]
    public float minDistance = 1f;

    [Tooltip("Maximum volume when player is close")]
    [Range(0f, 1f)]
    public float maxVolume = 0.9f;

    [Tooltip("Minimum volume when player is far")]
    [Range(0f, 1f)]
    public float minVolume = 0f;

    [Header("Engine Sound")]
    [Tooltip("Pitch variation based on car speed (0 = no variation, 1 = full variation)")]
    public float pitchSpeedFactor = 0.3f;
    
    private Rigidbody carRigidbody;
    private AudioSource audioSource;
    private CarAI carAI;
    private float basePitch = 1f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        carRigidbody = GetComponent<Rigidbody>();
        carAI = GetComponent<CarAI>();
        
        if (audioSource == null)
        {
            Debug.LogError($"CarEngineSound: No AudioSource on {gameObject.name}");
            return;
        }

        if (engineClip == null)
        {
            Debug.LogError($"CarEngineSound: No engine clip assigned on {gameObject.name}");
            return;
        }
        
        // Configure audio source for 3D spatial audio
        audioSource.clip = engineClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound - positioned in space
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = maxVolume; // Start at max volume, will be adjusted by distance
        audioSource.dopplerLevel = 0f; // Disable doppler effect for cleaner sound
        
        // Auto-find player if not set
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Start playing
        audioSource.Play();
        Debug.Log($"CarEngineSound: Started engine sound on {gameObject.name}");
    }

    void Update()
    {
        if (audioSource == null) return;

        // Vary pitch based on car speed
        if (carRigidbody != null && pitchSpeedFactor > 0)
        {
            float speed = carRigidbody.linearVelocity.magnitude;
            float speedFactor = Mathf.Clamp01(speed / 15f); // normalize by typical max speed
            audioSource.pitch = 1f + (speedFactor * pitchSpeedFactor);
        }
        else
        {
            audioSource.pitch = basePitch;
        }

        // Ensure audio is playing
        if (!audioSource.isPlaying && engineClip != null)
        {
            audioSource.Play();
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}
