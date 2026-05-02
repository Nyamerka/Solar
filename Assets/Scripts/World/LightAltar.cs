using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LightAltar : MonoBehaviour
{
    [SerializeField] private float energyAmount = 50f;
    [SerializeField] private float chargeDuration = 2f;
    [SerializeField] private float cooldownDuration = 30f;
    [SerializeField] private Light altarLight;
    [SerializeField] private Color activeColor = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private Color cooldownColor = new Color(0.1f, 0.1f, 0.15f);

    private bool playerInRange;
    private bool isOnCooldown;
    private bool isCharging;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        SetAltarState(true);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || isOnCooldown || isCharging) return;

        StartCoroutine(ChargeCoroutine());
    }

    private IEnumerator ChargeCoroutine()
    {
        isCharging = true;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { isCharging = false; yield break; }

        var energy = player.GetComponent<EnergyComponent>();
        if (energy == null) { isCharging = false; yield break; }

        float elapsed = 0f;
        float energyPerSecond = energyAmount / chargeDuration;

        while (elapsed < chargeDuration)
        {
            energy.Add(energyPerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
        isOnCooldown = true;
        SetAltarState(false);

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        SetAltarState(true);
    }

    private void SetAltarState(bool active)
    {
        if (altarLight != null)
        {
            altarLight.color = active ? activeColor : cooldownColor;
            altarLight.intensity = active ? 2f : 0.3f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
