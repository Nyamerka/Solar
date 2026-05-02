using UnityEngine;
using System;
using TMPro;

public class Pickup : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float labelShowDistance = 3f;
    [SerializeField] private string displayName;

    public event Action OnPickedUp;

    private Vector3 startPos;
    private bool collected;
    private TextMeshPro labelTMP;
    private Transform playerTransform;

    protected virtual void Start()
    {
        startPos = transform.position;

        if (string.IsNullOrEmpty(displayName))
            displayName = InferDisplayName();

        CreateLabel();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private string InferDisplayName()
    {
        if (this is ArtifactPickup) return "Артефакт";
        if (this is EnergyPickup) return "Энергия";
        if (this is KeyPickup) return "Ключ";
        if (this is UpgradePickup) return "Улучшение";
        if (this is LightEssence) return "Эссенция света";
        return "Предмет";
    }

    private void CreateLabel()
    {
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(transform, false);
        labelGO.transform.localPosition = new Vector3(0, 1.5f, 0);

        labelTMP = labelGO.AddComponent<TextMeshPro>();
        labelTMP.text = displayName;
        labelTMP.fontSize = 3f;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color = new Color(1f, 1f, 1f, 0.9f);
        labelTMP.enableWordWrapping = false;

        var rt = labelGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(4f, 1f);

        labelGO.SetActive(false);
    }

    protected virtual void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * yOffset;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (labelTMP == null) return;

        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool show = dist <= labelShowDistance;
        if (labelTMP.gameObject.activeSelf != show)
            labelTMP.gameObject.SetActive(show);

        if (show)
        {
            var cam = Camera.main;
            if (cam != null)
                labelTMP.transform.rotation = Quaternion.LookRotation(
                    labelTMP.transform.position - cam.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        OnCollected(other.gameObject);
        OnPickedUp?.Invoke();
        Destroy(gameObject);
    }

    protected virtual void OnCollected(GameObject player) { }
}
