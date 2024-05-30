using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weapon; // Assign the weapon ScriptableObject in the Inspector
    public AudioClip pickupSound; // Assign the pickup sound in the Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WeaponController weaponController = collision.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.CollectWeapon(weapon);
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
