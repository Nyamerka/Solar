using UnityEngine;

public class EnemyLightVisibility : MonoBehaviour
{
    [SerializeField] private float fadeTime = 1.5f;

    public bool IsVisible { get; private set; }

    private Renderer[] renderers;
    private float revealTimer;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        SetVisibility(false);
    }

    private void Update()
    {
        if (revealTimer > 0)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer <= 0)
                SetVisibility(false);
        }
    }

    public void Reveal()
    {
        revealTimer = fadeTime;
        if (!IsVisible)
            SetVisibility(true);
    }

    private void SetVisibility(bool visible)
    {
        IsVisible = visible;
        foreach (var r in renderers)
            r.enabled = visible;
    }
}
