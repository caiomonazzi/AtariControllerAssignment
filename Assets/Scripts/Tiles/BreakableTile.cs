using UnityEngine;

public class BreakableTile : MonoBehaviour
{
    public int maxHits = 3; // Number of hits the tile can take before breaking
    public AudioClip hitSound; // Sound to play when the tile is hit
    public AudioClip breakSound; // Sound to play when the tile breaks
    public bool shouldDropItems; // Should this tile drop items when broken
    public GameObject[] dropItems; // Array of items to drop when the tile breaks

    private int currentHits;

    private void Start()
    {
        currentHits = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeHit();
        }
    }

    private void TakeHit()
    {
        currentHits++;
        PlaySound(hitSound);

        if (currentHits >= maxHits)
        {
            BreakTile();
        }
    }

    private void BreakTile()
    {
        PlaySound(breakSound);

        if (shouldDropItems)
        {
            DropItems();
        }

        Destroy(gameObject); // Destroy the tile
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && LevelManager.Instance != null && LevelManager.Instance.sfxSource != null)
        {
            LevelManager.Instance.sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound clip or LevelManager's sfxSource is missing.");
        }
    }

    private void DropItems()
    {
        if (dropItems.Length > 0)
        {
            foreach (GameObject item in dropItems)
            {
                Instantiate(item, transform.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("No drop items assigned to BreakableTile.");
        }
    }
}
