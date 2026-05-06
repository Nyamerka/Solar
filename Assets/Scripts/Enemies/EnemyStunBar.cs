using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyStunBar : MonoBehaviour
{
    [SerializeField] private float barWidth = 0.8f;
    [SerializeField] private float barHeight = 0.08f;
    [SerializeField] private float yOffset = 2.2f;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color fillColor = new Color(1f, 0.85f, 0.2f, 0.9f);

    private EnemyAI ai;
    private Transform barRoot;
    private Transform fillTransform;
    private Renderer bgRenderer;
    private Renderer fillRenderer;

    private float stunDuration;
    private bool wasStunned;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        CreateBar();
        barRoot.gameObject.SetActive(false);
    }

    private void CreateBar()
    {
        barRoot = new GameObject("StunBar").transform;
        barRoot.SetParent(transform);
        barRoot.localPosition = new Vector3(0, yOffset, 0);

        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "StunBarBG";
        bg.transform.SetParent(barRoot);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        Destroy(bg.GetComponent<Collider>());

        bgRenderer = bg.GetComponent<Renderer>();
        bgRenderer.material = CreateBarMaterial(backgroundColor);
        bgRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        bgRenderer.receiveShadows = false;

        var fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "StunBarFill";
        fill.transform.SetParent(barRoot);
        fill.transform.localPosition = new Vector3(0, 0, -0.001f);
        fill.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        Destroy(fill.GetComponent<Collider>());

        fillTransform = fill.transform;
        fillRenderer = fill.GetComponent<Renderer>();
        fillRenderer.material = CreateBarMaterial(fillColor);
        fillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fillRenderer.receiveShadows = false;
    }

    private Material CreateBarMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                  ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    private void LateUpdate()
    {
        bool isStunned = ai.CurrentState == EnemyState.Stunned;

        if (isStunned && !wasStunned)
        {
            stunDuration = ai.StunDurationTotal;
            barRoot.gameObject.SetActive(true);
        }

        if (isStunned)
        {
            float ratio = stunDuration > 0f
                ? Mathf.Clamp01(ai.StunTimeRemaining / stunDuration)
                : 0f;
            fillTransform.localScale = new Vector3(barWidth * ratio, barHeight, 1f);
            float xOffset = -barWidth * (1f - ratio) * 0.5f;
            fillTransform.localPosition = new Vector3(xOffset, 0, -0.001f);

            var cam = Camera.main;
            if (cam != null)
                barRoot.rotation = cam.transform.rotation;
        }
        else if (wasStunned)
        {
            barRoot.gameObject.SetActive(false);
        }

        wasStunned = isStunned;
    }
}
