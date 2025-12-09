using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthKit : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("Amount of health restored (ignored if HealToFull is enabled)")]
    public int healAmount = 25;
    [Tooltip("If true, this kit will restore the player's health to full instead of adding an amount")]
    public bool healToFull = false;

    [Header("Pickup Behavior")]
    [Tooltip("If true the kit is consumed on pickup and will be destroyed/disabled")]
    public bool oneTimeUse = true;
    [Tooltip("If oneTimeUse is true the kit will reappear after this many seconds")]
    public float respawnTime = 20f; 

    [Header("Optional Effects")]
    public ParticleSystem pickupEffect;
    public AudioClip pickupSound;

    Collider _collider;
    Renderer[] _renderers;
    AudioSource _audio;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null) _collider.isTrigger = true;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _audio = GetComponent<AudioSource>();
        if (_audio == null && pickupSound != null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
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
