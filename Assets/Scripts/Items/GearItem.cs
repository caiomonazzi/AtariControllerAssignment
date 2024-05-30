using UnityEngine;

[CreateAssetMenu(fileName = "New Gear Item", menuName = "Gear Item")]
public class GearItem : ScriptableObject
{
    public string gearName;
    public float gravityScale = 0.5f;
    public float drag = 5f;
    public float runSpeed = 10f;
    public float walkSpeed = 5f;
    public bool changeMass;
    public float mass;
    public AudioClip hitSound;
    public float duration = 5f; // Duration the gear item will be active
    public Sprite gearSprite; // Sprite for the gear
}
