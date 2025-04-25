using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -5);

    
    [SerializeField] private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    
    [SerializeField] private float mouseSensitivity = 2f;
    // Clamp pitch so the camera doesn't flip
    [SerializeField] private float pitchMin = -30f; 
    [SerializeField] private float pitchMax = 60f;

    private float yaw = 0f;   // Horizontal angle (left-right)
    private float pitch = 0f; // Vertical angle (up-down)

    private void LateUpdate()
    {
        // 1. Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. Update yaw & pitch
        yaw   += mouseX;       // rotate left/right
        pitch -= mouseY;       // rotate up/down
        pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 3. Calculate desired rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 4. Calculate desired position: target position + rotated offset
        Vector3 desiredPosition = cameraTarget.position + rotation * offset;

        // 5. Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        transform.position = smoothedPosition;

        // 6. Make the camera look at the player
        transform.LookAt(cameraTarget);
    }
}