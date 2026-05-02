using UnityEngine;
using UnityEngine.InputSystem;

public enum DoorRequirement
{
    None,
    Key,
    AllArtifacts
}

public class Door : MonoBehaviour
{
    [SerializeField] private DoorRequirement requirement = DoorRequirement.None;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float openSpeed = 2f;

    private bool isOpen;
    private bool playerInRange;
    private Vector3 closedPos;
    private Vector3 openPos;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        closedPos = transform.position;
        openPos = closedPos + openOffset;
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

    private void Update()
    {
        Vector3 target = isOpen ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || isOpen) return;

        TryOpen();
    }

    public void TryOpen()
    {
        if (isOpen) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var inv = player.GetComponent<PlayerInventory>();

        switch (requirement)
        {
            case DoorRequirement.None:
                Open();
                break;
            case DoorRequirement.Key:
                if (inv != null && inv.HasKey) Open();
                break;
            case DoorRequirement.AllArtifacts:
                if (inv != null && inv.HasAllArtifacts) Open();
                break;
        }
    }

    public void Open()
    {
        isOpen = true;
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
