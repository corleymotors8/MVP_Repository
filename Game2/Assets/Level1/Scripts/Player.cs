using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour, IDamageable
{
    // Audio Clips
    private AudioSource audioSource; // Reference to the AudioSource
    public AudioClip landSound;
    public AudioClip jumpSound;
    public AudioClip deathSound;
    public AudioClip stompSound;
    public AudioClip offLadder;
    public AudioClip hitByEnemy;
    public AudioClip[] footsteps;
    public AudioClip[] climbing;
    public AudioClip[] attackSounds;
    public AudioClip moveSound; // Assign your footstep sounds in the Inspector
    public float moveSoundVolume = 1.0f; // Public variable to control volume (default is max: 1.0)
    public float jumpVolume = 1.0f; // Public variable to control volume (default is max: 1.0)
    public float landVolume = 1.0f; // Public variable to control volume (default is max: 1.0)

    // To make climbing sounds less frequent
    private float lastPlayTime = 0f;
    private float cooldownDuration = 0.4f; // Adjust this to match your animation length


    //Editable state variables
    public float speed = 10.0f;
    public float jumpForce = 5.0f;
    public float doubleJumpForce = 5.0f;
    public float attackCooldown = 0.5f; // Time between attacks
    [Header("Health")]
    public int maxHealth = 10; // Set enemy's health in Inspector
    private int currentHealth;
   

   //Fixed state variables
    [HideInInspector]
    public float horizontalInput;
   [HideInInspector]
    public float verticalInput;
    [HideInInspector]
    public bool isFacingRight = true;
    public bool isGrounded = true;
    private bool isJumping = false;
    [HideInInspector]
    public bool isWalking = false;
     [HideInInspector]
    public bool isClimbing = false;
     [HideInInspector]
    public bool canClimb = false;
     [HideInInspector]
    public bool isAttacking = false;

    [HideInInspector]
    public int jumpCount = 0;
    private bool previousFallingState;
    public bool isFalling = false;
    public float fallThreshold = -0.1f;  // Threshold for detecting falling
    public bool isImmune = false; // Give player immunity when respawn
    public bool preventRespawn = false;
    
    // For bat riding
    [HideInInspector]
    public bool onBat = false;
    public float batSpeed = 5.0f;

    // Respawn 
     private Vector3 respawnPosition;
     private Vector3 startingPosition; // For debugging, to instantly respawn player

    // For staying with moving platform
    private Transform platformTransform = null; // Only created when player lands on platform
    private Vector3 previousPlatformPosition;

    // Other
    Animator animator;
    private Rigidbody2D rb;
    private Rigidbody2D currentBatRb;

    private LevelManager levelManager;  // Access LevelManager
    private GameManager gameManager;
  
   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
       gameManager = FindFirstObjectByType<GameManager>();  // Find and assign GameManager
       levelManager = FindFirstObjectByType<LevelManager>();  // Find and assign LevelManager
       startingPosition = transform.position; // for debugging
       currentHealth = maxHealth; // Start at full health
      
       
       animator = GetComponent<Animator>();
       animator.SetBool("isFalling", false);
       rb = GetComponent<Rigidbody2D>();
       audioSource = GetComponent<AudioSource>();      
   }

   // Update is called once per frame
   void Update()
 {
    //Insant player respawn button for debugging if / key hit
    if (Input.GetKeyDown(KeyCode.Slash))
    {
        transform.position = startingPosition;
    }

    // Respawn player if they fall out of camera view
    if (!preventRespawn && transform.position.y < Camera.main.transform.position.y - Camera.main.orthographicSize)
    {
        GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
    
        if (gameManager != null)
        {
        gameManager.PlayerDied();
        Debug.Log("Player fell out of camera view");
        }

    }

    // Handle left-right & up-down movement
    horizontalInput = Input.GetAxis("Horizontal");
    verticalInput = Input.GetAxis("Vertical");  

     // Single jump (not rideable enemy)
    if (gameManager.playerCanJump && gameManager.playerCanMove && Input.GetButtonDown("Jump") && isGrounded && jumpCount == 0)
        {
            // Check jump force applied, jump height and player position before and after jump
            // Debug.Log("Jumping with force: " + jumpForce + " at position: " + transform.position);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            isJumping = true;
            ++ jumpCount;
            animator.SetBool("isJumping", !isGrounded);
            animator.SetBool("isGrounded", false);
        //  audioSource.PlayOneShot(jumpSound, jumpVolume); // Play the sound
        }

    // Double jump (not on rideable enemy)
    else if (gameManager.playerCanDoubleJump && Input.GetButtonDown("Jump") && !isGrounded && jumpCount == 1)
    {
        // Debug.Log("Double jumping with force: " + doubleJumpForce + " at position: " + transform.position);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
        audioSource.PlayOneShot(jumpSound, jumpVolume); // Play the sound
        ++ jumpCount;
    }

    // Attacking
    if (gameManager.playerCanAttack && Input.GetKeyDown(KeyCode.B))
    {
        StartCoroutine(Attack());
        
    }

    // *** Normal player movement *** ///
       
        // Walking on ground
        if (!canClimb & gameManager.playerCanMove) // Not on ladder
        {
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            isWalking = true;

        }
        else
        {
            isWalking = false;
        }
       

        if (canClimb) // On ladder
        {
        rb.linearVelocity = new Vector2(horizontalInput * speed/2, verticalInput * (speed/1.5f)); // Slow horizontal movement if on ladder
        // Reduce gravity on player so doesn't slide down ladder
        rb.gravityScale = 0;
        isClimbing = true;
        }
        else
        {
        rb.gravityScale = 9;
        isClimbing = false;
        }

        if (canClimb && verticalInput != 0)
        {
        animator.SetBool("isClimbing", true);
        }

        
    // Flip character animation

        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

     // Check if the player is falling based on velocity
       if (rb.linearVelocity.y < -0.05f)
        {
         isFalling = true;
        }
        else
        {
        isFalling = false;
}

    // Keep player on platform
     if (platformTransform != null)
        {
            // Calculate how much the platform has moved since the last frame
            Vector3 platformMovement = platformTransform.position - previousPlatformPosition;

            // Move the player by the same amount
            transform.position += platformMovement;

            // Update the previous platform position
            previousPlatformPosition = platformTransform.position;
        }
}

IEnumerator Attack()
{
    Debug.Log("Attack started");
    if (isAttacking) 
    {
        Debug.Log("Attack blocked - already attacking");
        yield break;
    }
    
    PlayAttackSound();
    isAttacking = true;
    animator.SetBool("isAttacking", true);
    
    yield return new WaitForSeconds(attackCooldown);

    isAttacking = false;
    animator.SetBool("isAttacking", false);
}

private void Flip()
{
    isFacingRight = !isFacingRight;
    Vector3 scale = transform.localScale;
    scale.x *= -1; // Flip the x-axis
    transform.localScale = scale;
}

/// Sounds ///

public void PlayFootstepSound() 
{
		if (footsteps.Length > 0) 
        {
            AudioClip clip = footsteps[UnityEngine.Random.Range(0, footsteps.Length)];
			audioSource.PlayOneShot(clip, 0.03f);
		}
}

public void ClimbingSound() 
{
	if (Time.time - lastPlayTime >= cooldownDuration)
    {
        if (climbing.Length > 0) 
        {
            AudioClip clip = climbing[UnityEngine.Random.Range(0, climbing.Length)];
			audioSource.PlayOneShot(clip, 0.1f);
            lastPlayTime = Time.time;
		}
    }
}

public void PlayAttackSound()
{
    if (attackSounds.Length > 0)
    {
        AudioClip clip = attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)];
        audioSource.PlayOneShot(clip, 0.1f);
    }
}

 
   private void FixedUpdate()
   {
    animator.SetFloat("climbSpeed", verticalInput);
    
    //Trigger move animation (xVelocity) and jump animation (yVelocity)
      animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
      animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }
   
    /// Ladder climbing
   private void OnTriggerEnter2D(Collider2D other)
   {
       if (other.CompareTag("Climbable"))
       {
        //    Debug.Log("Player is on ladder");
           canClimb = true;
       }
    
    
    //*** MORE PRECISE WAY TO HANDLE JUMPING TRANSITIONS BUT BUGGY ***////
    //    if (other.CompareTag("Ground"))
    //    {
    //     isGrounded = true;
    //     animator.SetBool("isJumping", false);
    //    }
   }
       


   private void OnTriggerExit2D(Collider2D other)
   {
       if (other.CompareTag("Climbable"))
       {
        //    Debug.Log("Player is off ladder");
           canClimb = false;
           animator.SetBool("isClimbing", false);
        // Play off ladder sound
        // Only play half duration
        // audioSource.PlayOneShot(offLadder, 0.2f);

       }
   }

   // Grounding function for jumps
   private void OnCollisionEnter2D(Collision2D collision)
    {
    Vector2 relativePosition = transform.position - collision.transform.position; // check if player is above the enemy
  
        // Jump Detection
       if (collision.gameObject.CompareTag("Ground"))
       {
           isGrounded = true;
           animator.SetBool("isGrounded", true);
           animator.SetBool("isJumping", false);
           jumpCount = 0;
           isJumping = false;
           isFalling = false;
           audioSource.PlayOneShot(landSound, landVolume); // Play the sound
       }
    }

    public void TakeDamage(int damage)
    {
        //Play sound
        audioSource.PlayOneShot(hitByEnemy, 0.4f);
        
        // Handle taking damage
        currentHealth -= damage;

        if (currentHealth <= 0)
            {
            GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                gameManager.PlayerDied();
                }
            }
        Debug.Log("Player took damage. Health left: " + currentHealth);
    }

    public void PlayerHitByEnemy()
{
    // Notify the Game Manager to handle player death 
    GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
    if (gameManager != null)
    {
        gameManager.PlayerDied();
    }

    // Make player invisible
    GetComponent<SpriteRenderer>().enabled = false;
}

}
