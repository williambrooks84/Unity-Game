using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject playAgainButton; // assign your Play Again button here (optional)

    // Call this from your Play Again button's OnClick event
    public void OnPlayAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public TMP_Text score; 
    public int scoreValue = 0;

    [Header("Player Health UI")]
    public TMP_Text playerHealthText;         // shows "HP: current/max"
    public TMP_Text wastedText;               // assign a TMP text for "Wasted" message
    public Health playerHealthRef;            // assign your Player's Health here (or leave null and tag Player)

    // cached subscription flag
    bool _subscribedToPlayerHealth = false;

    void Start()
    {
        UpdateScoreText();
        TryBindPlayerHealth();
        UpdatePlayerHealthUIImmediate();
        if (wastedText != null) wastedText.gameObject.SetActive(false);
        if (playAgainButton != null) playAgainButton.SetActive(false);
    }

    void OnDisable()
    {
        UnsubscribePlayerHealth();
    }

    public void SetScore(int value)
    {
        scoreValue = value;
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        scoreValue += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (score != null)
            score.text = "Score: " + scoreValue;
    }

    // --- Player Health binding & updates ---
    void TryBindPlayerHealth()
    {
        if (playerHealthRef == null)
        {
            var playerGO = SafeFindPlayer();
            if (playerGO != null)
            {
                playerHealthRef = playerGO.GetComponent<Health>();
            }
        }

        if (playerHealthRef != null && !_subscribedToPlayerHealth)
        {
            playerHealthRef.onHealthChanged?.AddListener(OnPlayerHealthChanged);
            _subscribedToPlayerHealth = true;
        }
    }

    void UnsubscribePlayerHealth()
    {
        if (_subscribedToPlayerHealth && playerHealthRef != null)
        {
            playerHealthRef.onHealthChanged?.RemoveListener(OnPlayerHealthChanged);
        }
        _subscribedToPlayerHealth = false;
    }

    void OnPlayerHealthChanged(int current, int max)
    {
        UpdatePlayerHealthUI(current, max);
        bool isDead = current <= 0;
        if (wastedText != null)
        {
            wastedText.text = isDead ? "Wasted" : "";
            wastedText.gameObject.SetActive(isDead);
        }
        if (playAgainButton != null)
        {
            playAgainButton.SetActive(isDead);
        }
    }

    void UpdatePlayerHealthUIImmediate()
    {
        if (playerHealthRef != null)
        {
            UpdatePlayerHealthUI(playerHealthRef.currentHealth, playerHealthRef.maxHealth);
        }
        else
        {
            // clear UI if not bound
            if (playerHealthText != null) playerHealthText.text = "";
        }
    }

    void UpdatePlayerHealthUI(int current, int max)
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"Health: {current}/{max}";
        }
    }

    GameObject SafeFindPlayer()
    {
        try
        {
            return GameObject.FindGameObjectWithTag("Player");
        }
        catch
        {
            return null;
        }
    }
}
