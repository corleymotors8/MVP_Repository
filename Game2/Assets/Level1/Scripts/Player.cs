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
    public AudioClip deathSound;
    public AudioClip stompSound;
    public AudioClip offLadder;
    public AudioClip hitByEnemy;
    public AudioClip shieldBounce;
    public AudioClip shieldBreak;
    public AudioClip shieldRestore;
    public AudioClip[] footsteps;
    public AudioClip[] climbing;
    public AudioClip[] attackSounds;
    public AudioClip throwSound;
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
    [HideInInspector]
    public int currentHealth;
    public int maxShieldHealth;
    [HideInInspector]
    public int currentShieldHealth;
    private bool isJumping;
    [HideInInspector]
   
   // Shield variables and methods
    public bool shieldActive = true;
    private bool isRegenerating = false;
    public float shieldRegenerationRate = 0.25f; // Regenerate 1 shield point per second (adjustable)
    private Coroutine shieldRegenerationCoroutine = null; // Reference to store the active coroutine
   

   //Fixed state variables
    [HideInInspector]
    public float horizontalInput;
   [HideInInspector]
    public float verticalInput;
    [HideInInspector]
    public bool isFacingRight = true;
    public bool isGrounded = true;
    [HideInInspector]
    public bool isWalking = false;
     [HideInInspector]
    public bool isClimbing = false;
     [HideInInspector]
    public bool canClimb = false;
     [HideInInspector]
    public bool isAttacking = false;
    private bool isOnLadder = false;
    private bool isActivelyClimbing = false;

    [HideInInspector]
    public int jumpCount = 0;
    private bool previousFallingState;
    public bool isFalling = false;
    public bool isFallingJetPack = false;
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

    //Ball throwing
    private BallController heldBall;
    public float throwForce = 10f;
    public float throwAngle = 45f; // Degrees

    // Other
    public Animator animator;
    private Rigidbody2D rb;
    private Rigidbody2D currentBatRb;
    private SpriteRenderer crabSprite; // Use until we get proper shield animation
    private Color originalColor; // Use until we get proper shield animation

    private LevelManager levelManager;  // Access LevelManager
    private GameManager gameManager;
  
   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
       currentShieldHealth = maxShieldHealth;

       //Placeholder until proper shield animation
       crabSprite = GetComponent<SpriteRenderer>(); // Automatically get the SpriteRenderer
    if (crabSprite != null)
    {
        originalColor = crabSprite.color; // Store the original color
    }
       
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
        }

    }




    // Attacking
    if (gameManager.playerCanAttack && Input.GetKeyDown(KeyCode.B))
    {
        StartCoroutine(Attack());
        
    }

    if (gameManager.playerCanMove && Input.GetKeyDown(KeyCode.H) && heldBall != null)
{
    ThrowBall();
    audioSource.PlayOneShot(throwSound, 0.3f);
}
       
    // Walking on ground
  // Only read input if player can move
    if (gameManager.playerCanMove)
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");  
        
        if (!canClimb) // Not on ladder
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
    }
    else
    {
        // Reset input values when player can't move
        horizontalInput = 0;
        verticalInput = 0;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isWalking = false;
    }
       
   // Climbing
        if (canClimb) // On ladder
        {
            if (!isOnLadder)
            {
                // Just got on ladder
                isOnLadder = true;
                rb.gravityScale = 0;
            }

            // Handle climbing movement
            rb.linearVelocity = new Vector2(horizontalInput * speed/1.5f, verticalInput * (speed/1.25f));
            
            // Only set climbing states when there's actual vertical movement
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                if (!isActivelyClimbing)
                {
                    isActivelyClimbing = true;
                    isClimbing = true;
                    animator.SetBool("isClimbing", true);
                }
            }
            else
            {
                // Player is on ladder but not moving vertically
                isActivelyClimbing = false;
                // Don't set isClimbing to false here to maintain the blend tree state
            }
        }
        else
        {
            if (isOnLadder)
            {
                // Just got off ladder
                isOnLadder = false;
                isActivelyClimbing = false;
                isClimbing = false;
                animator.SetBool("isClimbing", false);
                
                if (!isFallingJetPack)
                {
                    rb.gravityScale = 9;
                }
            }
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
    // Update the blend tree parameter only when actively climbing
        if (isActivelyClimbing)
        {
            animator.SetFloat("climbSpeed", verticalInput);
        }
    
    //Trigger move animation (xVelocity) and jump animation (yVelocity)
      animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
      animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }
   
    /// Ladder climbing
   private void OnTriggerEnter2D(Collider2D other)
   {
       if (other.CompareTag("Climbable"))
       {
           Debug.Log("Player is on ladder");
           canClimb = true;
       }
   }
       
   private void OnTriggerExit2D(Collider2D other)
   {
       if (other.CompareTag("Climbable"))
       {
        //    Debug.Log("Player is off ladder");
           canClimb = false;
           animator.SetBool("isClimbing", false);
           Debug.Log("Player is off ladder");
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
           jumpCount = 0;
           isJumping = false;
           isFalling = false;
           audioSource.PlayOneShot(landSound, landVolume); // Play the sound
       }

       // Death platforms (to be added later, for now just red platforms)
         if (collision.gameObject.CompareTag("DeathPlatform"))
         {
              GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
              if (gameManager != null)
              {
                gameManager.PlayerDied();
              }
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

/// *** Shield behavior *** ///

   public void HitShield()
{
    // Play shield bounce sound
    audioSource.PlayOneShot(shieldBounce, 0.5f);
    --currentShieldHealth;
    Debug.Log("Shield hit. Current shield health: " + currentShieldHealth);
    
    // Start regeneration if not already regenerating
    if (!isRegenerating && currentShieldHealth > 0)
    {
        StartShieldRegeneration();
    }
    
    if (currentShieldHealth <= 0)
    {
        // Play shield break sound
        Debug.Log("Shield broken");
        crabSprite.color = Color.red; // Change to red
        
        // Stop any current regeneration
        StopShieldRegeneration();
        
        audioSource.PlayOneShot(shieldBreak, 0.9f);
        //Shield is broken, disable it and start recovery
        shieldActive = false;
        StartCoroutine(RestoreShield());
    }
}

private void StartShieldRegeneration()
{
    // Stop any existing regeneration before starting a new one
    StopShieldRegeneration();
    
    // Start the regeneration coroutine and store the reference
    shieldRegenerationCoroutine = StartCoroutine(RegenerateShield());
}

private void StopShieldRegeneration()
{
    if (shieldRegenerationCoroutine != null)
    {
        StopCoroutine(shieldRegenerationCoroutine);
        shieldRegenerationCoroutine = null;
    }
    isRegenerating = false;
}

private IEnumerator RegenerateShield()
{
    isRegenerating = true;
    
    while (currentShieldHealth < maxShieldHealth && shieldActive)
    {
        // Wait for regeneration interval
        yield return new WaitForSeconds(1f / shieldRegenerationRate);
        
        // Increase shield health by 1
        currentShieldHealth++;
        Debug.Log("Shield regenerated. Current shield health: " + currentShieldHealth);
        
        // Optional: Add a small visual or sound effect for regeneration
        // You could add a subtle pulse effect or quiet sound here
    }
    
    isRegenerating = false;
    shieldRegenerationCoroutine = null;
}

    private IEnumerator RestoreShield()
{
    yield return new WaitForSeconds(5f);
    
    // Restore shield to max health
    currentShieldHealth = maxShieldHealth; 
    shieldActive = true;
    crabSprite.color = originalColor; // Restore original color
    audioSource.PlayOneShot(shieldRestore, 0.9f);
    Debug.Log("Shield restored with 1 health point");
    
}

// 7. Add cleanup in OnDisable to prevent coroutine errors
private void OnDisable()
{
    StopShieldRegeneration();
}

// *** THROWING BALLS *** /// 
public void SetHeldBall(BallController ball) { heldBall = ball; }

public void ThrowBall()
{
    if (heldBall != null)
    {
        // Calculate throw direction based on player facing
        Vector2 throwDirection = isFacingRight ? Vector2.right : Vector2.left;
        
        // Apply an upward angle
        Vector2 newThrowForce = new Vector2(throwDirection.x, 1f).normalized * throwForce;
        
        // Tell the ball to release itself and apply the force
        heldBall.ThrowWithForce(newThrowForce);
        
        // Clear the reference
        heldBall = null;
    }
}




}
