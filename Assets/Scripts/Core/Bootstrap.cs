using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (GameManager.Instance != null) return;

        var managers = new GameObject("[Solar Managers]");
        managers.AddComponent<GameManager>();
        managers.AddComponent<PlayerUpgrades>();
    }
}
