using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // Assign your Main Camera's transform in the Inspector
    private Animator animator;

    // Ground Movement
    private Rigidbody rb;
    [SerializeField] private float walkSpeed = 5f;

    [SerializeField] private float runSpeed = 10f;

    private float moveSpeed;

    private float moveHorizontal;
    private float moveForward;
    [SerializeField] private float friction = 10.0f;

    // Jumping
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f; 
    [SerializeField] private float ascendMultiplier = 2f;

    [SerializeField] private AudioClip jumpSound;
    
    private bool isGrounded = true;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

        // Set the raycast to be slightly beneath the player's feet
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 6) + 0.5f;

        // Hides the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward   = Input.GetAxisRaw("Vertical");

        // Debug or check your input
        // Debug.Log($"Horizontal: {moveHorizontal}\nVertical: {moveForward}");

        // Update animator blend-tree parameters
        animator.SetFloat("horizontal", moveHorizontal);
        //animator.SetFloat("vertical",   moveForward);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Ground Check
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);

            if (moveForward > 0)
            {
                animator.SetTrigger("landingRollTrigger");
            }

            animator.SetBool("inAir", !isGrounded);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        /*
        // If you want zero movement control in the air, keep this condition.
        // Otherwise, remove it if you want partial air control.
        if (isGrounded)
        {
            MovePlayer();
        }
        */

        if (Input.GetKey(KeyCode.LeftShift) && (moveHorizontal != 0 || moveForward != 0)){
            Debug.Log("Left Shift Pressed");
            moveSpeed = runSpeed;
            animator.SetFloat("vertical", 1.0f);
        }
        else{
            Debug.Log("Left Shift Not Pressed");
            moveSpeed = walkSpeed;
            animator.SetFloat("vertical", moveForward / 2);
        }

        MovePlayer();

        ApplyJumpPhysics();
    }

    void MovePlayer()
    {
        // 1. Flatten camera direction so player moves on horizontal plane
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // 2. Build movement direction using camera-based forward/right
        Vector3 movement = (cameraRight * moveHorizontal + cameraForward * moveForward).normalized;
        Vector3 targetVelocity = movement * moveSpeed;

        // Update animator for forward speed (optional)
        animator.SetFloat("zSpeed", targetVelocity.z);

        // This makes it so that the camera does not rotate the character model when the player is walking. 
        // Want to change it so that it camera also does not change the direction the character moving when walking.
        /*
        if (movement.magnitude > 0.1f && moveSpeed == runSpeed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        */

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 3. Apply velocity with friction if no input
        if (moveForward != 0 || moveHorizontal != 0) // If input is detected
        {
            // Replace linearVelocity -> velocity if you see errors
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            // Apply friction by smoothly reducing velocity
            rb.linearVelocity = new Vector3(
                Mathf.Lerp(rb.linearVelocity.x, 0, friction * Time.fixedDeltaTime),
                rb.linearVelocity.y,
                Mathf.Lerp(rb.linearVelocity.z, 0, friction * Time.fixedDeltaTime)
            );
        }
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        animator.SetTrigger("jumpTrigger");
        SoundFXManager.instance.PlaySoundFXClip(jumpSound, transform, 1f);
    }

    void ApplyJumpPhysics()
    {
        // If using linearVelocity, keep it consistent. Otherwise use rb.velocity if you prefer.
        if (rb.linearVelocity.y < 0) // Falling
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0) // Rising
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (ascendMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
