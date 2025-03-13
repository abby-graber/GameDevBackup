using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 entryDirection = Vector3.up;
    [SerializeField] private bool localDirection = false;
    [SerializeField, Range(1.0f, 2.0f)] private float triggerScale = 1.25f;
    private BoxCollider platformCollider = null;
    private BoxCollider checkTrigger = null;

    private void Start()
    {
        platformCollider = GetComponent<BoxCollider>();
        platformCollider.isTrigger = false;

        checkTrigger = gameObject.AddComponent <BoxCollider>();
        checkTrigger.size = platformCollider.size * triggerScale;
        checkTrigger.center = platformCollider.center;
        checkTrigger.isTrigger = true; 
    }

    private void OnTriggerStay(Collider other)
    {
        if(Physics.ComputePenetration(
            checkTrigger, transform.position, transform.rotation,
            other, other.transform.position, other.transform.rotation,
            out Vector3 collisionDirection, out float penetrationDepth))
        {
            Vector3 direction;
            if (localDirection)
            {
                direction = transform.TransformDirection(entryDirection.normalized);

            }
            else
            {
                direction = entryDirection;
            }

            float dot = Vector3.Dot(direction, collisionDirection);

            // Opposite direction; passing not allowed
            if(dot < 0)
            {
                Physics.IgnoreCollision(platformCollider, other, false);
            } else
            {
                Physics.IgnoreCollision(platformCollider, other, true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 direction;
        if(localDirection)
        {
            direction = transform.TransformDirection(entryDirection.normalized);

        }
        else
        {
            direction = entryDirection;
        }

            Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, entryDirection);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -entryDirection);
    }
}