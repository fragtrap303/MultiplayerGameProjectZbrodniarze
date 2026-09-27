using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Trafiony: " + collision.gameObject.name);

        Destroy(gameObject);
    }
}