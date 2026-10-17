using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Ammo UI")]
    [SerializeField] private BaseShooting baseShooting;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text reloadText;

    private void OnEnable()
    {
        if (baseShooting == null)
            return;

        baseShooting.AmmoChanged += UpdateAmmoDisplay;
        baseShooting.ReloadStateChanged += UpdateReloadDisplay;

        UpdateAmmoDisplay(
            baseShooting.CurrentAmmo,
            baseShooting.ReserveAmmo
        );

        UpdateReloadDisplay(baseShooting.IsReloading);
    }

    private void OnDisable()
    {
        if (baseShooting == null)
            return;

        baseShooting.AmmoChanged -= UpdateAmmoDisplay;
        baseShooting.ReloadStateChanged -= UpdateReloadDisplay;
    }

    private void UpdateAmmoDisplay(int currentAmmo, int reserveAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {baseShooting.MagazineSize}";
        }
    }

    private void UpdateReloadDisplay(bool isReloading)
    {
        if (reloadText != null)
        {
            reloadText.text = isReloading ? "RELOADING..." : "";
        }
    }
}