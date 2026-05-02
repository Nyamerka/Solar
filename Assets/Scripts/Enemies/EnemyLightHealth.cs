using UnityEngine;
using System;

public class EnemyLightHealth : MonoBehaviour
{
    [SerializeField] private float lightHP = 6f;

    public float CurrentLightHP { get; private set; }
    public bool IsDead => CurrentLightHP <= 0f;

    public event Action OnLightDamaged;
    public event Action OnLightDeath;

    private EnemyAI ai;

    private void Awake()
    {
        CurrentLightHP = lightHP;
        ai = GetComponentInParent<EnemyAI>();
    }

    public void OnLightHit(float damage, LightAttackType attackType, float stunDuration = 0f)
    {
        if (IsDead) return;

        // Wraith only takes damage when visible
        if (ai != null && ai.Type == EnemyType.Wraith)
        {
            var vis = GetComponent<EnemyLightVisibility>();
            if (vis != null && !vis.IsVisible) return;
        }

        CurrentLightHP -= damage;
        OnLightDamaged?.Invoke();

        if (stunDuration > 0f && ai != null)
            ai.Stun(stunDuration);

        if (attackType == LightAttackType.SustainedBeam && ai != null)
            ai.Stun(0.1f);

        if (CurrentLightHP <= 0f)
        {
            OnLightDeath?.Invoke();
            if (ai != null) ai.Die();
        }
    }
}
