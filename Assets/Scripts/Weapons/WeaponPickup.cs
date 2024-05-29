using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weapon; // Assign the weapon ScriptableObject in the Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WeaponController weaponController = collision.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.CollectWeapon(weapon);
                Destroy(gameObject); // Destroy the pickup after collecting
            }
        }
    }
}
