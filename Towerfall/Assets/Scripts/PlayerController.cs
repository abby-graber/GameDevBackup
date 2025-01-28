using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Rigidbody rb;

    public float moveSpeed = 5f;
    public float jumpForce = 10;

    public bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handle movement
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys
        
        jumpForce = 0;

        /*
        NEED TO CHECK COLLISION WITH PLAYER AND GROUND/TERRAIN. IF TRUE, SET isGrounded to true, else false
        */

        if (Input.GetButton("Jump")){
            rb.linearVelocity =  new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            Debug.Log("Jump");
        }

        Vector3 movement = new Vector3(moveX, 0, moveZ);
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // Detect collisions with the ground
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}
