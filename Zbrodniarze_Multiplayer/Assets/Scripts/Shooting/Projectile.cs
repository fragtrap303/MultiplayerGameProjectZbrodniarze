using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;

    private NetworkObject shooter;

    public void SetShooter(NetworkObject shooterObject)
    {
        shooter = shooterObject;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(Despawn), lifetime);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !IsServer;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        Health health = collision.gameObject.GetComponentInParent<Health>();
        if (health != null && health.NetworkObject != shooter)
        {
            health.TakeDamage(damage);
        }

        Despawn();
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}