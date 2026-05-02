using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private DoorRequirement requirement = DoorRequirement.AllArtifacts;
    [SerializeField] private bool requireKey;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        var inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;

        bool canExit = true;

        if (requirement == DoorRequirement.AllArtifacts && !inv.HasAllArtifacts)
            canExit = false;
        if (requireKey && !inv.HasKey)
            canExit = false;

        if (canExit)
        {
            triggered = true;
            GameManager.Instance?.WinLevel();
        }
    }
}
