using UnityEngine;

public class HiddenPassage : MonoBehaviour
{
    [SerializeField] private float revealDuration = 1.5f;

    private Collider wallCollider;
    private Renderer wallRenderer;
    private float lightExposureTime;
    private bool revealed;

    private void Awake()
    {
        wallCollider = GetComponent<Collider>();
        wallRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (revealed) return;

        if (lightExposureTime > 0)
        {
            lightExposureTime -= Time.deltaTime;
        }
    }

    public void OnLightExposure(float duration)
    {
        if (revealed) return;
        lightExposureTime += duration;

        if (lightExposureTime >= revealDuration)
            Reveal();
    }

    public void Reveal()
    {
        if (revealed) return;
        revealed = true;

        if (wallCollider != null) wallCollider.enabled = false;
        if (wallRenderer != null) wallRenderer.enabled = false;
    }
}
