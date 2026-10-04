using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class BaseShooting : NetworkBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 30f;

    [Header("Fire Mode")]
    [SerializeField] private FireMode fireMode = FireMode.Single;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float spread = 0f;

    private float nextFireTime;
    private bool isBursting;

    private void Update()
    {
        if(!IsOwner) return;
        switch (fireMode)
        {
            case FireMode.Single:
                HandleSingleFire();
                break;

            case FireMode.Automatic:
                HandleAutomaticFire();
                break;

            case FireMode.Burst:
                HandleBurstFire();
                break;
        }
    }

    private void HandleSingleFire()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void HandleAutomaticFire()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void HandleBurstFire()
    {
        if (Input.GetMouseButtonDown(0) &&
            Time.time >= nextFireTime &&
            !isBursting)
        {
            StartCoroutine(Burst());
        }
    }

    private System.Collections.IEnumerator Burst()
    {
        isBursting = true;

        for (int i = 0; i < burstCount; i++)
        {
            Shoot();

            yield return new WaitForSeconds(fireRate);
        }

        nextFireTime = Time.time;
        isBursting = false;
    }

    private void Shoot()
    {
        for (int i = 0; i < projectilesPerShot; i++)
        {
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        Vector3 direction = GetShootDirection();

        ShootProjectileServerRpc(firePoint.position, direction);
    }

    [ServerRpc]
    private void ShootProjectileServerRpc(Vector3 position, Vector3 direction)
    {
        Projectile projectile = Instantiate(
            projectilePrefab,
            position,
            Quaternion.LookRotation(direction)
        );

        projectile.SetShooter(NetworkObject);
        projectile.NetworkObject.Spawn();

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
    }

    private Vector3 GetShootDirection()
    {
        Vector3 direction = firePoint.forward;

        if (spread > 0f)
        {
            direction = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            ) * direction;
        }

        return direction.normalized;
    }
}