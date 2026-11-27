using System;
using System.Collections.Generic;

public static class WeaponGenerator
{
    private delegate string Modifier(WeaponConfig w, Random r);

    
    private static readonly Modifier[] pool =
    {
        // Odbicia
        (w, r) => { w.maxBounces = r.Next(1, 4); w.damage *= 0.85f;
                    return $"Odbicia x{w.maxBounces}"; },

        // Wybuch 
        (w, r) => { w.explosionEnabled = true;
                    w.explosionRadius = Range(r, 2f, 4f);
                    w.damage *= 0.7f;
                    return $"Wybuch (promieñ {w.explosionRadius:0.#})"; },

        // Odrzut
        (w, r) => { w.knockbackEnabled = true; w.knockbackForce = Range(r, 6f, 15f);
                    return "Odrzut"; },

        // Naprowadzanie
        (w, r) => { w.homingEnabled = true;
                    w.homingRange = Range(r, 10f, 20f);
                    w.homingStrength = Range(r, 2f, 6f);
                    w.projectileSpeed *= 0.8f;
                    return "Naprowadzanie"; },

        // Œrut
        (w, r) => { w.projectilesPerShot = r.Next(3, 6);
                    w.spread = Math.Max(w.spread, Range(r, 4f, 8f));
                    w.damage *= 0.45f;
                    return $"Œrut x{w.projectilesPerShot}"; },

        // Du¿y magazynek
        (w, r) => { float m = Range(r, 1.5f, 2f);
                    w.magazineSize = (int)(w.magazineSize * m);
                    w.startingReserveAmmo = (int)(w.startingReserveAmmo * m);
                    return "Du¿y magazynek"; },

        // Szybkie prze³adowanie
        (w, r) => { w.reloadTime *= Range(r, 0.4f, 0.65f);
                    return "Szybkie prze³adowanie"; },

        // Mocne pociski
        (w, r) => { w.damage *= Range(r, 1.3f, 1.6f); w.fireRate *= 1.25f;
                    return "Mocne pociski"; },

        // Szybki ogieñ
        (w, r) => { float m = Range(r, 0.6f, 0.8f);
                    w.fireRate *= m; w.burstCooldown *= m; w.spread += 2f;
                    return "Szybki ogieñ"; },

        // Szybkie pociski
        (w, r) => { w.projectileSpeed *= Range(r, 1.5f, 2f);
                    return "Szybkie pociski"; },
    };

   
    public static void Generate(int seed, WeaponConfig w)
    {
        var r = new Random(seed);
        w.ResetStats();

        //losowy tryb strzelania
        w.fireMode = (FireMode)r.Next(0, 3);
        switch (w.fireMode)
        {
            case FireMode.Single:
                w.damage = 40; w.fireRate = 0.45f; w.magazineSize = 12;
                w.startingReserveAmmo = 48; w.reloadTime = 1.8f; w.projectileSpeed = 35f;
                break;
            case FireMode.Automatic:
                w.damage = 12; w.fireRate = 0.1f; w.magazineSize = 30;
                w.startingReserveAmmo = 120; w.reloadTime = 2.2f; w.projectileSpeed = 30f;
                w.spread = 1.5f;
                break;
            case FireMode.Burst:
                w.damage = 20; w.fireRate = 0.08f; w.burstCount = 3; w.burstCooldown = 0.45f;
                w.magazineSize = 24; w.startingReserveAmmo = 96; w.reloadTime = 2f;
                w.projectileSpeed = 32f;
                break;
        }

        //1-3 ró¿ne modyfikatory
        int count = r.Next(1, 4);
        var indices = new List<int>();
        for (int i = 0; i < pool.Length; i++) indices.Add(i);

        for (int i = 0; i < count; i++)
        {
            int pick = r.Next(indices.Count);
            w.modifierLabels.Add(pool[indices[pick]](w, r));
            indices.RemoveAt(pick);
        }

        
        if (w.explosionEnabled)
            w.explosionDamage = w.damage;

        w.damage = (float)Math.Round(w.damage, 1);
        w.explosionDamage = (float)Math.Round(w.explosionDamage, 1);
    }

    private static float Range(Random r, float min, float max)
        => min + (float)r.NextDouble() * (max - min);
}