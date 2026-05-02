using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        if (pausePanel != null)
        {
            var buttons = pausePanel.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string n = btn.name.ToLower();
                if (n.Contains("продолж") || n.Contains("resume"))
                    btn.onClick.AddListener(Resume);
                else if (n.Contains("меню") || n.Contains("menu"))
                    btn.onClick.AddListener(GoToMenu);
            }
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Pause.performed += OnPause;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Pause.performed -= OnPause;
        inputActions.Player.Disable();
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void GoToMenu()
    {
        Resume();
        GameManager.Instance.LoadMenu();
    }
}
