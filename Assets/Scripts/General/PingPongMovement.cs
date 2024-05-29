using System.Collections;
using UnityEngine;

public class PingPongMovement : MonoBehaviour
{
    #region Variables

    public Transform target; // Point to which you must go.
    public float speed = 1f;
    public bool flip = false; // Does the object spin when you come back?

    private Vector3 origin; // Point of origin

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeOrigin();
    }

    private void Start()
    {
        StartMovementCoroutine(target.position);
    }

    #endregion

    #region Private Methods

    private void InitializeOrigin()
    {
        origin = transform.position;
    }

    private void StartMovementCoroutine(Vector3 point)
    {
        StartCoroutine(Move(point));
    }

    private IEnumerator Move(Vector3 point)
    {
        while (Vector3.Distance(point, transform.position) > 1)
        {
            MoveTowardsPoint(point);
            yield return null;
        }

        Flip();

        Vector3 nextPoint = point == origin ? target.position : origin;
        StartMovementCoroutine(nextPoint);
    }

    private void MoveTowardsPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (!flip) return;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    #endregion
}
