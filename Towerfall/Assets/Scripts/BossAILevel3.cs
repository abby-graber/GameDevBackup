using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BossLevel3 : MonoBehaviour
{
    public NavMeshAgent nav;
    public Transform player;
    public LayerMask groundLayer, playerLayer;
    public float walkPointRange;
    public float timeBetweenAttacks;
    public float sightRange;
    public float attackRange;
    public int damage;
    // public Animator animator;
    public ParticleSystem hitEffect;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool alreadyAttacked;

    private void Awake()
    {
        // animator = GetComponent<Animator>();
        player = GameObject.Find("Player").transform;
        nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        bool playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        bool playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            nav.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        // animator.SetFloat("Velocity", 0.2f);

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        nav.SetDestination(player.position);
        // animator.SetFloat("Velocity", 0.6f);
        nav.isStopped = false;
    }

    private void AttackPlayer()
    {
        nav.SetDestination(transform.position);
        nav.isStopped = true;

        if (!alreadyAttacked)
        {
            transform.LookAt(player.position);
            alreadyAttacked = true;
            // animator.SetBool("Attack", true);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                if (hit.collider.CompareTag("Player")) 
                {
                    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        //if (animator.GetBool("Attack"))
        //{
        //    animator.SetBool("Attack", false);
        //}
    }

    private void DestroyEnemy()
    {
        StartCoroutine(DestroyEnemyCoroutine());
    }

    private IEnumerator DestroyEnemyCoroutine()
    {
        // animator.SetBool("Dead", true);
        yield return new WaitForSeconds(1.8f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
