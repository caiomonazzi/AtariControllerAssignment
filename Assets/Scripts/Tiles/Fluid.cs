using UnityEngine;
using System.Collections;

public class Fluid : MonoBehaviour
{
    [SerializeField] private bool doDamage;
    [SerializeField] private int damage = 0;
    [SerializeField] private float fluidGravityScale = 0.5f;
    [SerializeField] private float fluidDrag = 5f;
    [SerializeField] private float fluidRunSpeed = 10f;
    [SerializeField] private float fluidWalkSpeed = 5f;

    [SerializeField] private bool changeMass; // New boolean field
    [SerializeField] private float mass;

    [SerializeField] private bool hasHitSound; // Determines if a hit sound should be played
    [SerializeField] private AudioClip hitSound; // Sound to play when the player hits the fluid

    [SerializeField] private bool hasLoopSound; // Determines if a loop sound should be played while the player is in the fluid
    [SerializeField] private AudioClip loopSound; // Loopable sound to play while the player is in the fluid
    private AudioSource audioSource;


    private bool isInFluid = false;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null && !isInFluid)
            {
                Debug.Log("Player entered fluid");
                isInFluid = true;

                Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

                character.runSpeed = fluidRunSpeed;
                character.walkSpeed = fluidWalkSpeed;

                if (changeMass) // Check if mass should be changed
                {
                    rb.mass = mass;
                }
                rb.gravityScale = fluidGravityScale;
                rb.drag = fluidDrag;

                if (doDamage)
                {
                    StartCoroutine(ApplyDamageOverTime(character));
                }

                if (hasHitSound && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }

                if (hasLoopSound && loopSound != null)
                {
                    audioSource.clip = loopSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null && isInFluid)
            {
                Debug.Log("Player exited fluid");
                isInFluid = false;

                Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
                Rigidbody2DParameters originalParams = character.GetOriginalRigidbody2DParameters();

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

                if (hasLoopSound && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    private IEnumerator ApplyDamageOverTime(Character character)
    {
        HealthController health = character.GetComponent<HealthController>();

        while (isInFluid)
        {
            health.TakeDamage(damage);
            yield return new WaitForSeconds(1f); // Damage interval
        }
    }
}


