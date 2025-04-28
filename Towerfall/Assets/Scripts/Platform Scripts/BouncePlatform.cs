using UnityEngine;

public class BouncePlatform : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;  // How strong the bounce is
    //[SerializeField] private string playerTag = "Player";

    private void Start(){
        Debug.Log("Bouncy Activated");
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bouncy Collided");

        if (other.CompareTag(playerTag))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Zero out current Y velocity before applying the bounce
                Vector3 velocity = rb.linearVelocity;
                velocity.y = 0f;
                rb.linearVelocity = velocity;

                // Add upward force
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
        }
    }
    */

     private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision Detected");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Collided");

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Zero out current Y velocity before applying the bounce
                Vector3 velocity = rb.linearVelocity;
                velocity.y = 0f;
                rb.linearVelocity = velocity;

                // Add upward force
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
        }
    }
}