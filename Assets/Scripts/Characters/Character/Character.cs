using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Character : MonoBehaviour
{
    #region Variables
    [HideInInspector] public HealthController health;
    private Controller character;
    private AttackController attack;
    private Animator animator;

    [SerializeField] private float runSpeed = 40f; // Movement speed.
    [SerializeField] private bool doubleJump = true; // Enable for double jump.
    private int jumps = 0;
    private int maxJumps = 1;
    private float horizontalMove = 0f; // To what extent it moves horizontally.
    private bool isJumping = false;
    private bool isCrouching = false;
    private int currentDirection = 0; // In which direction it moves.
    #endregion

    #region Unity Methods
    private void Awake()
    {
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
        character.Move(horizontalMove * Time.fixedDeltaTime, isCrouching, isJumping);
    }

    #endregion

    #region Input Handlers

    private void HandleMovementInput()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Mathf.Abs(horizontalMove) > 0)
        {
            LevelManager.Instance.PlayWalkSound();
        }
        else
        {
            LevelManager.Instance.PlayIdleSound();
        }
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

    #endregion

    #region Public Methods

    public void Move(int dir)
    {
        if (health.isDead) return;

        // Set the new direction.
        currentDirection = dir;

        switch (dir)
        {
            default: horizontalMove = 0; break;
            case -1: horizontalMove = -runSpeed; break;
            case 1: horizontalMove = runSpeed; break;
        }
    }

    public void Jump(bool j)
    {
        if (health.isDead) return;

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

    public void OnLanding()
    {
        // Restore jumps when touching the ground.
        Jump(false);
    }

    public void OnCrouching(bool isCrouching)
    {
        // Play crouch animation while crouching.
        animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleDeath()
    {
        LevelManager.Instance.PlayDieSound();
    }
    #endregion
}
