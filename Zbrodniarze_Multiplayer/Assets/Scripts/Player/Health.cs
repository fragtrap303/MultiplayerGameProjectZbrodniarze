using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private readonly NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    private Renderer[] playerRenderers;
    private Collider[] playerColliders;

    private void Awake()
    {
        playerRenderers = GetComponentsInChildren<Renderer>(true);
        playerColliders = GetComponentsInChildren<Collider>(true);
    }

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        ApplyDeathState(currentHealth.Value <= 0f);
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || damage <= 0f || currentHealth.Value <= 0f)
            return;

        currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0f);

        Debug.Log($"{gameObject.name} HP: {currentHealth.Value}/{maxHealth}");

        if (currentHealth.Value <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} umarł i nie żyje.");
        ApplyDeathState(true);
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        if (newHealth <= 0f)
        {
            Die();
        }
    }

    private void ApplyDeathState(bool isDead)
    {
        foreach (Renderer playerRenderer in playerRenderers)
        {
            playerRenderer.enabled = !isDead;
        }

        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.enabled = !isDead;
        }
    }

    public void Respawn()
    {
        if (!IsServer)
            return;

        currentHealth.Value = maxHealth;
        ApplyDeathState(false);
    }

    public float GetCurrentHealth()
    {
        return currentHealth.Value;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
}