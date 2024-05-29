using UnityEngine;

public class AttackController : MonoBehaviour
{
    #region Variables

    public int damage = 10;       // Amount of damage.
    public float speed = 1f;      // Attack speed.
    public float range = 1f;      // Attack radius that takes as its center the attackPoint.
    public Transform attackPoint; // Point from which to attack.
    public LayerMask whatIsEnemy; // A mask determining what is enemy to the character.

    [HideInInspector] public bool isAttacking = false;

    private Animator animator;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnDrawGizmosSelected()
    {
        DrawAttackArea();
    }

    #endregion

    #region Public Methods

    public void Attack(bool a)
    {
        isAttacking = a;
        animator.SetFloat("AttackSpeed", speed);
        animator.SetBool("IsAttacking", isAttacking);
    }

    public void Hit()
    {
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPoint.position, range, whatIsEnemy);
        DamageEnemies(enemiesToDamage);
    }

    public void StopAttack()
    {
        Attack(false);
    }

    #endregion

    #region Private Methods

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
    }

    private void DrawAttackArea()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, range);
    }

    private void DamageEnemies(Collider2D[] enemiesToDamage)
    {
        HealthController lastEnemy = null;

        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            HealthController enemyHealth = enemiesToDamage[i].GetComponent<HealthController>();

            if (lastEnemy != null && enemyHealth == lastEnemy)
                continue;

            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            lastEnemy = enemyHealth;
        }
    }

    #endregion
}
