using UnityEngine;
using System.Collections;

public class JetpackController : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] public float maxJumpForce = 20f;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float fallGravityScale = 3f;
    [SerializeField] private float normalGravityScale = 9f;

    
    private AudioSource audioSource;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip poweringUpSound;
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip thrusterSound;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minVolume = 0.2f;

    Player player;
    private Rigidbody2D rb;
    private Animator animator;
    private GameManager gameManager;
    private float chargeStartTime;
    private bool isCharging;
    private bool isGrounded;
    private bool isFalling;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Start charging when spacebar is pressed
        if (gameManager.jetpackEnabled && gameManager.playerCanMove && Input.GetButtonDown("Jump") && isGrounded)
        {
            StartCharging();
        }

        // Continue charging while holding spacebar
        if (isCharging)
        {
            UpdateCharging();
        }

        // Launch when spacebar is released
        if (Input.GetButtonUp("Jump") && isCharging)
        {
            Launch();
        }

        // Check if falling
        if (rb.linearVelocity.y < -0.1f && !isGrounded)
        {
            StartFalling();
        }
        else if (rb.linearVelocity.y >= -0.1f && isFalling)
        {

            StopFalling();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        animator.SetBool("isPoweringUp", true);
        audioSource.clip = poweringUpSound;
        audioSource.loop = true;
        audioSource.volume = minVolume;
        audioSource.Play();
    }

    private void UpdateCharging()
    {
        float chargeDuration = Mathf.Min(Time.time - chargeStartTime, maxChargeTime);
        float chargePercent = chargeDuration / maxChargeTime;
        
        // Gradually increase sound volume
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, chargePercent);
    }

    private void Launch()
    {
        
        float chargeDuration = Mathf.Min(Time.time - chargeStartTime, maxChargeTime);
        float chargePercent = chargeDuration / maxChargeTime;
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);

        // Stop charging effects
        isCharging = false;
        animator.SetBool("isPoweringUp", false);
        animator.SetBool("isLaunched", true);
        audioSource.Stop();


        // Play launch sound
        audioSource.loop = false;
        audioSource.PlayOneShot(launchSound);

        // Apply force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

   private void StartFalling()
{
    // Only start the falling sequence if we weren't already falling
    if (!isFalling)
    {
        Debug.Log("Player falling");
        isFalling = true;
        player.isFallingJetPack = true;
        animator.SetBool("isLaunched", false);
        animator.SetBool("isFalling", true);
        rb.gravityScale = fallGravityScale;
        //Debug if gravity is being set on player

        // Play thruster sound
        audioSource.clip = thrusterSound;
        audioSource.loop = false;  
        audioSource.volume = maxVolume;
        audioSource.Play();
      
    }
}

    private void StopFalling()
    {
        isFalling = false;
        player.isFallingJetPack = false;
        animator.SetBool("isFalling", false);
        animator.SetBool("isLaunched", false);  // Add this line
        animator.SetBool("isPoweringUp", false); // Add this for safety
        rb.gravityScale = normalGravityScale;
        audioSource.Stop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            StopFalling();
            animator.SetBool("isPoweringUp", false);
        }
    }
}