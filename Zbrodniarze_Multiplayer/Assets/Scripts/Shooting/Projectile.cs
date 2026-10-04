using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [Header("Basic")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;

    [Header("Bounce")]
    [SerializeField] private int maxBounces = 0;

    [Header("Explosion")]
    [SerializeField] private bool explosionEnabled = false;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 25f;

    [Header("Knockback")]
    [SerializeField] private bool knockbackEnabled = false;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Homing")]
    [SerializeField] private bool homingEnabled = false;
    [SerializeField] private float homingRange = 15f;
    [SerializeField] private float homingStrength = 5f;

    private NetworkObject shooter;
    private Rigidbody rb;

    private int currentBounces;

    public void SetShooter(NetworkObject shooterObject)
    {
        shooter = shooterObject;
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        currentBounces = 0;

        if (IsServer)
        {
            Invoke(nameof(Despawn), lifetime);
        }

        if (rb != null)
        {
            rb.isKinematic = !IsServer;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (homingEnabled)
        {
            HandleHoming();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
            return;

        Health health = collision.gameObject.GetComponentInParent<Health>();

        // Bezpoœrednie obra¿enia
        if (health != null && health.NetworkObject != shooter)
        {
            health.TakeDamage(damage);

            if (knockbackEnabled)
            {
                ApplyKnockback(health.gameObject);
            }
        }

        // Eksplozja
        if (explosionEnabled)
        {
            Explode();
        }

        // Odbicie
        if (currentBounces < maxBounces)
        {
            Bounce(collision);
            return;
        }

        Despawn();
    }

    private void Bounce(Collision collision)
    {
        if (rb == null)
            return;

        currentBounces++;

        Vector3 incomingDirection = rb.linearVelocity.normalized;

        Vector3 reflectedDirection = Vector3.Reflect(
            incomingDirection,
            collision.contacts[0].normal
        );

        float speed = rb.linearVelocity.magnitude;

        rb.linearVelocity = reflectedDirection * speed;

        transform.rotation = Quaternion.LookRotation(
            reflectedDirection
        );
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();

            if (health == null)
                continue;

            // Nie ranimy strzelaj¹cego
            if (health.NetworkObject == shooter)
                continue;

            health.TakeDamage(explosionDamage);

            if (knockbackEnabled)
            {
                ApplyExplosionKnockback(health.gameObject);
            }
        }
    }
    private void ApplyKnockback(GameObject target)
    {
        KnockbackReceiver knockbackReceiver =
            target.GetComponentInParent<KnockbackReceiver>();

        if (knockbackReceiver == null)
            return;

        Vector3 direction =
            target.transform.position - transform.position;

        knockbackReceiver.ApplyKnockback(
            direction,
            knockbackForce
        );
    }

    private void ApplyExplosionKnockback(GameObject target)
    {
        KnockbackReceiver knockbackReceiver =
            target.GetComponentInParent<KnockbackReceiver>();

        if (knockbackReceiver == null)
            return;

        Vector3 direction =
            target.transform.position - transform.position;

        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return;

        float forceMultiplier =
            1f - Mathf.Clamp01(distance / explosionRadius);

        knockbackReceiver.ApplyKnockback(
            direction,
            knockbackForce * forceMultiplier
        );
    }
    private void HandleHoming()
    {
        if (rb == null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            homingRange
        );

        NetworkObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();

            if (health == null)
                continue;

            if (health.NetworkObject == shooter)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                health.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = health.NetworkObject;
            }
        }

        if (closestTarget == null)
            return;

        Vector3 direction = (
            closestTarget.transform.position - transform.position
        ).normalized;

        Vector3 newDirection = Vector3.Lerp(
            rb.linearVelocity.normalized,
            direction,
            homingStrength * Time.fixedDeltaTime
        );

        float speed = rb.linearVelocity.magnitude;

        rb.linearVelocity = newDirection.normalized * speed;

        transform.rotation = Quaternion.LookRotation(
            rb.linearVelocity
        );
    }

    private void Despawn()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}