using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -8f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float orthoSize = 8f;

    private Transform target;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            else return;
        }

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }
}
