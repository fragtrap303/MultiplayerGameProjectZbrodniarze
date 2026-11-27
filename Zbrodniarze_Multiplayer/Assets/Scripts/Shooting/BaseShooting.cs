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
    [SerializeField] private float burstCooldown = 0.4f;
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float spread = 0f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int startingReserveAmmo = 90;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private bool autoReloadWhenEmpty = true;

    [Header("References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator FPSAnimator;
    [SerializeField] private WeaponConfig weaponConfig;   

    private float nextFireTime;
    private bool isBursting;
    private bool isReloading;
    private int currentAmmo;
    private int reserveAmmo;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;
    public WeaponConfig Weapon => weaponConfig;

    public event System.Action<int, int> AmmoChanged;
    public event System.Action<bool> ReloadStateChanged;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(reloadKey))
        {
            RequestReloadServerRpc();
        }

        if (isReloading)
            return;
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            currentAmmo = magazineSize;
            reserveAmmo = startingReserveAmmo;
            NotifyAmmoChanged();
        }
    }

    
    public void ApplyWeapon(WeaponConfig w)
    {
        StopAllCoroutines();   
        isBursting = false;
        isReloading = false;

        fireMode = w.fireMode;
        fireRate = w.fireRate;
        burstCount = w.burstCount;
        burstCooldown = w.burstCooldown;
        projectilesPerShot = w.projectilesPerShot;
        spread = w.spread;
        projectileSpeed = w.projectileSpeed;
        magazineSize = w.magazineSize;
        startingReserveAmmo = w.startingReserveAmmo;
        reloadTime = w.reloadTime;

        currentAmmo = magazineSize;
        reserveAmmo = startingReserveAmmo;
        nextFireTime = 0f;

        NotifyAmmoChanged();
        ReloadStateChanged?.Invoke(false);
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
            
            if (isReloading || currentAmmo <= 0)
                break;

            Shoot();

            
            if (i < burstCount - 1)
                yield return new WaitForSeconds(fireRate);
        }

        nextFireTime = Time.time + burstCooldown;
        isBursting = false;
    }

    private void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
            return;

        currentAmmo--;
        NotifyAmmoChanged();

        for (int i = 0; i < projectilesPerShot; i++)
        {
            PlayShootAnimationRpc();
            ShootProjectile();
        }

        if (currentAmmo <= 0 && autoReloadWhenEmpty && reserveAmmo > 0)
        {
            RequestReloadServerRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayShootAnimationRpc()
    {
        if (IsOwner)
            FPSAnimator.SetTrigger("didShoot");
        else
            playerAnimator.SetTrigger("didShoot");
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

        
        if (weaponConfig != null && weaponConfig.damage > 0f)
            projectile.Configure(weaponConfig);

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

    [ServerRpc]
    private void RequestReloadServerRpc()
    {
        if (isReloading ||
            currentAmmo >= magazineSize ||
            reserveAmmo <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private void NotifyAmmoChanged()
    {
        AmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        ReloadStateChanged?.Invoke(true);

        PlayReloadAnimationRpc();

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        NotifyAmmoChanged();

        isReloading = false;
        ReloadStateChanged?.Invoke(false);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayReloadAnimationRpc()
    {
        if (IsOwner)
            FPSAnimator.SetTrigger("didReload");
        else
            playerAnimator.SetTrigger("didReload");
    }
}