using UnityEngine;
using System.Collections;

public class JetpackController : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float maxJumpForce = 20f;
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

    private Rigidbody2D rb;
    private Animator animator;
    private GameManager gameManager;
    private float chargeStartTime;
    private bool isCharging;
    private bool isGrounded;
    private bool isFalling;

    private void Start()
    {
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
            Debug.Log("Jump button pressed");
            StartCharging();
            Debug.Log("Charging");
        }

        // Continue charging while holding spacebar
        if (isCharging)
        {
            UpdateCharging();
        }

        // Launch when spacebar is released
        if (Input.GetButtonUp("Jump") && isCharging)
        {
            Debug.Log("Launching");
            Launch();
        }

        // Check if falling
        if (rb.linearVelocity.y < -0.1f && !isGrounded)
        {
            Debug.Log("Start falling");
            StartFalling();
        }
        else if (rb.linearVelocity.y >= -0.1f && isFalling)
        {
            Debug.Log("Stop falling");
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
        Debug.Log($"Launch - isPoweringUp: {animator.GetBool("isPoweringUp")}, isLaunched: {animator.GetBool("isLaunched")}");


        // Play launch sound
        audioSource.loop = false;
        audioSource.PlayOneShot(launchSound);

        // Apply force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void StartFalling()
    {
        Debug.Log("Player falling");
        isFalling = true;
        animator.SetBool("isLaunched", false);
        animator.SetBool("isFalling", true);
        rb.gravityScale = fallGravityScale;

        // Play thruster sound
        audioSource.clip = thrusterSound;
        audioSource.loop = true;
        audioSource.volume = maxVolume;
        audioSource.Play();
    }

    private void StopFalling()
    {
        isFalling = false;
        animator.SetBool("isFalling", false);
        animator.SetBool("isLaunched", false);  // Add this line
        animator.SetBool("isPoweringUp", false); // Add this for safety
        Debug.Log($"StopFalling - isPoweringUp: {animator.GetBool("isPoweringUp")}, isLaunched: {animator.GetBool("isLaunched")}, isFalling: {animator.GetBool("isFalling")}");
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