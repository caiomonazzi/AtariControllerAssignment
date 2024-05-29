using UnityEngine;

public class Slime : MonoBehaviour
{
    #region Variables

    private HealthController health;
    private AttackController attack;
    private PingPongMovement movement;
    private Animator animator;

    private bool isWalking = false;
    private float speed = 0f; // Movement speed.

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        SetMovementSpeed();

        // Subscribe to the death event
        health.OnDeath += HandleDeath;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleAnimations();

        if (ShouldStopWalking())
        {
            isWalking = false;
            return;
        }

        if (attack.isAttacking)
        {
            isWalking = false;
        }
        else
        {
            CheckAndAttackEnemies();
            isWalking = true;
        }
    }

    #endregion

    #region Private Methods

    private void InitializeComponents()
    {
        health = GetComponent<HealthController>();
        attack = GetComponent<AttackController>();
        movement = GetComponent<PingPongMovement>();
        animator = GetComponent<Animator>();
    }

    private void SetMovementSpeed()
    {
        // Set the speed of movement.
        speed = movement.speed;
    }

    private void HandleMovement()
    {
        movement.speed = isWalking ? speed : 0;
    }

    private void HandleAnimations()
    {
        animator.SetBool("IsWalking", isWalking);
    }

    private bool ShouldStopWalking()
    {
        // If can't walk...
        return health.isDead || health.isHurting || movement == null;
    }

    private void CheckAndAttackEnemies()
    {
        // Get a list with all enemies within range.
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attack.attackPoint.position, attack.range, attack.whatIsEnemy);

        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            // If the player is found in that list and not dead...
            if (enemiesToDamage[i].CompareTag("Player") && !enemiesToDamage[i].GetComponent<HealthController>().isDead)
            {
                // Attack.
                attack.Attack(true);
            }
        }
    }

    private void HandleDeath()
    {
        // Handle any additional logic you want on death, e.g., dropping loot.
        Debug.Log("Slime has died.");
        // The GameObject will be destroyed by the HealthController after 2 seconds.
    }

    #endregion
}
