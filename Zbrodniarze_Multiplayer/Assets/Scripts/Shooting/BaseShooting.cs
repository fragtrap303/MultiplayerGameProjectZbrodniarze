using UnityEngine;

public class BaseShooting : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 30f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Projectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        rb.linearVelocity = firePoint.forward * projectileSpeed;
    }
}