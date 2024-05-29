using UnityEngine;

public class HealthController : MonoBehaviour
{
    #region Variables

    public int maxLife = 100; // Maximum life.
    public int health = 0; // Current life.

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isHurting = false;

    private Animator animator;

    // Delegate to determine when to update the life bar.
    public delegate void TakeHealth(int amount);
    public TakeHealth HealthEvent;

    // Delegate to notify when the character dies.
    public delegate void CharacterDeath();
    public CharacterDeath OnDeath;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        SetInitialHealth();
    }

    #endregion

    #region Public Methods

    public void TakeDamage(int amount)
    {
        if (health - amount > health) return; // Prevent invalid damage application

        ApplyDamage(amount);

        if (health <= 0)
        {
            HandleDeath();
        }
        else
        {
            HandleHurt(true);
        }

        LaunchHealthEvent();
    }

    public void Heal(int amount)
    {
        if (health + amount < health) return; // Prevent invalid healing application

        ApplyHeal(amount);
        LaunchHealthEvent();
    }

    public void Hurt(bool h)
    {
        isHurting = h;

        if (h) animator.Play("Hurt");

        animator.SetBool("IsHurting", isHurting);
    }

    public void Death()
    {
        isDead = true;
        animator.SetBool("IsDead", isDead);
        GetComponent<Collider2D>().enabled = false;
    }

    public void LaunchHealthEvent()
    {
        HealthEvent?.Invoke(health);
    }

    public void StopHurt()
    {
        Hurt(false);
    }

    #endregion

    #region Private Methods

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
    }

    private void SetInitialHealth()
    {
        // Set health.
        health = maxLife;
    }

    private void ApplyDamage(int amount)
    {
        health -= amount;
    }

    private void ApplyHeal(int amount)
    {
        health += amount;

        if (health > maxLife)
        {
            health = maxLife;
        }
    }

    private void HandleDeath()
    {
        health = 0;
        Death();
        OnDeath?.Invoke();
        Destroy(gameObject, 2f); // Destroy the game object after 2 seconds
    }

    private void HandleHurt(bool h)
    {
        Hurt(h);
    }

    #endregion
}
