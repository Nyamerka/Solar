using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerNoise : MonoBehaviour
{
    [SerializeField] private float noiseSmoothing = 5f;

    public float CurrentNoise { get; private set; }

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        float targetNoise = Mathf.Clamp01(speed / 5f);
        CurrentNoise = Mathf.Lerp(CurrentNoise, targetNoise, noiseSmoothing * Time.deltaTime);
    }
}
