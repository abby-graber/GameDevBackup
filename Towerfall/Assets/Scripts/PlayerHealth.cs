using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    public Transform respawnPoint; // Assign this in the Inspector
    public float respawnDelay = 2f; // Time before respawn

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Player took damage! Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        gameObject.SetActive(false); // Temporarily disable the player
        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        health = 100; // Reset health
        transform.position = respawnPoint.position; // Move player to spawn
        gameObject.SetActive(true); // Re-enable the player
        Debug.Log("Player respawned!");
    }
}
