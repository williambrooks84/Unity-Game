using UnityEngine;

public class ShotSoundEffect : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip shotSound;
    
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Range(0.5f, 2f)]
    public float pitchVariation = 0.1f;

    private static ShotSoundEffect instance;

    void Awake()
    {
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

        GameObject tempAudio = new GameObject("ShotSound");
        tempAudio.transform.position = position;

        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = clip != null ? clip : shotSound;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 30f;

        if (pitchVariation > 0)
        {
            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        audioSource.Play();

        Destroy(tempAudio, audioSource.clip.length);
    }
}
