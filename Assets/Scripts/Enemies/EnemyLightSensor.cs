using UnityEngine;

public class EnemyLightSensor : MonoBehaviour
{
    private EnemyAI ai;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
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

            case EnemyType.Sentry:
                // Sentry doesn't react behaviorally to light, only takes damage
                break;

            case EnemyType.Wraith:
                // Wraith becomes visible (handled by EnemyLightVisibility) — no behavior change from light
                break;

            case EnemyType.Lurker:
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    ai.Flee(playerObj.transform.position);
                break;
        }
    }
}
