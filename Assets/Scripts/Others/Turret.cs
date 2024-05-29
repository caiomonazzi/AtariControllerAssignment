using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target; // The target to aim at (e.g., player)
    public float range = 10f; // Range within which the turret can detect and fire at the target
    public float fireRate = 1f; // How often the turret fires (shots per second)
    public GameObject projectilePrefab; // The projectile prefab to instantiate
    public Transform firePoint; // The point from where the projectile is fired

    private float fireCountdown = 0f; // Countdown timer for firing
    private AudioSource audioSource;
    public AudioClip fireSound; // Sound to play when firing

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
        }

        if (target != null)
        {
            AimAtTarget();

            if (fireCountdown <= 0f)
            {
                Fire();
                fireCountdown = 1f / fireRate;
            }

            fireCountdown -= Time.deltaTime;
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
