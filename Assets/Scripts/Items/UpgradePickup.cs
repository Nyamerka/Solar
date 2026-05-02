using UnityEngine;

public enum UpgradeType
{
    Range,
    Duration
}

public class UpgradePickup : Pickup
{
    [SerializeField] private UpgradeType upgradeType = UpgradeType.Range;
    [SerializeField] private float amount = 1f;

    protected override void OnCollected(GameObject player)
    {
        if (PlayerUpgrades.Instance == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Range:
                PlayerUpgrades.Instance.AddRange(amount);
                break;
            case UpgradeType.Duration:
                PlayerUpgrades.Instance.AddDuration(amount);
                break;
        }
    }
}
