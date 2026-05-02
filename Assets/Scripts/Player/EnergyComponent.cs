using UnityEngine;
using System;

public class EnergyComponent : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float passiveRegen = 5f;
    [SerializeField] private float darkRegen = 10f;
    [SerializeField] private float regenCooldown = 1f;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => maxEnergy;
    public bool IsInDarkness { get; private set; }

    public event Action<float> OnEnergyChanged;

    private float lastAttackTime;
    private Flashlight flashlight;

    private void Start()
    {
        CurrentEnergy = maxEnergy;
        flashlight = GetComponentInChildren<Flashlight>();
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }

    private void Update()
    {
        UpdateDarknessState();

        if (Time.time - lastAttackTime < regenCooldown) return;

        float regenRate = IsInDarkness ? darkRegen : passiveRegen;
        if (CurrentEnergy < maxEnergy)
        {
            CurrentEnergy = Mathf.Min(CurrentEnergy + regenRate * Time.deltaTime, maxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy);
        }
    }

    private void UpdateDarknessState()
    {
        bool flashActive = flashlight != null && flashlight.IsLightActive;
        IsInDarkness = !flashActive && Time.time - lastAttackTime > regenCooldown;
    }

    public bool TryConsume(float amount)
    {
        if (CurrentEnergy < 5f) return false;
        if (CurrentEnergy < amount) return false;

        CurrentEnergy -= amount;
        lastAttackTime = Time.time;
        OnEnergyChanged?.Invoke(CurrentEnergy);
        return true;
    }

    public void ConsumeOverTime(float ratePerSecond)
    {
        float cost = ratePerSecond * Time.deltaTime;
        CurrentEnergy = Mathf.Max(0f, CurrentEnergy - cost);
        lastAttackTime = Time.time;
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }

    public void Add(float amount)
    {
        CurrentEnergy = Mathf.Min(CurrentEnergy + amount, maxEnergy);
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }

    public bool HasEnough(float amount) => CurrentEnergy >= amount;
}
