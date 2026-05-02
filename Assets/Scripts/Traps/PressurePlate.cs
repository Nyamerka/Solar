using UnityEngine;
using System;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private bool oneShot = true;

    public event Action OnPressed;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        OnPressed?.Invoke();
    }
}
