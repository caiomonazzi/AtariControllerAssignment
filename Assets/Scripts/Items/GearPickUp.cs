using UnityEngine;
using System.Collections;

public class GearPickup : MonoBehaviour
{
    public GearItem gearItem; // Assign the GearItem ScriptableObject in the Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GearController gearController = collision.GetComponent<GearController>();
            if (gearController != null)
            {
                gearController.CollectGear(gearItem);
                Destroy(gameObject); // Destroy the pickup after collecting
            }
        }
    }
}
