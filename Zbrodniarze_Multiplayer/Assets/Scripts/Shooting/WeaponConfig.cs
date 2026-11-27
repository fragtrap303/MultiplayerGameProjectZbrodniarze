using System.Collections.Generic;
using UnityEngine;

public class WeaponConfig : MonoBehaviour
{
    [Header("Shooting")]
    public FireMode fireMode;
    public float fireRate;
    public int burstCount = 3;
    public float burstCooldown = 0.4f;
    public int projectilesPerShot = 1;
    public float spread;
    public float projectileSpeed;

    [Header("Ammunition")]
    public int magazineSize;
    public int startingReserveAmmo;
    public float reloadTime;

    [Header("Projectile")]
    public float damage;
    public float lifetime = 5f;
    public int maxBounces;
    public bool explosionEnabled;
    public float explosionRadius;
    public float explosionDamage;
    public bool knockbackEnabled;
    public float knockbackForce;
    public bool homingEnabled;
    public float homingRange;
    public float homingStrength;

    [Header("Preview (UI tile)")]
    public List<string> modifierLabels = new List<string>();

    
    public void ResetStats()
    {
        fireMode = FireMode.Single;
        fireRate = 0.2f; burstCount = 3; burstCooldown = 0.4f;
        projectilesPerShot = 1; spread = 0f; projectileSpeed = 30f;

        magazineSize = 30; startingReserveAmmo = 90; reloadTime = 2f;

        damage = 25f; lifetime = 5f; maxBounces = 0;
        explosionEnabled = false; explosionRadius = 0f; explosionDamage = 0f;
        knockbackEnabled = false; knockbackForce = 0f;
        homingEnabled = false; homingRange = 0f; homingStrength = 0f;

        modifierLabels.Clear();
    }
}