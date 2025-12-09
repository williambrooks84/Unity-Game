using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class HealthChangedEvent : UnityEvent<int,int> {} 
[System.Serializable] public class IntEvent : UnityEvent<int> {} 

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

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

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;

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
        
        // Unlock cursor so player can click Play Again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        gameObject.SetActive(false);
    }
}
