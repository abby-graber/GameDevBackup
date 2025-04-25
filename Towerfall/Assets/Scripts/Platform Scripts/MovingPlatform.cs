using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public LayerMask playerLayer;
    public float speed = 5;
    public float sightRange;

    private bool toggle = false;
    private float lerpValue = 0;
    private bool playerInSightRange = false;
    private Transform playerTransform;
    private Vector3 previousPosition;

    void Start()
    {
        pointA = transform.position;
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Check for player in sight range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);

        if (playerInSightRange)
        {
            // Move between points
            if (toggle)
            {
                lerpValue += Time.fixedDeltaTime * speed;
                if (lerpValue >= 1f)
                {
                    lerpValue = 1f;
                    toggle = false;
                }
            }
            else
            {
                lerpValue -= Time.fixedDeltaTime * speed;
                if (lerpValue <= 0f)
                {
                    lerpValue = 0f;
                    toggle = true;
                }
            }

            // Move platform and calculate displacement
            Vector3 newPosition = Vector3.Lerp(pointA, pointB, lerpValue);
            Vector3 displacement = newPosition - previousPosition;
            transform.position = newPosition;

            // Move player only if they are on the platform
            if (playerTransform != null)
            {
                playerTransform.position += displacement;
            }

            previousPosition = newPosition;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerTransform = other.transform;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerTransform = null;
        }
    }
}
