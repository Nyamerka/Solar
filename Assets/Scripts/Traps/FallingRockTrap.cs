using UnityEngine;

public class FallingRockTrap : MonoBehaviour
{
    [SerializeField] private PressurePlate plate;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int damage = 1;

    private bool activated;

    private void OnEnable()
    {
        if (plate != null)
            plate.OnPressed += Activate;
    }

    private void OnDisable()
    {
        if (plate != null)
            plate.OnPressed -= Activate;
    }

    private void Activate()
    {
        if (activated) return;
        activated = true;

        if (rockPrefab == null || spawnPoint == null) return;

        var rock = Instantiate(rockPrefab, spawnPoint.position, Quaternion.identity);

        var rb = rock.GetComponent<Rigidbody>();
        if (rb == null) rb = rock.AddComponent<Rigidbody>();
        rb.mass = 5f;

        var dmg = rock.GetComponent<DamageCollider>();
        if (dmg == null) dmg = rock.AddComponent<DamageCollider>();
        dmg.SetDamage(damage);

        Destroy(rock, 5f);
    }
}
