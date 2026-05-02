using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Flashlight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light spotLight;
    [SerializeField] private Light burstLight;
    [SerializeField] private EnergyComponent energy;

    [Header("Quick Flash")]
    [SerializeField] private float flashDuration = 0.6f;
    [SerializeField] private float flashCost = 20f;
    [SerializeField] private float flashDamage = 1f;
    [SerializeField] private float flashStunDuration = 1f;

    [Header("Sustained Beam")]
    [SerializeField] private float beamCostPerSecond = 15f;
    [SerializeField] private float beamDamagePerSecond = 2f;

    [Header("Overcharge Burst")]
    [SerializeField] private float burstCost = 50f;
    [SerializeField] private float burstRadius = 6f;
    [SerializeField] private float burstDamage = 8f;
    [SerializeField] private float burstStunDuration = 3f;
    [SerializeField] private float burstCooldown = 4f;

    [Header("Detection")]
    [SerializeField] private float detectionAngle = 45f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;

    public float BurstCooldownNormalized =>
        burstCooldownTimer > 0 ? burstCooldownTimer / burstCooldown : 0f;

    public bool IsLightActive => (spotLight != null && spotLight.enabled)
                              || (burstLight != null && burstLight.enabled);

    private PlayerInputActions inputActions;
    private bool isBeamActive;
    private bool isFlashing;
    private float burstCooldownTimer;
    private float baseRange;
    private float baseDuration;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (energy == null)
            energy = GetComponentInParent<EnergyComponent>();

        if (spotLight == null)
            spotLight = GetComponent<Light>();

        if (burstLight == null)
        {
            var parent = transform.parent;
            if (parent != null)
            {
                var bl = parent.Find("BurstLight");
                if (bl != null) burstLight = bl.GetComponent<Light>();
            }
        }

        if (spotLight != null) spotLight.enabled = false;
        if (burstLight != null) burstLight.enabled = false;

        if (enemyLayer == 0)
        {
            int idx = LayerMask.NameToLayer("Enemy");
            enemyLayer = idx >= 0 ? (1 << idx) : (1 << 8);
        }
        if (wallLayer == 0)
        {
            int idx = LayerMask.NameToLayer("Wall");
            wallLayer = idx >= 0 ? (1 << idx) : (1 << 9);
        }

        baseRange = detectionRange;
        baseDuration = flashDuration;
    }

    private void OnEnable()
    {
        inputActions.Player.Flash.started += OnFlashStarted;
        inputActions.Player.Flash.canceled += OnFlashCanceled;
        inputActions.Player.Burst.performed += OnBurstPerformed;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Flash.started -= OnFlashStarted;
        inputActions.Player.Flash.canceled -= OnFlashCanceled;
        inputActions.Player.Burst.performed -= OnBurstPerformed;
        inputActions.Player.Disable();
    }

    private void Update()
    {
        ApplyUpgrades();

        if (burstCooldownTimer > 0)
            burstCooldownTimer -= Time.deltaTime;

        if (isBeamActive && !isFlashing)
            BeamUpdate();
    }

    private void ApplyUpgrades()
    {
        if (PlayerUpgrades.Instance == null) return;
        detectionRange = baseRange + PlayerUpgrades.Instance.FlashRangeBonus;
        flashDuration = baseDuration + PlayerUpgrades.Instance.FlashDurationBonus;

        if (spotLight != null)
            spotLight.range = detectionRange;
    }

    private void OnFlashStarted(InputAction.CallbackContext ctx)
    {
        if (energy == null) return;
        if (!energy.HasEnough(5f)) return;

        isBeamActive = true;

        if (!isFlashing)
            StartCoroutine(QuickFlashCoroutine());
    }

    private void OnFlashCanceled(InputAction.CallbackContext ctx)
    {
        isBeamActive = false;
        if (spotLight != null) spotLight.enabled = false;
    }

    private IEnumerator QuickFlashCoroutine()
    {
        isFlashing = true;

        if (!energy.TryConsume(flashCost))
        {
            isFlashing = false;
            yield break;
        }

        AimAtNearestEnemy();

        if (spotLight != null) spotLight.enabled = true;

        DealDamageInCone(flashDamage, LightAttackType.QuickFlash);

        yield return new WaitForSeconds(flashDuration);

        if (!isBeamActive)
        {
            if (spotLight != null) spotLight.enabled = false;
        }

        isFlashing = false;
    }

    private void AimAtNearestEnemy()
    {
        Transform root = transform.root;
        Vector3 origin = root.position + Vector3.up * 0.5f;
        var enemies = FindEnemiesInRange(origin, detectionRange);
        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (var enemy in enemies)
        {
            Vector3 dir = enemy.transform.position - origin;
            float dist = dir.magnitude;
            if (dist < minDist)
            {
                if (!Physics.Raycast(origin, dir.normalized, dist, wallLayer))
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }
        }

        if (nearest != null)
        {
            Vector3 lookDir = nearest.position - root.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                root.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void BeamUpdate()
    {
        if (energy == null || !energy.HasEnough(1f))
        {
            isBeamActive = false;
            if (spotLight != null) spotLight.enabled = false;
            return;
        }

        if (spotLight != null) spotLight.enabled = true;

        energy.ConsumeOverTime(beamCostPerSecond);
        DealDamageInCone(beamDamagePerSecond * Time.deltaTime, LightAttackType.SustainedBeam);
        CheckHiddenPassagesInCone();
    }

    private void CheckHiddenPassagesInCone()
    {
        Collider[] allHits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in allHits)
        {
            var passage = hit.GetComponent<HiddenPassage>();
            if (passage == null) continue;

            Vector3 dir = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > detectionAngle * 0.5f) continue;

            passage.OnLightExposure(Time.deltaTime);
        }
    }

    private void OnBurstPerformed(InputAction.CallbackContext ctx)
    {
        if (energy == null) return;
        if (burstCooldownTimer > 0) return;
        if (!energy.TryConsume(burstCost)) return;

        burstCooldownTimer = burstCooldown;
        StartCoroutine(BurstCoroutine());
    }

    private IEnumerator BurstCoroutine()
    {
        if (burstLight != null)
        {
            burstLight.enabled = true;
            burstLight.range = burstRadius;
        }

        DealDamageInRadius(burstDamage, burstRadius);

        RevealHiddenPassages(burstRadius);

        yield return new WaitForSeconds(0.3f);

        if (burstLight != null) burstLight.enabled = false;
    }

    private void DealDamageInCone(float damage, LightAttackType attackType)
    {
        Vector3 origin = transform.root.position + Vector3.up * 0.5f;
        foreach (var enemy in FindEnemiesInRange(origin, detectionRange))
        {
            Vector3 dirToTarget = (enemy.transform.position - origin).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > detectionAngle * 0.5f) continue;

            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (Physics.Raycast(origin, dirToTarget, dist, wallLayer)) continue;

            float stunDur = attackType == LightAttackType.QuickFlash ? flashStunDuration : 0f;
            enemy.OnLightHit(damage, attackType, stunDur);

            var visibility = enemy.GetComponent<EnemyLightVisibility>();
            if (visibility != null)
                visibility.Reveal();

            var lightSensor = enemy.GetComponent<EnemyLightSensor>();
            if (lightSensor != null)
                lightSensor.OnLightDetected(attackType);
        }
    }

    private void DealDamageInRadius(float damage, float radius)
    {
        Vector3 origin = transform.root.position + Vector3.up * 0.5f;
        foreach (var enemy in FindEnemiesInRange(origin, radius))
        {
            Vector3 dir = (enemy.transform.position - origin).normalized;
            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (Physics.Raycast(origin, dir, dist, wallLayer)) continue;

            enemy.OnLightHit(damage, LightAttackType.OverchargeBurst, burstStunDuration);

            var visibility = enemy.GetComponent<EnemyLightVisibility>();
            if (visibility != null)
                visibility.Reveal();

            var lightSensor = enemy.GetComponent<EnemyLightSensor>();
            if (lightSensor != null)
                lightSensor.OnLightDetected(LightAttackType.OverchargeBurst);
        }
    }

    private static List<EnemyLightHealth> FindEnemiesInRange(Vector3 origin, float range)
    {
        var result = new List<EnemyLightHealth>();
        var allEnemies = Object.FindObjectsOfType<EnemyLightHealth>();
        float rangeSq = range * range;
        foreach (var e in allEnemies)
        {
            if (e.IsDead) continue;
            if ((e.transform.position - origin).sqrMagnitude <= rangeSq)
                result.Add(e);
        }
        return result;
    }

    private void RevealHiddenPassages(float radius)
    {
        Collider[] allHits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in allHits)
        {
            var passage = hit.GetComponent<HiddenPassage>();
            if (passage != null)
                passage.Reveal();
        }
    }
}

public enum LightAttackType
{
    QuickFlash,
    SustainedBeam,
    OverchargeBurst
}
