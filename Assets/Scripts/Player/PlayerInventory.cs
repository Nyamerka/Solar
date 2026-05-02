using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int artifactsRequired = 3;

    public int ArtifactsCollected { get; private set; }
    public int ArtifactsRequired => artifactsRequired;
    public bool HasKey { get; private set; }
    public bool HasAllArtifacts => ArtifactsCollected >= artifactsRequired;

    public event Action OnArtifactCollected;
    public event Action OnKeyCollected;

    public void AddArtifact()
    {
        ArtifactsCollected++;
        OnArtifactCollected?.Invoke();
    }

    public void AddKey()
    {
        HasKey = true;
        OnKeyCollected?.Invoke();
    }
}
