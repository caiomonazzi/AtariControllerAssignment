using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public int damage; // The damage that the enemy will receive
    public float speed; // Constant speed of the ammo
    public float amountPropulsion; // Amount of acceleration to add

    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public enum PropulsionType
    {
        ConstantVelocity,
        Accelerating,
        Homing
    }

    public PropulsionType propulsionType;
    public Transform target; // Used for homing projectiles

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Debug.Log("Projectile initialized. Rigidbody2D component is set.");
    }

    public void Initialize(Vector3 targetPosition, int damage, float speed, LayerMask enemyLayer, LayerMask playerLayer, PropulsionType propulsionType, float amountPropulsion)
    {
        this.damage = damage;
        this.speed = speed;
        this.enemyLayer = enemyLayer;
        this.playerLayer = playerLayer;
        this.propulsionType = propulsionType;
        this.amountPropulsion = amountPropulsion;


        Vector2 direction = (targetPosition - transform.position).normalized;

        Debug.Log("Initializing projectile with parameters: ");
        Debug.Log($"Damage: {damage}, Speed: {speed}, Propulsion Type: {propulsionType}, Amount Propulsion: {amountPropulsion}");
        Debug.Log($"Projectile Position: {transform.position}, Target Position: {targetPosition}");
        Debug.Log($"Direction: {direction}");

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing.");
            return;
        }

        switch (propulsionType)
        {
            case PropulsionType.ConstantVelocity:
                rb.velocity = direction * speed;
                Debug.Log("Projectile set to ConstantVelocity.");
                break;
            case PropulsionType.Accelerating:
                StartCoroutine(AccelerateProjectile(direction));
                Debug.Log("Projectile set to Accelerating.");
                break;
            case PropulsionType.Homing:
                target = FindClosestEnemy();
                if (target != null)
                {
                    Debug.Log($"Projectile set to Homing. Target acquired: {target.name}");
                }
                else
                {
                    Debug.LogWarning("No target found for Homing projectile.");
                }
                break;
        }
    }

    private void Update()
    {
        if (propulsionType == PropulsionType.Homing && target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
    }

    private IEnumerator AccelerateProjectile(Vector2 direction)
    {
        float currentSpeed = 0f;
        while (currentSpeed < speed)
        {
            if (rb == null)
            {
                Debug.LogError("Rigidbody2D component is null during acceleration.");
                yield break; // Exit the coroutine if Rigidbody2D is null
            }

            currentSpeed += Time.deltaTime * amountPropulsion; // Adjust this to control acceleration rate
            rb.velocity = direction * currentSpeed;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (((1 << hitInfo.gameObject.layer) & playerLayer) != 0)
        {
            HealthController playerHealth = hitInfo.GetComponent<HealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (((1 << hitInfo.gameObject.layer) & enemyLayer) != 0)
        {
            HealthController enemyHealth = hitInfo.GetComponent<HealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
}
