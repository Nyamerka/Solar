using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    [SerializeField] private float invincibilityDuration = 1f;

    public int CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    public event Action OnDamaged;
    public event Action OnDeath;

    private float lastDamageTime;

    private void Start()
    {
        CurrentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (Time.time - lastDamageTime < invincibilityDuration) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        lastDamageTime = Time.time;
        OnDamaged?.Invoke();

        if (CurrentHP <= 0)
        {
            OnDeath?.Invoke();
            GameManager.Instance?.LoseLevel();
        }
    }
}
