using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target; // The target to aim at (e.g., player)
    public float range = 10f; // Range within which the turret can detect and fire at the target
    public float fireRate = 1f; // How often the turret fires (shots per second)
    public GameObject projectilePrefab; // The projectile prefab to instantiate
    public Transform firePoint; // The point from where the projectile is fired
    public Transform groundCheck; // Point for checking if the turret is on the ground
    public LayerMask groundLayer; // Layer mask to define what is ground

    public bool isStatic = true; // Determines if the turret is static or can walk
    public float walkSpeed = 2f; // Speed at which the turret walks

    private float fireCountdown = 0f; // Countdown timer for firing
    private AudioSource audioSource;
    public AudioClip fireSound; // Sound to play when firing

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isFacingRight = true;

    private Vector3 lastPosition;
    private float stuckTime = 2f;
    private float stuckThreshold = 5f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > range)
        {
            FindTarget();
        }

        if (target != null && Vector3.Distance(transform.position, target.position) <= range)
        {
            if (isStatic)
            {
                AimAtTarget();
            }
            else
            {
                MoveTowardsTarget();
            }

            if (fireCountdown <= 0f)
            {
                Fire();
                fireCountdown = 1f / fireRate;
            }

            fireCountdown -= Time.deltaTime;
        }

        if (!isStatic)
        {
            MoveForward();
            CheckGround();
            CheckIfStuck();
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

        if (nearestTarget != null && shortestDistance <= range)
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
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.Normalize();

        rb.velocity = new Vector2(direction.x * walkSpeed, rb.velocity.y);

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }

        Debug.Log("Moving towards target: " + direction);
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (!isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        Debug.Log("Ground check: " + isGrounded);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }
}
