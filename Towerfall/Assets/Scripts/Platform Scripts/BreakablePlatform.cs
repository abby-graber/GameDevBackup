using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    private float health = 100f;
    private bool takeDamage;
    private Collider platformCollider;
    public ParticleSystem hitEffectPrefab;  // Reference to the Particle System prefab

    private void Start()
    {
        platformCollider = GetComponent<Collider>();
    }

    // Method to handle damage
    public void TakeDamage(float damage)
    {
        health -= damage;
        PlayHitEffect();  // Play the hit effect when damage is taken

        // Check if the platform's health is less than or equal to zero
        if (health <= 0)
        {
            DestroyPlatform();  // Destroy the platform after it is broken
        }
    }

    // Play the hit effect at the platform's position
    private void PlayHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            // Instantiate the hit effect at the platform's position and play it
            ParticleSystem effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            effect.Play();  // Play the particle system

            // Destroy the effect after it finishes playing
            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    private void DestroyPlatform()
    {
        // Perform any necessary cleanup or visual effects before destroying the platform
        Destroy(gameObject);  // Immediately destroy the platform
    }
}
