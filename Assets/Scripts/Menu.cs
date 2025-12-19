using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject playAgainButton; 

    [Header("Kill Counter")]
    public TMP_Text killCountText;   
    public static int killCount = 0; 

    [Header("Player Health UI")]
    public TMP_Text playerHealthText; 
    public TMP_Text wastedText;  
    public Health playerHealthRef; 

    [Header("Victory UI")]
    public TMP_Text victoryText;              

    [Header("Welcome UI")]
    public GameObject welcomeCanvas;         
    public GameObject hudCanvas;
    public Button welcomeStartButton;  
    [Tooltip("If true, clicking anywhere while the welcome UI is shown will start the game (fallback if the button doesn't register).")]
    public bool acceptClickAnywhereToStart = true;
    private PlayerMovementMouse playerMovement;
    private FirstPersonView firstPersonView;
    private GameObject crosshairUI;

    bool _subscribedToPlayerHealth = false;
    static bool _welcomeDisplayed = false;

    void Awake()
    {
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
        UpdateKillCountText();
        TryBindPlayerHealth();
        UpdatePlayerHealthUIImmediate();
        if (wastedText != null) wastedText.gameObject.SetActive(false);
        if (victoryText != null) {
            victoryText.text = "";
            victoryText.gameObject.SetActive(false);
        }
        if (playAgainButton != null) playAgainButton.SetActive(false);

        ShowWelcome();

        if (welcomeStartButton != null)
        {
            welcomeStartButton.onClick.AddListener(StartGameFromWelcome);
        }

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

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Debug.Log("Menu: Created EventSystem for UI input.");
        }

        if (welcomeCanvas != null)
        {
            var cg = welcomeCanvas.GetComponent<UnityEngine.CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
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
    public void ShowVictoryScreen()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (victoryText != null)
        {
            victoryText.text = "Victory!";
            victoryText.gameObject.SetActive(true);
        }
        if (playAgainButton != null)
            playAgainButton.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;
        if (firstPersonView != null)
            firstPersonView.enabled = false;
    }

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

    public void StartGameFromWelcome()
    {
        _welcomeDisplayed = true;
        Debug.Log($"Menu.StartGameFromWelcome() called on GameObject='{gameObject.name}' (id={gameObject.GetInstanceID()})");
        if (welcomeCanvas != null) welcomeCanvas.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        killCount = 0;
        UpdateKillCountText();
        if (playerMovement != null) playerMovement.enabled = true;
        if (firstPersonView != null) firstPersonView.enabled = true;
        Debug.Log("Menu: Start button pressed — game started from welcome.");
    }

    void LateUpdate()
    {
        if (welcomeCanvas != null && welcomeCanvas.activeSelf)
        {
            if (Cursor.lockState != CursorLockMode.None || Cursor.visible == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (acceptClickAnywhereToStart)
            {
                bool primaryClicked = false;
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
        killCount = 0;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerMovement != null)
            playerMovement.enabled = true;
        if (firstPersonView != null)
            firstPersonView.enabled = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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
