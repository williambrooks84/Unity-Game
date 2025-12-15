using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject playAgainButton; // assign your Play Again button here (optional)

    // public TMP_Text score; 
    // public int scoreValue = 0;

    [Header("Kill Counter")]
    public TMP_Text killCountText;            // shows "Kills: count"
    public static int killCount = 0;          // static so Health can access it

    [Header("Player Health UI")]
    public TMP_Text playerHealthText;         // shows "HP: current/max"
    public TMP_Text wastedText;               // assign a TMP text for "Wasted" message
    public Health playerHealthRef;            // assign your Player's Health here (or leave null and tag Player)

    [Header("Victory UI")]
    public TMP_Text victoryText;              // assign a TMP text for "Victory" message

    // References for disabling aiming/crosshair
    private PlayerMovementMouse playerMovement;
    private FirstPersonView firstPersonView;
    private GameObject crosshairUI;

    // cached subscription flag
    bool _subscribedToPlayerHealth = false;

    void Start()
    {
        // UpdateScoreText();
        UpdateKillCountText();
        TryBindPlayerHealth();
        UpdatePlayerHealthUIImmediate();
        if (wastedText != null) wastedText.gameObject.SetActive(false);
        if (victoryText != null) {
            victoryText.text = "";
            victoryText.gameObject.SetActive(false);
        }
        if (playAgainButton != null) playAgainButton.SetActive(false);

        // Cache references for disabling on victory
        var playerGO = SafeFindPlayer();
        if (playerGO != null)
        {
            playerMovement = playerGO.GetComponent<PlayerMovementMouse>();
            firstPersonView = FindObjectOfType<FirstPersonView>();
            var health = playerGO.GetComponent<Health>();
            if (health != null)
                crosshairUI = health.crosshairUI;
        }
    }
    // Call this from ControlPointManager when all points are captured
    public void ShowVictoryScreen()
    {
        // Show system cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (victoryText != null)
        {
            victoryText.text = "Victory!";
            victoryText.gameObject.SetActive(true);
        }
        if (playAgainButton != null)
            playAgainButton.SetActive(true);

        // Disable aiming and crosshair
        if (playerMovement != null)
            playerMovement.enabled = false;
        if (firstPersonView != null)
            firstPersonView.enabled = false;
    }


    void OnDisable()
    {
        UnsubscribePlayerHealth();
    }

    public void OnPlayAgainClicked()
    {
        // Reset kill counter for new session
        killCount = 0;
        // Hide system cursor and lock for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Re-enable aiming and crosshair before reload
        if (playerMovement != null)
            playerMovement.enabled = true;
        if (firstPersonView != null)
            firstPersonView.enabled = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // public void SetScore(int value)
    // {
    //     scoreValue = value;
    //     UpdateScoreText();
    // }

    // public void AddScore(int amount)
    // {
    //     scoreValue += amount;
    //     UpdateScoreText();
    // }

    // void UpdateScoreText()
    // {
    //     if (score != null)
    //         score.text = "Score: " + scoreValue;
    // }

    void UpdateKillCountText()
    {
        if (killCountText != null)
            killCountText.text = "Kills: " + killCount;
    }

    public static void AddKill()
    {
        killCount++;
        var menu = FindObjectOfType<Menu>();
        if (menu != null) menu.UpdateKillCountText();
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

        // Show cursor when dead (game over)
        if (isDead)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
