using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Character : MonoBehaviour
{
    #region Variables
    [HideInInspector] public HealthController health;
    [HideInInspector] private WeaponController weaponController;

    private Controller character;
    private AttackController attack;
    private Animator animator;

    [SerializeField] public float runSpeed = 20f; // Movement speed.
    [SerializeField] public float walkSpeed = 10f; // Movement speed.
    public float originalRunSpeed;
    public float originalWalkSpeed;
    [SerializeField] private float staminaTime = 100f; // Run stamina time.
    [SerializeField] private bool doubleJump = true; // Enable for double jump.
    [SerializeField] public Transform attackPoint; // Reference to the attack point
    private bool canClim = false;
    private int jumps = 0;
    private int maxJumps = 1;
    private float horizontalMove = 0f; // To what extent it moves horizontally.
    public bool isFacingRight = true; // For determining which way the player is currently facing.
    public bool isJumping = false;
    private bool isCrouching = false;
    private bool isRunning = false;
    private bool isClimbing = false;
    private float verticalMove = 0f; // To what extent it moves vertically.

    private Rigidbody2D m_Rigidbody2D;
    private Vector2 m_Velocity = Vector2.zero;
    private float m_MovementSmoothing = .05f;

    private int currentDirection = 0; // In which direction it moves.

    private float lastRunKeyPressTime = 0f;
    bool pressedRunFirstTime = false;
    private float staminaRunningTime = 0f;
    private const float doubleKeyPressDelay = .25f;


    #endregion

    #region Unity Methods
    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        originalRunSpeed = runSpeed;
        originalWalkSpeed = walkSpeed;

        character = GetComponent<Controller>();
        health = GetComponent<HealthController>();
        attack = GetComponent<AttackController>();
        animator = GetComponent<Animator>();

        // If double jump is allowed, increase the maximum number of jumps.
        if (doubleJump) maxJumps = 2;

        // Subscribe to death event
        health.OnDeath += HandleDeath;
    }

    // Get all the inputs.
    private void Update()
    {
        if (health.isDead) return;

        HandleMovementInput();
        HandleJumpInput();
        HandleAttackInput();
        HandleCrouchInput();
    }

    private void FixedUpdate()
    {
        // If attacking or dead, do not move.
        if (attack.isAttacking || health.isDead)
        {
            character.Move(0, false, false);
            return;
        }

        // Move character.
        if (isClimbing)
        {
            Climb(verticalMove * Time.fixedDeltaTime);
        }
        else
        {
            character.Move(horizontalMove * Time.fixedDeltaTime, isCrouching, isJumping);
        }
    }
    #endregion

    #region Input Handlers
    private void HandleMovementInput()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (pressedRunFirstTime)
            {
                // A key was already pressed, checking for double press
                if (Time.time - lastRunKeyPressTime <= doubleKeyPressDelay)
                {
                    // Key was pressed twice in the desired delay
                    pressedRunFirstTime = false;
                    if (!isRunning)
                    {
                        isRunning = true;
                        staminaRunningTime = Time.time;
                    }
                }
            }
            else
            {
                // Key is pressed for the first time
                pressedRunFirstTime = true;
            }

            // Updating last time that the key was pressed
            lastRunKeyPressTime = Time.time;
        }
        else if (isRunning && (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)))
        {
            // A key was released while running, cancelling it
            isRunning = false;
        }

        // Waiting for double key press but reached the delay, restarting the double press logic
        if (pressedRunFirstTime && Time.time - lastRunKeyPressTime > doubleKeyPressDelay)
        {
            pressedRunFirstTime = false;
        }

        // Checking to see if the player stamina timed out while running
        if (isRunning && ((Time.time - staminaRunningTime) > staminaTime))
        {
            // Stamina timeout, player is not running anymore
            isRunning = false;
        }

        // Move the char at the selected speed
        float speed = isRunning ? runSpeed : walkSpeed;
        horizontalMove = horizontalMove * speed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Mathf.Abs(horizontalMove) != 0)
        {
            LevelManager.Instance.PlayWalkSound();
        }
        else
        {
            LevelManager.Instance.PlayIdleSound();
        }

        // Check and change attackpoint direction if needed
        HandleFlip(horizontalMove);

    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Jump(true);
        }
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Attack(true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            Attack(false);
        }
    }

    private void HandleCrouchInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Crouch(true);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            Crouch(false);
        }
    }


    private void HandleFlip(float move)
    {
        // If the input is moving the player right and the player is facing left...
        if (move > 0 && !isFacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (move < 0 && isFacingRight)
        {
            // ... flip the player.
            Flip();
        }
    }

    private void Flip()
    {


        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Log the scale before flipping
        Debug.Log("Character scale before flip: " + transform.localScale);

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // Log the scale after flipping
        Debug.Log("Character scale after setting: " + transform.localScale);

        // Flip the attackPoint
        if (attackPoint != null)
        {
            Debug.Log("Before Flip: attackPoint local scale: " + attackPoint.localScale);

            // Multiply the attackPoint's x local scale by -1.
            Vector3 attackScale = attackPoint.localScale;
            attackScale.x *= -1;
            attackPoint.localScale = attackScale;

            // Log the scale after flipping
            Debug.Log("After Flip: attackPoint local scale: " + attackPoint.localScale);
        }
    }

    #endregion

    #region Public Methods

    public void Move(int dir)
    {
        if (health.isDead) return;

        // Set the new direction.
        currentDirection = dir;
        float speed = isRunning ? runSpeed : walkSpeed;

        switch (dir)
        {
            default: horizontalMove = 0; break;
            case -1: horizontalMove = -speed; break;
            case 1: horizontalMove = speed; break;
        }
    }

    public void Jump(bool j)
    {
        if (health.isDead) return;

        // Ensure maxJumps is set according to doubleJump setting
        maxJumps = doubleJump ? 2 : 1;

        // If attempting to jump and haven't reached max jumps...
        if (j && jumps < maxJumps)
        {
            jumps++;

            LevelManager.Instance.PlayJumpSound();
            // Add vertical force if it's not the first jump.
            if (jumps > 1)
            {
                character.Jump();
            }
            else
            {
                animator.Play("Jump");
            }
        }
        // If stopping the jump and currently jumping.
        else if (!j && isJumping)
        {
            jumps = 0;
        }

        isJumping = j;

        // Animator handles animations based on jump count.
        animator.SetInteger("Jumps", jumps);
    }

    public void Crouch(bool c)
    {
        if (health.isDead) return;

        // Update crouch state.
        isCrouching = c;
    }

    public void Attack(bool a)
    {
        if (health.isDead) return;

        // Communicate with the attack controller to attack.
        attack.Attack(a);
        if (a)
        {
            LevelManager.Instance.PlayAttackSound();
        }
    }

    public void Climb(float move)
    {
        // Apply vertical movement
        Vector2 targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move);
        m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        // Update the animator
        animator.SetFloat("ClimbSpeed", Mathf.Abs(move));
    }


    public void OnLanding()
    {
        // Restore jumps when touching the ground.
        Jump(false);
        isJumping = false; // Ensure jumping state is reset
    }
    public Rigidbody2DParameters GetOriginalRigidbody2DParameters()
    {
        return character.GetOriginalRigidbody2DParameters();
    }

    public void OnCrouching(bool isCrouching)
    {
        // Play crouch animation while crouching.
        animator.SetBool("IsCrouching", isCrouching);
    }

    public void AllowClimbing(bool allow)
    {
  //      canClimb = allow;
        isClimbing = false;

        if (allow)
        {
            // Enable climbing behavior
            // For example, disable gravity
            character.GetComponent<Rigidbody2D>().gravityScale = 0;
        }
        else
        {
            // Disable climbing behavior
            // Restore gravity
            character.GetComponent<Rigidbody2D>().gravityScale = character.GetOriginalRigidbody2DParameters().gravityScale;
        }
    }

    private void HandleDeath()
    {
        LevelManager.Instance.PlayDieSound();
    }
    #endregion
}
