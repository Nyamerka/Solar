using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        level1Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(1));
        level2Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(2));
        if (level3Button != null)
            level3Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(3));
        quitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());

        level2Button.interactable = GameManager.Instance.UnlockedLevels >= 2;
        if (level3Button != null)
            level3Button.interactable = GameManager.Instance.UnlockedLevels >= 3;
    }
}
