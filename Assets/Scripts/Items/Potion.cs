using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour
{
    public int potionAmount = 20; // Amount of health to restore
    public bool isPoison = false; // Determines if the potion is poison

    public bool rotationOn = true; // Determines if the potion should rotate
    public float rotationSpeed = 50f; // Speed of the rotation effect
    public bool limitRotation = true; // Restrict rotation to specified angles
    public float maxRotationAngle = 30f; // Maximum rotation angle for restricted rotation
    public bool wobbleOn = false; // Determines if the potion should wobble
    public float wobbleSpeed = 1f; // Speed of the wobble effect
    public float wobbleAmplitude = 0.5f; // Amplitude of the wobble effect
    public AudioClip collectSound; // Sound to play when the potion is collected
    public GameObject collectEffect; // Particle effect to play when the potion is collected
    public float effectDestroyDelay = 1f; // Time in seconds to wait before destroying the collectible effect

    private Vector3 initialPosition;
    private float rotationAngle = 0f;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (rotationOn)
        {
            RotatePotion();
        }

        if (wobbleOn)
        {
            WobblePotion();
        }
    }

    private void RotatePotion()
    {
        if (limitRotation)
        {
            rotationAngle += rotationSpeed * Time.deltaTime;
            float clampedAngle = Mathf.PingPong(rotationAngle, maxRotationAngle * 2) - maxRotationAngle;
            transform.rotation = Quaternion.Euler(0, 0, clampedAngle);
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void WobblePotion()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmplitude;
        transform.position = initialPosition + new Vector3(0, wobble, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthController healthController = other.GetComponent<HealthController>();
            if (healthController != null)
            {
                if (isPoison)
                {
                    healthController.TakeDamage(potionAmount); // Damage the player
                }
                else
                {
                    healthController.Heal(potionAmount); // Heal the player
                }

                // Play the collection sound effect
                if (collectSound != null && LevelManager.Instance != null && LevelManager.Instance.sfxSource != null)
                {
                    LevelManager.Instance.sfxSource.PlayOneShot(collectSound);
                }

                // Instantiate the collection particle effect
                if (collectEffect != null)
                {
                    GameObject effectInstance = Instantiate(collectEffect, transform.position, Quaternion.identity);
                    Destroy(effectInstance, effectDestroyDelay); // Destroy the effect after delay
                }

                Destroy(gameObject); // Destroy the potion after use
            }
        }
    }
}