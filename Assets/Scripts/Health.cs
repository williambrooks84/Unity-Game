using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class HealthChangedEvent : UnityEvent<int,int> {} 
[System.Serializable] public class IntEvent : UnityEvent<int> {} 

public class Health : MonoBehaviour
{
    [Header("UI")]
    public GameObject crosshairUI;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    // Track who dealt damage for kill counting
    private GameObject lastDamageSource = null;

    [Header("Events (optional)")]
    public HealthChangedEvent onHealthChanged;
    public IntEvent onDamaged;
    public UnityEvent onDeath;

    void Awake()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;
    }

    void Update()
    {
        if (gameObject.CompareTag("Player") && !IsDead && transform.position.y < -5f)
        {
            TakeDamage(currentHealth); 
        }
    }

    public bool IsDead => currentHealth <= 0;

    public void TakeDamage(int amount, GameObject damageSource = null)
    {
        if (amount <= 0 || IsDead) return;

        lastDamageSource = damageSource;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        onDamaged?.Invoke(amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        onDeath?.Invoke();

        if (crosshairUI != null)
            crosshairUI.SetActive(false);

        // Only count kill if NPC was killed by player projectile
        if (!gameObject.CompareTag("Player") && lastDamageSource != null && lastDamageSource.CompareTag("Player"))
        {
            Menu.AddKill();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.SetActive(false);
    }
}
