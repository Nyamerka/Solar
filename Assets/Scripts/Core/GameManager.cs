using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentLevel { get; private set; }
    public int UnlockedLevels
    {
        get => PlayerPrefs.GetInt("UnlockedLevels", 1);
        private set => PlayerPrefs.SetInt("UnlockedLevels", value);
    }

    public event Action OnLevelWon;
    public event Action OnLevelLost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Level") && scene.name.Length >= 7)
        {
            if (int.TryParse(scene.name.Substring(5), out int level))
                CurrentLevel = level;
        }
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLevel(int level)
    {
        Time.timeScale = 1f;
        CurrentLevel = level;
        SceneManager.LoadScene("Level0" + level);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void WinLevel()
    {
        if (CurrentLevel >= UnlockedLevels)
            UnlockedLevels = CurrentLevel + 1;

        PlayerPrefs.Save();
        OnLevelWon?.Invoke();
    }

    public void LoseLevel()
    {
        OnLevelLost?.Invoke();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
