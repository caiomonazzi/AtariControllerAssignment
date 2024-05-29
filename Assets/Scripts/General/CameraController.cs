using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform target;
    [SerializeField] private float speed = 10f;

    private Vector3 cameraPosition;

    #endregion

    #region Unity Methods

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        ApplyCameraPosition();
    }

    #endregion

    #region Private Methods

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
