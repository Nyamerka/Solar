using UnityEngine;

public class LightEssence : Pickup
{
    [SerializeField] private float energyAmount = 25f;
    [SerializeField] private float lifetime = 10f;

    protected override void Start()
    {
        base.Start();
        Destroy(gameObject, lifetime);
    }

    protected override void OnCollected(GameObject player)
    {
        var energy = player.GetComponent<EnergyComponent>();
        if (energy != null)
            energy.Add(energyAmount);
    }
}
