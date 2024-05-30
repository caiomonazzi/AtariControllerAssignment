using UnityEngine;
using System.Collections;

public class GearController : MonoBehaviour
{
    private Character character;
    private GearItem currentGear;
    private Coroutine gearCoroutine;
    private AudioSource audioSource;

    private Rigidbody2DParameters originalParams;

    private void Awake()
    {
        character = GetComponent<Character>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Ensure the audio source is set to loop
    }

    private void Start()
    {
        originalParams = character.GetOriginalRigidbody2DParameters();
    }

    public void CollectGear(GearItem newGear)
    {
        if (gearCoroutine != null)
        {
            StopCoroutine(gearCoroutine);
        }

        ApplyGearParameters(newGear);
        gearCoroutine = StartCoroutine(RemoveGearAfterTime(newGear.duration));
    }

    private void ApplyGearParameters(GearItem gear)
    {
        currentGear = gear;
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

        character.runSpeed = gear.runSpeed;
        character.walkSpeed = gear.walkSpeed;

        if (gear.changeMass)
        {
            rb.mass = gear.mass;
        }
        rb.gravityScale = gear.gravityScale;
        rb.drag = gear.drag;

        // Play loop sound if assigned
        if (gear.loopSound != null)
        {
            audioSource.clip = gear.loopSound;
            audioSource.Play();
        }


        // Change the gear sprite
        if (character.gearHolder != null)
        {
            SpriteRenderer gearSpriteRenderer = character.gearHolder.GetComponent<SpriteRenderer>();
            if (gearSpriteRenderer != null)
            {
                gearSpriteRenderer.sprite = gear.gearSprite;
            }
        }
    }

    private IEnumerator RemoveGearAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveGear();
    }

    private void RemoveGear()
    {
        currentGear = null;
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

        rb.bodyType = originalParams.bodyType;
        rb.sharedMaterial = originalParams.material;
        rb.simulated = originalParams.simulated;
        rb.useAutoMass = originalParams.useAutoMass;
        rb.mass = originalParams.mass;
        rb.drag = originalParams.linearDrag;
        rb.angularDrag = originalParams.angularDrag;
        rb.gravityScale = originalParams.gravityScale;
        rb.collisionDetectionMode = originalParams.collisionDetectionMode;
        rb.interpolation = originalParams.interpolation;
        rb.constraints = originalParams.constraints;

        character.runSpeed = character.originalRunSpeed;
        character.walkSpeed = character.originalWalkSpeed;

        // Reset the gear sprite
        if (character.gearHolder != null)
        {
            SpriteRenderer gearSpriteRenderer = character.gearHolder.GetComponent<SpriteRenderer>();
            if (gearSpriteRenderer != null)
            {
                gearSpriteRenderer.sprite = null;
            }
        }
    }
}
