using UnityEngine;

public class EnergyPickup : Pickup
{
    [SerializeField] private float energyAmount = 50f;

    protected override void OnCollected(GameObject player)
    {
        var energy = player.GetComponent<EnergyComponent>();
        if (energy != null)
            energy.Add(energyAmount);
    }
}
