using UnityEngine;

/// <summary>
/// Plays a shot sound effect at a given position.
/// Attach this to an empty GameObject or call it from anywhere.
/// </summary>
public class ShotSoundEffect : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip shotSound;
    
    [Range(0f, 1f)]
    public float volume = 0.8f;
    
    [Range(0.5f, 2f)]
    public float pitchVariation = 0.1f; // adds slight pitch variation to each shot

    private static ShotSoundEffect instance;

    void Awake()
    {
        // Singleton pattern - keep one instance throughout the game
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void PlayShotAtPosition(Vector3 position, AudioClip clip = null)
    {
        if (instance == null)
        {
            Debug.LogWarning("ShotSoundEffect: No instance found. Add ShotSoundEffect component to a GameObject.");
            return;
        }

        instance.PlayShot(position, clip);
    }

    private void PlayShot(Vector3 position, AudioClip clip)
    {
        if (shotSound == null && clip == null)
        {
            Debug.LogWarning("ShotSoundEffect: No audio clip assigned!");
            return;
        }

        // Create a temporary GameObject for the sound
        GameObject tempAudio = new GameObject("ShotSound");
        tempAudio.transform.position = position;

        // Add AudioSource
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = clip != null ? clip : shotSound;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f; // 3D audio at shot position
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 30f;

        // Add slight pitch variation
        if (pitchVariation > 0)
        {
            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        // Play the sound
        audioSource.Play();

        // Destroy after sound finishes
        Destroy(tempAudio, audioSource.clip.length);
    }
}
