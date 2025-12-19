using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthKit : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 25;
    public bool healToFull = false;

    [Header("Pickup Behavior")]
    public bool oneTimeUse = true;
    public float respawnTime = 20f; 

    [Header("Optional Effects")]
    public ParticleSystem pickupEffect;
    
    [Tooltip("Leave empty to use default Health.wav sound")]
    public AudioClip pickupSound;

    Collider _collider;
    Renderer[] _renderers;
    AudioSource _audio;

    [Header("Visuals")]
    public bool spin = true;
    public Vector3 spinAxis = Vector3.up;
    public bool spinInWorldSpace = true;
    public bool spinAroundCenter = true;
    public float spinSpeed = 45f;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null) _collider.isTrigger = true;
        _renderers = GetComponentsInChildren<Renderer>(true);
        
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryPickup(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        TryPickup(collision.gameObject);
    }

    void Update()
    {
        if (!spin) return;
        if (_renderers == null || _renderers.Length == 0) return;
        bool visible = false;
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r != null && r.enabled)
            {
                visible = true;
                break;
            }
        }
        if (visible)
        {
            float angle = spinSpeed * Time.deltaTime;
            Vector3 axisWorld = spinInWorldSpace ? Vector3.up : transform.TransformDirection(spinAxis.normalized);

            if (spinAroundCenter && _renderers != null && _renderers.Length > 0)
            {
                Bounds b = _renderers[0].bounds;
                for (int i = 1; i < _renderers.Length; i++) if (_renderers[i] != null) b.Encapsulate(_renderers[i].bounds);
                Vector3 center = b.center;

                for (int i = 0; i < _renderers.Length; i++)
                {
                    var r = _renderers[i];
                    if (r == null) continue;

                    r.transform.RotateAround(center, axisWorld, angle);
                }
            }
            else
            {
                if (spinInWorldSpace)
                    transform.Rotate(Vector3.up * angle, Space.World);
                else
                    transform.Rotate(spinAxis.normalized * angle, Space.Self);
            }
        }
    }

    void TryPickup(GameObject otherObj)
    {
        if (otherObj == null) return;

        Health health = otherObj.GetComponent<Health>();
        if (health == null) health = otherObj.GetComponentInParent<Health>();
        if (health == null) health = otherObj.GetComponentInChildren<Health>(true);

        if (health == null)
        {
            return;
        }

        bool isPlayer = false;
        Transform check = health.transform;
        while (check != null)
        {
            if (check.gameObject.CompareTag("Player"))
            {
                isPlayer = true;
                break;
            }
            if (check.parent == null) break;
            check = check.parent;
        }

        if (!isPlayer) return;

        if (health.IsDead)
        {
            return;
        }

        

        if (healToFull)
            health.ResetHealth();
        else
            health.Heal(healAmount);

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        if (pickupSound != null && _audio != null)
        {
            _audio.PlayOneShot(pickupSound);
        }

        if (oneTimeUse)
        {
            if (respawnTime <= 0f) respawnTime = 120f;
            StartCoroutine(ConsumeRoutine());
        }
    }

    IEnumerator ConsumeRoutine()
    {
        if (_collider != null) _collider.enabled = false;
        foreach (var r in _renderers) if (r != null) r.enabled = false;

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            foreach (var r in _renderers) if (r != null) r.enabled = true;
            if (_collider != null) _collider.enabled = true;
        }
        else
        {

            Destroy(gameObject);
        }
    }
}
