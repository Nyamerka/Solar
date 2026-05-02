using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private LayerMask wallMask;

    public float ViewAngle => viewAngle;
    public float ViewDistance => viewDistance;

    private void Awake()
    {
        if (wallMask == 0)
            wallMask = 1 << LayerMask.NameToLayer("Wall");
    }

    public bool CanSeePlayer(Transform player)
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0;
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle * 0.5f) return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDir = (player.position + Vector3.up * 0.5f) - rayOrigin;

        if (Physics.Raycast(rayOrigin, rayDir.normalized, dist, wallMask))
            return false;

        return true;
    }
}
