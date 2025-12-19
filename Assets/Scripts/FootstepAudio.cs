using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [Tooltip("Clip to play while walking. Ideally a looping footstep track.")]
    public AudioClip walkingClip;

    [Tooltip("Play the clip in a loop while moving. If disabled, plays one-shots at intervals.")]
    public bool useLoopingClip = false;

    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Tooltip("Minimum speed to consider the player as moving.")]
    public float moveThreshold = 0.1f;

    [Tooltip("Seconds between footsteps when not looping.")]
    public float stepIntervalSeconds = 1.0f;

    [Tooltip("Minimum allowed interval between one-shot steps.")]
    public float minStepIntervalSeconds = 0.8f;

    [Tooltip("Global multiplier to lengthen the time between footsteps.")]
    public float globalIntervalScale = 1.25f;

    [Header("Pitch Variation")]
    [Tooltip("Minimum randomized pitch for footsteps.")]
    public float pitchMin = 0.97f;

    [Tooltip("Maximum randomized pitch for footsteps.")]
    public float pitchMax = 1.03f;

    [Tooltip("Seconds between pitch target changes while looping.")]
    public float pitchJitterIntervalSeconds = 1.2f;

    [Tooltip("How quickly to lerp to the new pitch when looping.")]
    public float pitchLerpSpeed = 4f;

    private AudioSource _source;
    private CharacterController _controller;
    private Vector3 _lastPos;
    private float _stepTimer;
    private float _pitchTimer;
    private float _pitchTarget = 1f;
    private float _currentPitch = 1f;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        _controller = GetComponent<CharacterController>();
        _lastPos = transform.position;

        if (walkingClip != null && useLoopingClip)
        {
            _source.clip = walkingClip;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = volume;
            _source.pitch = 1f;
            _pitchTarget = 1f;
            _currentPitch = 1f;
        }
    }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 1e-5f);
        float speed = (transform.position - _lastPos).magnitude / dt;
        _lastPos = transform.position;

        bool grounded = _controller ? _controller.isGrounded : true;
        bool isMoving = speed > moveThreshold;

        if (!grounded)
        {
            if (_source.isPlaying) _source.Stop();
            _stepTimer = 0f;
            _pitchTimer = 0f;
            return;
        }

        if (walkingClip == null)
            return;

        _source.volume = volume;

        if (useLoopingClip)
        {
            if (isMoving)
            {
                if (!_source.isPlaying)
                {
                    float startPitch = Random.Range(pitchMin, pitchMax);
                    _source.pitch = startPitch;
                    _currentPitch = startPitch;
                    _pitchTarget = startPitch;
                    _pitchTimer = 0f;
                    _source.Play();
                }
                else
                {
                    _pitchTimer += Time.deltaTime;
                    if (_pitchTimer >= pitchJitterIntervalSeconds)
                    {
                        _pitchTarget = Random.Range(pitchMin, pitchMax);
                        _pitchTimer = 0f;
                    }
                    _currentPitch = Mathf.Lerp(_currentPitch, _pitchTarget, Time.deltaTime * pitchLerpSpeed);
                    _source.pitch = _currentPitch;
                }
            }
            else
            {
                if (_source.isPlaying) _source.Stop();
                _pitchTimer = 0f;
            }
        }
        else
        {
            if (isMoving)
            {
                _stepTimer += Time.deltaTime;
                float speedScale = Mathf.Clamp(moveThreshold / speed, 0.85f, 1f);
                float interval = Mathf.Max(minStepIntervalSeconds, stepIntervalSeconds * speedScale * globalIntervalScale);
                if (_stepTimer >= interval)
                {
                    _source.pitch = Random.Range(pitchMin, pitchMax);
                    _source.PlayOneShot(walkingClip, volume);
                    _stepTimer = 0f;
                }
            }
            else
            {
                _stepTimer = 0f;
            }
        }
    }
}
