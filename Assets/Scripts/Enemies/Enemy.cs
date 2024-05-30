using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State { Idle, Patrolling, Targeting, Attacking }
    [SerializeField] private State currentState = State.Idle; // Current state of the turret, serialized for debugging

    public float detectionRange = 15f; // Range within which the turret can detect the target
    public float attackRange = 10f; // Range within which the turret can fire at the target
    public float fireRate = 1f; // How often the turret fires (shots per second)
    public GameObject projectilePrefab; // The projectile prefab to instantiate
    public Transform firePoint; // The point from where the projectile is fired
    public Transform groundCheck; // Point for checking if the turret is on the ground
    public Transform frontCheck; // Point for checking if the turret is hitting something in front
    public LayerMask groundLayer; // Layer mask to define what is ground

    public bool isStatic = true; // Determines if the turret is static or can walk
    public float walkSpeed = 2f; // Speed at which the turret walks
    public float patrolRadius = 10f; // The maximum distance the turret can patrol from its starting point

    private Transform target; // The target to aim at
    private float fireCountdown = 0f; // Countdown timer for firing
    private AudioSource audioSource;
    public AudioClip fireSound; // Sound to play when firing

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isFacingRight = true;

    private Vector3 initialPosition; // The starting point of the turret
    private Vector3 lastPosition;
    private float stuckTime = 0f;
    private float stuckThreshold = 2f; // Time before considering stuck

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
        initialPosition = transform.position; // Store the initial position
    }

    private void Update()
    {
        FindTarget();

        switch (currentState)
        {
            case State.Idle:
                if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
                {
                    currentState = isStatic ? State.Targeting : State.Patrolling;
                }
                break;
            case State.Patrolling:
                if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
                {
                    currentState = State.Targeting;
                }
                else
                {
                    MoveForward();
                    CheckGround();
                    CheckFront();
                    CheckIfStuck();
                    StayWithinRadius();
                }
                break;
            case State.Targeting:
                if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
                {
                    currentState = State.Attacking;
                }
                else if (target == null || Vector3.Distance(transform.position, target.position) > detectionRange)
                {
                    currentState = isStatic ? State.Idle : State.Patrolling;
                }
                else
                {
                    if (isStatic) AimAtTarget();
                }
                break;
            case State.Attacking:
                if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
                {
                    if (fireCountdown <= 0f)
                    {
                        Fire();
                        fireCountdown = 1f / fireRate;
                    }
                    fireCountdown -= Time.deltaTime;
                }
                else
                {
                    currentState = State.Targeting;
                }
                break;
        }
    }

    private void MoveForward()
    {
        rb.velocity = new Vector2((isFacingRight ? 1 : -1) * walkSpeed, rb.velocity.y);
    }

    private void FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (GameObject potentialTarget in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.transform.position);
            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestTarget = potentialTarget;
            }
        }

        if (nearestTarget != null && shortestDistance <= detectionRange)
        {
            target = nearestTarget.transform;
        }
        else
        {
            target = null;
        }
    }

    private void AimAtTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void Fire()
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            Vector3 targetPosition = target.position;
            projectile.Initialize(targetPosition, projectile.damage, projectile.speed, LayerMask.GetMask("Enemy"), LayerMask.GetMask("Player"), projectile.propulsionType, projectile.amountPropulsion);
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        Vector3 firePointScale = firePoint.localScale;
        firePointScale.x *= -1;
        firePoint.localScale = firePointScale;
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.2f, groundLayer);
        if (hit.collider == null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            Flip();
        }
    }

    private void CheckFront()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(frontCheck.position, direction, 0.2f, groundLayer);
        if (hit.collider != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            Flip();
        }
    }

    private void CheckIfStuck()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime >= stuckThreshold)
            {
                Flip();
                stuckTime = 0f;
            }
        }
        else
        {
            stuckTime = 0f;
        }

        lastPosition = transform.position;
    }

    private void StayWithinRadius()
    {
        float distanceFromStart = Vector3.Distance(transform.position, initialPosition);
        if (distanceFromStart > patrolRadius)
        {
            Flip();
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, walkSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, 0.25f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(frontCheck.position, 0.25f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition, patrolRadius); // Draw the patrol radius
    }
}
