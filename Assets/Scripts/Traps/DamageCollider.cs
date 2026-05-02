using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private bool hasHit;

    public void SetDamage(int d) => damage = d;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        hasHit = true;
        var health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
