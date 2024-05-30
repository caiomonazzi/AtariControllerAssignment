using UnityEngine;

public class Enemy : MonoBehaviour
{
    private HealthController healthController;
    private bool isDead = false;
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
    public AudioClip deathSound; // Sound to play when the enemy dies

    public bool shouldDropItems; // Should this enemy drop items when it dies
    public GameObject[] dropItems; // Array of items to drop when the enemy dies

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
        healthController = GetComponent<HealthController>();

        if (healthController != null)
        {
            healthController.OnDeath += Die;
        }
        lastPosition = transform.position;
        initialPosition = transform.position; // Store the initial position
    }

    private void Update()
    {
        FindTarget();
        CheckGround(); // Always check if the enemy is grounded

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
                    if (isGrounded) // Only move if grounded
                    {
                        MoveForward();
                        CheckFront();
                        CheckIfStuck();
                        StayWithinRadius();
                    }
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

        // Log the scale before flipping
        // Debug.Log("Enemy scale before flip: " + transform.localScale);

        // Multiply the enemy's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // Log the scale after flipping
        // Debug.Log("Enemy scale after setting: " + transform.localScale);

        // Flip the firePoint
        if (firePoint != null)
        {
            // Debug.Log("Before Flip: firePoint local scale: " + firePoint.localScale);

            // Multiply the firePoint's x local scale by -1.
            Vector3 firePointScale = firePoint.localScale;
            firePointScale.x *= -1;
            firePoint.localScale = firePointScale;

            // Log the scale after flipping
            // Debug.Log("After Flip: firePoint local scale: " + firePoint.localScale);
        }
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.2f, groundLayer);
        isGrounded = hit.collider != null; // Set isGrounded based on the ground check
        if (!isGrounded)
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
        Gizmos.DrawWireSphere(this.gameObject.transform.position, patrolRadius); // Draw the patrol radius
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;

        // Play death sound
        PlaySound(deathSound);

        // Drop items if applicable
        if (shouldDropItems)
        {
            DropItems();
        }

        Destroy(gameObject); // Destroy the enemy game object
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && LevelManager.Instance != null && LevelManager.Instance.sfxSource != null)
        {
            LevelManager.Instance.sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound clip or LevelManager's sfxSource is missing.");
        }
    }

    private void DropItems()
    {
        if (dropItems.Length > 0)
        {
            foreach (GameObject item in dropItems)
            {
                Instantiate(item, transform.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("No drop items assigned to Enemy.");
        }
    }
}


