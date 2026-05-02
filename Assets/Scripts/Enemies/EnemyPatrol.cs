using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointThreshold = 0.5f;

    private int currentIndex;

    public void UpdatePatrol(NavMeshAgent agent)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= waypointThreshold)
            currentIndex = (currentIndex + 1) % waypoints.Length;
    }
}
