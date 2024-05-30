using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform target;
    [SerializeField] private float speed = 10f;

    private Vector3 cameraPosition;

    #endregion

    #region Unity Methods

    private void Start()
    {
        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            FindPlayer();
        }

        if (target != null)
        {
            UpdateCameraPosition();
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            ApplyCameraPosition();
        }
    }

    #endregion

    #region Private Methods

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player character not found. Please assign the target in the CameraController.");
        }
    }

    private void UpdateCameraPosition()
    {
        cameraPosition = new Vector3(
            Mathf.SmoothStep(transform.position.x, target.transform.position.x, speed * Time.fixedDeltaTime),
            Mathf.SmoothStep(transform.position.y, target.transform.position.y, speed * Time.fixedDeltaTime));
    }

    private void ApplyCameraPosition()
    {
        transform.position = cameraPosition + Vector3.forward * -10;
    }

    #endregion
}