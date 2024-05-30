using UnityEngine;
using System.Collections;

public class GearPickup : MonoBehaviour
{
    public GearItem gearItem; // Assign the GearItem ScriptableObject in the Inspector
    public AudioClip pickupSound; // Assign the pickup sound in the Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GearController gearController = collision.GetComponent<GearController>();
            if (gearController != null)
            {
                gearController.CollectGear(gearItem);
                PlayPickupSound();
                Destroy(gameObject); // Destroy the pickup after collecting
            }
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound != null && LevelManager.Instance != null && LevelManager.Instance.sfxSource != null)
        {
            LevelManager.Instance.sfxSource.PlayOneShot(pickupSound);
        }
        else
        {
            Debug.LogWarning("Pickup sound or LevelManager's sfxSource is missing.");
        }
    }
}
