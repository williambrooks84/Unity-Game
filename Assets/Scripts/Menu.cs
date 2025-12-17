using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    [Header("Welcome UI")]
    public GameObject welcomeCanvas;          // assign the welcome panel/canvas here
    public GameObject hudCanvas;              // assign the main HUD canvas (so we can hide it while welcome shows)
    public Button welcomeStartButton;         // optional: drag the Start button here to auto-wire
    [Tooltip("If true, clicking anywhere while the welcome UI is shown will start the game (fallback if the button doesn't register).")]
    public bool acceptClickAnywhereToStart = true;
    // References for disabling aiming/crosshair
    private PlayerMovementMouse playerMovement;
    private FirstPersonView firstPersonView;
    private GameObject crosshairUI;

    // cached subscription flag
    bool _subscribedToPlayerHealth = false;
    // Prevent welcome UI from being shown multiple times across duplicated Menu instances
    static bool _welcomeDisplayed = false;

    void Awake()
    {
        // Prevent duplicate Menu components from fighting each other.
        var others = FindObjectsOfType<Menu>();
        foreach (var m in others)
        {
            if (m != this)
            {
                Debug.LogWarning($"Menu.Awake: Another Menu instance found on '{m.gameObject.name}' (id={m.gameObject.GetInstanceID()}). Disabling this one ('{gameObject.name}').");
                this.enabled = false;
                return;
            }
        }
    }

    void Start()
    {
        Debug.Log($"Menu.Start() on GameObject='{gameObject.name}' (id={gameObject.GetInstanceID()})");
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
        // Show welcome panel (if assigned) at game start
        ShowWelcome();

        // auto-wire the welcome Start button if assigned
        if (welcomeStartButton != null)
        {
            welcomeStartButton.onClick.AddListener(StartGameFromWelcome);
        }

        // if no Start button assigned, try to find one under the welcome canvas
        if (welcomeStartButton == null && welcomeCanvas != null)
        {
            var foundBtn = welcomeCanvas.GetComponentInChildren<Button>(true);
            if (foundBtn != null)
            {
                welcomeStartButton = foundBtn;
                welcomeStartButton.onClick.AddListener(StartGameFromWelcome);
                Debug.Log("Menu: Auto-wired welcome Start button ('" + foundBtn.name + "').");
            }
            else
            {
                Debug.LogWarning("Menu: No Button found under welcomeCanvas to auto-wire StartGameFromWelcome.");
            }
        }

        // Ensure there's an EventSystem so UI can receive clicks
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Debug.Log("Menu: Created EventSystem for UI input.");
        }

        // Ensure welcome canvas (if it has a CanvasGroup) will receive raycasts
        if (welcomeCanvas != null)
        {
            var cg = welcomeCanvas.GetComponent<UnityEngine.CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
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

    // Show the welcome UI and hide HUD; pause the game
    public void ShowWelcome()
    {
        if (_welcomeDisplayed)
        {
            Debug.Log("Menu.ShowWelcome(): welcome already displayed; skipping.");
            return;
        }
        Debug.Log($"Menu.ShowWelcome() called on GameObject='{gameObject.name}' (id={gameObject.GetInstanceID()})");
        if (welcomeCanvas != null) welcomeCanvas.SetActive(true);
        if (hudCanvas != null) hudCanvas.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerMovement != null) playerMovement.enabled = false;
        if (firstPersonView != null) firstPersonView.enabled = false;
        Debug.Log("Menu: Showing welcome UI (cursor unlocked).");
    }

    // Called by the Start button on the welcome UI
    public void StartGameFromWelcome()
    {
        // mark welcome as displayed so other instances or later calls don't re-show it
        _welcomeDisplayed = true;
        Debug.Log($"Menu.StartGameFromWelcome() called on GameObject='{gameObject.name}' (id={gameObject.GetInstanceID()})");
        if (welcomeCanvas != null) welcomeCanvas.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // reset session state
        killCount = 0;
        UpdateKillCountText();
        if (playerMovement != null) playerMovement.enabled = true;
        if (firstPersonView != null) firstPersonView.enabled = true;
        Debug.Log("Menu: Start button pressed — game started from welcome.");
    }

    // Ensure the system cursor stays visible/unlocked while the welcome UI is active.
    // Other scripts may re-lock/hide the cursor in Start/Update; run this in LateUpdate
    // so the welcome screen reliably shows the cursor.
    void LateUpdate()
    {
        if (welcomeCanvas != null && welcomeCanvas.activeSelf)
        {
            if (Cursor.lockState != CursorLockMode.None || Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Fallback: accept any primary mouse click to start the game
            if (acceptClickAnywhereToStart)
            {
                bool primaryClicked = false;
                // Prefer the new Input System if available
                try
                {
                    var mouse = UnityEngine.InputSystem.Mouse.current;
                    if (mouse != null)
                    {
                        primaryClicked = mouse.leftButton.wasPressedThisFrame;
                    }
                }
                catch {}

                if (primaryClicked)
                {
                    Debug.Log("Menu: fallback click detected while welcome active — starting game (InputSystem).");
                    StartGameFromWelcome();
                }
            }
        }
    }


    void OnDisable()
    {
        UnsubscribePlayerHealth();
        if (welcomeStartButton != null)
            welcomeStartButton.onClick.RemoveListener(StartGameFromWelcome);
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
