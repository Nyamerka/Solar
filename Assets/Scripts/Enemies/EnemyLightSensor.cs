using UnityEngine;

public class EnemyLightSensor : MonoBehaviour
{
    private EnemyAI ai;
    private Transform playerTransform;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public void OnLightDetected(LightAttackType attackType)
    {
        if (ai == null) return;

        switch (ai.Type)
        {
            case EnemyType.Patrol:
                if (ai.CurrentState == EnemyState.Patrol || ai.CurrentState == EnemyState.Search)
                    ai.AlertToPlayer();
                break;

            case EnemyType.Lurker:
                if (playerTransform != null)
                    ai.Flee(playerTransform.position);
                break;
        }
    }
}
