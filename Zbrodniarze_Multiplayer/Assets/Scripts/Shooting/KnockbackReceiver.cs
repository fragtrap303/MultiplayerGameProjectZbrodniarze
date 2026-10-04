using UnityEngine;

public class KnockbackReceiver : MonoBehaviour
{
    [SerializeField] private float knockbackDecay = 8f;

    private CharacterController characterController;

    private Vector3 knockbackVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (knockbackVelocity.sqrMagnitude <= 0.01f)
        {
            knockbackVelocity = Vector3.zero;
            return;
        }

        characterController.Move(knockbackVelocity * Time.deltaTime);

        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();

        knockbackVelocity = direction * force;
    }
}