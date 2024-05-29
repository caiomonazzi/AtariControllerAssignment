using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float attackSpeed;
    public float range;
    public GameObject projectilePrefab; // Prefab for the weapon's projectile
    public Sprite weaponSprite; // Sprite for the weapon
    public Projectile.PropulsionType propulsionType; // Propulsion type for the projectile
    public float amountPropulsion;
    public float switchTime; // Duration the weapon will be active
}

public class WeaponController : MonoBehaviour
{
    private Character character;
    [SerializeField] private Weapon[] weapons; // Array of available weapons
    [SerializeField] private float weaponSwitchTime = 5f; // Default time to switch back to melee
    [SerializeField] private Transform shootingPoint; // Point from which to attack/shoot

    private SpriteRenderer shootingPointSpriteRenderer;
    private AttackController attackController;
    private Coroutine switchWeaponCoroutine;
    private Weapon currentWeapon;
    private bool hasWeapon = false;

    private void Awake()
    {
        character = FindFirstObjectByType<Character>();
        attackController = GetComponent<AttackController>();
        shootingPointSpriteRenderer = shootingPoint.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !character.isJumping)
        {
            HandleAttack();
        }
    }

    private void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        if (weapon == null)
        {
            // Reset to melee attack settings
            shootingPointSpriteRenderer.sprite = null;
            hasWeapon = false;
        }
        else
        {
            // Equip the ranged weapon
            shootingPointSpriteRenderer.sprite = weapon.weaponSprite;
            hasWeapon = true;
        }
    }

    private void HandleAttack()
    {
        if (hasWeapon && currentWeapon != null)
        {
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentWeapon.range, attackController.whatIsEnemy);
            if (enemiesInRange.Length > 0)
            {
                Collider2D nearestEnemy = GetNearestEnemyInFront(enemiesInRange);
                if (nearestEnemy != null)
                {
                    float distance = Vector2.Distance(character.attackPoint.position, nearestEnemy.transform.position);
                    if (distance <= attackController.range)
                    {
                        PerformMeleeAttack();
                    }
                    else
                    {
                        PerformRangedAttack(nearestEnemy.transform.position);
                    }
                }
            }
        }
        else
        {
            PerformMeleeAttack();
        }
    }

    private Collider2D GetNearestEnemyInFront(Collider2D[] enemies)
    {
        Collider2D nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(shootingPoint.position, enemy.transform.position);
            Vector2 directionToEnemy = enemy.transform.position - shootingPoint.position;

            // Check if the enemy is in front of the player
            if ((character.isFacingRight && directionToEnemy.x > 0) || (!character.isFacingRight && directionToEnemy.x < 0))
            {
                if (distance < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;
                }
            }
        }

        return nearestEnemy;
    }

    private void PerformMeleeAttack()
    {
        attackController.Hit();  // Calls the appropriate attack logic in AttackController
        Debug.Log("Performing Melee Attack");
    }

    private void PerformRangedAttack(Vector2 enemyPosition)
    {
        Vector2 direction = (enemyPosition - (Vector2)shootingPoint.position).normalized;
        Shoot(direction, enemyPosition);
    }

    private void Shoot(Vector2 direction, Vector2 targetPosition)
    {
        if (currentWeapon.projectilePrefab != null)
        {
            // Instantiate the projectile at the shootingPoint position with no rotation
            GameObject projectile = Instantiate(currentWeapon.projectilePrefab, shootingPoint.position, Quaternion.identity);

            // Get the Projectile script from the projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Set the projectile parameters
                projectileScript.Initialize(targetPosition, currentWeapon.damage, currentWeapon.attackSpeed, attackController.whatIsEnemy, LayerMask.GetMask("Player"), currentWeapon.propulsionType, currentWeapon.amountPropulsion);
            }

            Debug.Log("Shooting Test");
        }
    }

    public void CollectWeapon(Weapon newWeapon)
    {
        if (switchWeaponCoroutine != null)
        {
            StopCoroutine(switchWeaponCoroutine);
        }

        EquipWeapon(newWeapon);
        float switchTime = newWeapon != null ? newWeapon.switchTime : weaponSwitchTime;
        switchWeaponCoroutine = StartCoroutine(SwitchBackToMeleeAfterTime(switchTime));
    }

    private IEnumerator SwitchBackToMeleeAfterTime(float switchTime)
    {
        yield return new WaitForSeconds(switchTime);
        EquipWeapon(null); // Switch back to melee
    }
}
