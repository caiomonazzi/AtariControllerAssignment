using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Rigidbody2DParameters
{
    public RigidbodyType2D bodyType;
    public PhysicsMaterial2D material;
    public bool simulated;
    public bool useAutoMass;
    public float mass;
    public float linearDrag;
    public float angularDrag;
    public float gravityScale;
    public CollisionDetectionMode2D collisionDetectionMode;
    public RigidbodyInterpolation2D interpolation;
    public RigidbodyConstraints2D constraints;
    public bool isKinematic;
}

public class Controller : MonoBehaviour
{
    #region Variables

    [SerializeField] private float m_JumpForce = 400f; // Amount of force added when the player jumps.
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f; // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f; // How much to smooth out the movement

    [SerializeField] private bool m_AirControl = false; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck; // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck; // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider; // A collider that will be disabled when crouching

    private Rigidbody2DParameters originalRigidbody2DParameters;
    private Rigidbody2D m_Rigidbody2D;

    public bool onLadder = false;

    private float m_delayGroundCheck = 0.25f;
    private const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded; // Whether or not the player is grounded.
    private const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up

    private Vector3 m_Velocity = Vector3.zero;
    private float timeBeforeGroundCheck = 0f;

    #endregion

    #region Events

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnCrouchEvent;

    private bool m_wasCrouching = false;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        StoreRigidbody2DParameters();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }


    private void Update()
    {
        if (!m_Grounded)
        {
            timeBeforeGroundCheck -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (timeBeforeGroundCheck > 0f) return;

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    #endregion

    #region Public Methods
    public Rigidbody2DParameters GetOriginalRigidbody2DParameters()
    {
        return originalRigidbody2DParameters;
    }

    public void Move(float move, bool crouch, bool jump)
    {
        HandleCrouch(ref crouch);

        // Only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            HandleMovement(move, crouch);
        }

        // If the player should jump...
        if (m_Grounded && jump)
        {
            Jump();
        }
    }

    public void Jump()
    {
        m_Grounded = false;

        // Add a vertical force to the player.
        m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));

        timeBeforeGroundCheck = m_delayGroundCheck;
    }

    #endregion

    #region Private Methods
    private void StoreRigidbody2DParameters()
    {
        originalRigidbody2DParameters.bodyType = m_Rigidbody2D.bodyType;
        originalRigidbody2DParameters.material = m_Rigidbody2D.sharedMaterial;
        originalRigidbody2DParameters.simulated = m_Rigidbody2D.simulated;
        originalRigidbody2DParameters.useAutoMass = m_Rigidbody2D.useAutoMass;
        originalRigidbody2DParameters.mass = m_Rigidbody2D.mass;
        originalRigidbody2DParameters.linearDrag = m_Rigidbody2D.drag;
        originalRigidbody2DParameters.angularDrag = m_Rigidbody2D.angularDrag;
        originalRigidbody2DParameters.gravityScale = m_Rigidbody2D.gravityScale;
        originalRigidbody2DParameters.collisionDetectionMode = m_Rigidbody2D.collisionDetectionMode;
        originalRigidbody2DParameters.interpolation = m_Rigidbody2D.interpolation;
        originalRigidbody2DParameters.constraints = m_Rigidbody2D.constraints;
        originalRigidbody2DParameters.isKinematic = m_Rigidbody2D.isKinematic;
    }


    private void HandleCrouch(ref bool crouch)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }
    }

    private void HandleMovement(float move, bool crouch)
    {
        // If crouching
        if (crouch)
        {
            if (!m_wasCrouching)
            {
                m_wasCrouching = true;
                OnCrouchEvent.Invoke(true);
            }

            // Reduce the speed by the crouchSpeed multiplier
            move *= m_CrouchSpeed;

            // Disable one of the colliders when crouching
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = false;
        }
        else
        {
            // Enable the collider when not crouching
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = true;

            if (m_wasCrouching)
            {
                m_wasCrouching = false;
                OnCrouchEvent.Invoke(false);
            }
        }

        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        // And then smoothing it out and applying it to the character
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }

    #endregion
}
