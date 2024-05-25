using UnityEngine;

public class Coin : MonoBehaviour
{
    #region Variables

    [SerializeField] private int value = 1; // Coin value.
    private bool isCollected = false; // Flag to ensure coin is only collected once.

    #endregion

    #region Unity Methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            isCollected = true; // Mark the coin as collected.
            CollectCoin();
        }
    }

    #endregion

    #region Private Methods

    private void CollectCoin()
    {
        LevelManager.Instance.AddCoins(value);
        Destroy(gameObject);
    }

    #endregion
}
