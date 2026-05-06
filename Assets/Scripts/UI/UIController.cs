using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private Color heartActive = Color.red;
    [SerializeField] private Color heartEmpty = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private TextMeshProUGUI heartsText;

    [Header("Energy")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private Image energyFill;

    [Header("Artifacts")]
    [SerializeField] private TextMeshProUGUI artifactText;

    [Header("Burst Cooldown")]
    [SerializeField] private Image burstCooldownIcon;

    [Header("Upgrades")]
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private TextMeshProUGUI upgradeRangeText;
    [SerializeField] private TextMeshProUGUI upgradeDurationText;
    [SerializeField] private Image upgradeRangeIcon;
    [SerializeField] private Image upgradeDurationIcon;

    [Header("Overlays")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button winNextLevelButton;
    [SerializeField] private Button winMenuButton;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseMenuButton;

    private PlayerHealth playerHealth;
    private EnergyComponent energy;
    private PlayerInventory inventory;
    private Flashlight flashlight;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        playerHealth = player.GetComponent<PlayerHealth>();
        energy = player.GetComponent<EnergyComponent>();
        inventory = player.GetComponent<PlayerInventory>();
        flashlight = player.GetComponentInChildren<Flashlight>();

        if (playerHealth != null)
        {
            playerHealth.OnDamaged += UpdateHearts;
            playerHealth.OnDeath += ShowLoseScreen;
        }
        if (energy != null)
            energy.OnEnergyChanged += UpdateEnergy;
        if (inventory != null)
            inventory.OnArtifactCollected += UpdateArtifacts;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelWon += ShowWinScreen;
            GameManager.Instance.OnLevelLost += ShowLoseScreen;
        }

        SetupButtons();
        UpdateHearts();
        UpdateEnergy(energy != null ? energy.CurrentEnergy : 0);
        UpdateArtifacts();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    private void Update()
    {
        UpdateBurstCooldown();
        UpdateUpgrades();
    }

    private void SetupButtons()
    {
        if (winPanel != null)
        {
            foreach (var btn in winPanel.GetComponentsInChildren<Button>(true))
            {
                string n = btn.name.ToLower();
                if (n.Contains("следующ") || n.Contains("next"))
                {
                    winNextLevelButton = btn;
                    btn.onClick.AddListener(() =>
                    {
                        Time.timeScale = 1f;
                        int next = GameManager.Instance.CurrentLevel + 1;
                        if (next <= 3) GameManager.Instance.LoadLevel(next);
                        else GameManager.Instance.LoadMenu();
                    });
                    if (GameManager.Instance != null && GameManager.Instance.CurrentLevel >= 3)
                        btn.gameObject.SetActive(false);
                }
                else if (n.Contains("меню") || n.Contains("menu"))
                {
                    btn.onClick.AddListener(() => { Time.timeScale = 1f; GameManager.Instance.LoadMenu(); });
                }
            }
        }

        if (losePanel != null)
        {
            foreach (var btn in losePanel.GetComponentsInChildren<Button>(true))
            {
                string n = btn.name.ToLower();
                if (n.Contains("заново") || n.Contains("restart"))
                    btn.onClick.AddListener(() => { Time.timeScale = 1f; GameManager.Instance.RestartLevel(); });
                else if (n.Contains("меню") || n.Contains("menu"))
                    btn.onClick.AddListener(() => { Time.timeScale = 1f; GameManager.Instance.LoadMenu(); });
            }
        }
    }

    private void UpdateHearts()
    {
        if (playerHealth == null) return;

        if (heartsText != null)
        {
            string s = "";
            for (int i = 0; i < 3; i++)
                s += i < playerHealth.CurrentHP
                    ? "<color=#FF4444>\u2665</color> "
                    : "<color=#555555>\u2665</color> ";
            heartsText.text = s.TrimEnd();
            return;
        }

        if (heartIcons == null) return;
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].color = i < playerHealth.CurrentHP ? heartActive : heartEmpty;
    }

    private void UpdateEnergy(float current)
    {
        if (energyBar == null || energy == null) return;
        energyBar.value = current / energy.MaxEnergy;
        if (energyFill != null)
            energyFill.color = current < 20f ? Color.red : Color.cyan;
    }

    private void UpdateArtifacts()
    {
        if (artifactText == null || inventory == null) return;
        artifactText.text = $"{inventory.ArtifactsCollected} / {inventory.ArtifactsRequired}";
    }

    private void UpdateBurstCooldown()
    {
        if (burstCooldownIcon == null || flashlight == null) return;
        burstCooldownIcon.fillAmount = flashlight.BurstCooldownNormalized;
    }

    private void UpdateUpgrades()
    {
        if (PlayerUpgrades.Instance == null) return;

        float rangeBonus = PlayerUpgrades.Instance.FlashRangeBonus;
        float durationBonus = PlayerUpgrades.Instance.FlashDurationBonus;

        bool hasAny = rangeBonus > 0f || durationBonus > 0f;
        if (upgradesPanel != null && upgradesPanel.activeSelf != hasAny)
            upgradesPanel.SetActive(hasAny);

        if (upgradeRangeText != null)
        {
            upgradeRangeText.text = $"+{rangeBonus:0.#}";
            upgradeRangeText.gameObject.SetActive(rangeBonus > 0f);
        }
        if (upgradeRangeIcon != null)
            upgradeRangeIcon.gameObject.SetActive(rangeBonus > 0f);

        if (upgradeDurationText != null)
        {
            upgradeDurationText.text = $"+{durationBonus:0.#}с";
            upgradeDurationText.gameObject.SetActive(durationBonus > 0f);
        }
        if (upgradeDurationIcon != null)
            upgradeDurationIcon.gameObject.SetActive(durationBonus > 0f);
    }

    private void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void ShowLoseScreen()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= UpdateHearts;
            playerHealth.OnDeath -= ShowLoseScreen;
        }
        if (energy != null)
            energy.OnEnergyChanged -= UpdateEnergy;
        if (inventory != null)
            inventory.OnArtifactCollected -= UpdateArtifacts;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelWon -= ShowWinScreen;
            GameManager.Instance.OnLevelLost -= ShowLoseScreen;
        }
    }
}
