using UnityEngine;

public class KeyPickup : Pickup
{
    protected override void OnCollected(GameObject player)
    {
        var inv = player.GetComponent<PlayerInventory>();
        if (inv != null)
            inv.AddKey();
    }
}
