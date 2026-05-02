using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance { get; private set; }

    public float FlashRangeBonus { get; private set; }
    public float FlashDurationBonus { get; private set; }

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

    public void AddRange(float amount)
    {
        FlashRangeBonus += amount;
    }

    public void AddDuration(float amount)
    {
        FlashDurationBonus += amount;
    }
}
