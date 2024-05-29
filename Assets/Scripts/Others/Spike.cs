using UnityEngine;

public class Spike : MonoBehaviour
{
    #region Variables

    [SerializeField] private int damage = 100; // Damage amount.

    #endregion

    #region Unity Methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DamagePlayer(collision.GetComponent<HealthController>());
        }
    }

    #endregion

    #region Private Methods

    private void DamagePlayer(HealthController playerHealth)
    {
        playerHealth.TakeDamage(damage);
    }

    #endregion
}
