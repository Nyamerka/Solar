using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [SerializeField] private float hearingRange = 10f;
    [SerializeField] private float noiseThreshold = 0.3f;

    public bool CanHearPlayer(Transform player)
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > hearingRange) return false;

        var noise = player.GetComponent<PlayerNoise>();
        if (noise == null) return false;

        return noise.CurrentNoise > noiseThreshold;
    }
}
