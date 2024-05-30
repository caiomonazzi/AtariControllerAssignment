/*using UnityEngine;

public class Platformer : MonoBehaviour
{
    #region Unity Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AttachPlayer(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            DetachPlayer(collision.transform);
        }
    }

    #endregion

    #region Private Methods

    private void AttachPlayer(Transform player)
    {
        player.SetParent(transform);
    }

    private void DetachPlayer(Transform player)
    {
        player.SetParent(null);
    }

    #endregion
}
*/