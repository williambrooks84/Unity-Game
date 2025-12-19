using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class HealthUI : MonoBehaviour
{
    [Tooltip("Optional: assign the player's Health. If empty the script will find the GameObject tagged 'Player'.")]
    public Health playerHealth;

    [Header("UI Elements")]
    public Image plusIcon;  
    public Image fillOverlay; 
    public TMP_Text valueText; 

    Color _fillColor;

    void Start()
    {
        if (playerHealth == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerHealth = playerGO.GetComponent<Health>();
        }

        if (fillOverlay != null) _fillColor = fillOverlay.color;

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged?.AddListener(UpdateHealth);
            UpdateHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        else
        {
            UpdateVisuals(0, 1);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged?.RemoveListener(UpdateHealth);
    }

    void UpdateHealth(int current, int max)
    {
        UpdateVisuals(current, max);
    }

    void UpdateVisuals(int current, int max)
    {
        if (valueText != null) valueText.text = current.ToString();

        if (fillOverlay != null)
        {
            float t = (max > 0) ? Mathf.Clamp01(current / (float)max) : 0f; 
            Color c = _fillColor;
            c.a = t; 
            fillOverlay.color = c;
        }
    }
}
