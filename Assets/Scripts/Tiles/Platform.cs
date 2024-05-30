using System.Collections;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    #region Variables

    public float distance = 5f; // Distance to move in the specified direction
    public float speed = 1f;
    private float flipSpeed; // Speed at which the platform flips
    public float returnDelay = 1f; // Delay before returning to the original position

    public bool flip = false; // Does the object flip when it comes back?
    public bool teleport = false; // Does the platform teleport the player to a predefined destination?
    public AudioClip teleportSound; // Sound to play when the player is teleported
    private AudioSource audioSource; // AudioSource to play the teleport sound

    public bool isVertical = false; // Move vertically if true, horizontally if false
    public bool movePositiveDirection = true; // Move in the positive direction (right or up) if true, negative direction (left or down) if false

    public Vector3 teleportDestination; // Destination to teleport the player to, if teleport is true

    private Vector3 origin; // Point of origin
    private Vector3 target; // Calculated target position

    private Transform playerOnPlatform; // To track the player on the platform
    private bool isReturning = false; // Is the platform returning to its original position?

    #endregion

    #region Unity Methods

    private void Awake()
    {
        flipSpeed = speed * 0.01f;
        InitializeOrigin();
        CalculateTarget();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (!flip && !teleport)
        {
            StartMovementCoroutine(target);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AttachPlayer(collision.transform);
            if (flip)
            {
                StartCoroutine(FlipPlatform());
            }
            else if (teleport)
            {
                TeleportPlayer();
            }
            else
            {
                StopAllCoroutines(); // Stop any return movement
                isReturning = false;
                StartMovementCoroutine(target);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            DetachPlayer(collision.transform);
            if (!flip && !teleport)
            {
                StartCoroutine(ReturnToOriginAfterDelay());
            }
        }
    }

    #endregion

    #region Private Methods
    private void InitializeOrigin()
    {
        origin = transform.position;
    }

    private void CalculateTarget()
    {
        Vector3 direction = isVertical ? Vector3.up : Vector3.right;
        if (!movePositiveDirection)
        {
            direction = -direction;
        }

        target = origin + direction * distance;
    }

    private void StartMovementCoroutine(Vector3 point)
    {
        StartCoroutine(Move(point));
    }

    private IEnumerator Move(Vector3 point)
    {
        while (Vector3.Distance(point, transform.position) > 0.1f)
        {
            MoveTowardsPoint(point);
            yield return null;
        }

        if (!isReturning)
        {
            Vector3 nextPoint = point == origin ? target : origin;
            StartMovementCoroutine(nextPoint);
        }
    }

    private void MoveTowardsPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
    }

    private IEnumerator FlipPlatform()
    {
        if (playerOnPlatform != null)
        {
            DetachPlayer(playerOnPlatform);
        }

        float duration = 1f / flipSpeed; // Duration of the flip based on the flip speed
        float elapsedTime = 0f;

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, 180);

        Vector3 pivot = transform.position; // Use the platform's current position as the pivot

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        // Ensure the final rotation is exactly 180 degrees flipped
        transform.rotation = targetRotation;
        transform.position = pivot; // Maintain the position to ensure it pivots around the center

        // No movement after flip, platform remains static
    }

    private void TeleportPlayer()
    {
        if (playerOnPlatform != null)
        {
            playerOnPlatform.position = teleportDestination;
            // Play teleport sound
            if (teleportSound != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            DetachPlayer(playerOnPlatform);
        }
    }

    private IEnumerator ReturnToOriginAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        isReturning = true;
        StartMovementCoroutine(origin);
    }

    private void AttachPlayer(Transform player)
    {
        player.SetParent(transform);
        playerOnPlatform = player;
    }

    private void DetachPlayer(Transform player)
    {
        player.SetParent(null);
        playerOnPlatform = null;
    }

    #endregion
}
