using UnityEngine;

public class ArtifactPickup : Pickup
{
    protected override void OnCollected(GameObject player)
    {
        var inv = player.GetComponent<PlayerInventory>();
        if (inv != null)
            inv.AddArtifact();
    }
}
