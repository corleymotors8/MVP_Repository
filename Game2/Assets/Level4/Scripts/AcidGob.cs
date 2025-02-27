using UnityEngine;
using System.Collections;

public class AcidGob : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;
    private float maxDistance;
    private Vector3 startPosition;
    
    private Rigidbody2D rb;
    Player player;
    private Animator animator;
    private bool isExploding = false;
    private float initialInvulnerabilityTime = 0.1f; // Short time to ignore initial collisions
    private float invulnerabilityTimer = 0f;
    
    public AudioClip splashSound;
    private AudioSource audioSource;
    
    void Awake()
    {
        // Make gobs invulnerable for a short time to prevent colliding with enemy
        invulnerabilityTimer = initialInvulnerabilityTime;
        // Get or add required components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // No gravity for the projectile
        }
        
        // Get the animator component
        animator = GetComponent<Animator>();
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Make sure we have a collider
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.25f;
        }
    }

    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    public void Initialize(Vector3 dir, float spd, int dmg, float maxDist)
{
    direction = dir;
    speed = spd;
    damage = dmg;
    maxDistance = maxDist;
    startPosition = transform.position;
    
    // Make sure maxDistance is properly set
    // Debug.Log($"Initialize called with maxDist: {maxDist} - Start position: {startPosition}");
    
    // Set velocity of rigidbody
    rb.linearVelocity = direction * speed;
    
    // Force the initial flying animation state
    if (animator != null)
    {
        animator.Play("AcidFlying");
    }
}
    
void Update()
{
    // Don't update position if we're exploding
    if (isExploding)
        return;
    
    // Skip distance check for the first few frames to avoid premature explosion
    if (Time.frameCount < 5)
        return;
    
    // Only check distance if maxDistance is valid
    if (maxDistance > 0.1f)  // Use a small threshold to avoid floating point issues
    {
        float currentDistance = Vector3.Distance(startPosition, transform.position);
        if (currentDistance > maxDistance)
        {
            // Debug.Log($"Max distance exceeded: {currentDistance} > {maxDistance}");
            Explode();
        }
    }
    // Reduce invulnerability timer
    if (invulnerabilityTimer > 0)
    {
        invulnerabilityTimer -= Time.deltaTime;
    }
}
    
void OnTriggerEnter2D(Collider2D other)
{
    // Skip collisions during initial invulnerability
    if (invulnerabilityTimer > 0) return;
    
    // Prevent multiple collisions while exploding
    if (isExploding) return;
    
    // Check if we hit the player
    if (other.CompareTag("Player"))
    {
        Player player = other.GetComponent<Player>();
        
        if (player.shieldActive)
        {
            // Determine hit direction
            bool playerFacingRight = player.isFacingRight;
            bool acidGobMovingRight = rb.linearVelocity.x > 0;
            
            // Determine if hitting back/shell
            bool hittingBack = (playerFacingRight && acidGobMovingRight) || 
                              (!playerFacingRight && !acidGobMovingRight);
            
            if (hittingBack)
            {
                // Hit on back/shell - damage shield and bounce
                player.HitShield(); // Only call this when hitting the back
                
                // Check if shield is still active after being hit
                if (player.shieldActive)
                {
                    // Reflect velocity
                    Vector2 reflectedVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
                    rb.linearVelocity = reflectedVelocity;
                    return;
                }
            }
            else
            {
                // Hit on front - damage player directly, not shield
                player.TakeDamage(damage);
                Explode();
                return;
            }
        }
        
        // If we get here, either:
        // 1. Shield was never active, or
        // 2. Shield just broke from this hit
        player.TakeDamage(damage);
        Explode();
    }
    // Check for AcidSnail
    else if (other.GetComponent<AcidSnail>() != null)
    {
        AcidSnail snail = other.GetComponent<AcidSnail>();
        snail.TakeDamage(damage);
        Explode();
    }
    // Check if we hit terrain/walls
    else if (other.gameObject.layer == 8 || other.CompareTag("Ground"))
    {
        Explode();
    }
}
    
    private void Explode()
{
    // Prevent multiple explosions
    if (isExploding)
        return;
            
    isExploding = true;
    
    // Stop movement
    rb.linearVelocity = Vector2.zero;
    
    // Play splash sound if available
    if (audioSource != null && splashSound != null)
    {
        audioSource.PlayOneShot(splashSound, 0.03f);
    }
    
    // APPROACH 1: Try direct animator state control
    if (animator != null)
    {
        // Debug.Log("Forcing AcidExplode animation state");
        animator.Play("AcidExplode", 0, 0f);
        
        // Check if state transition worked
        StartCoroutine(CheckAnimationState());
    }
    
    // Destroy after a delay
    Destroy(gameObject, 0.5f);
}

private IEnumerator CheckAnimationState()
{
    yield return new WaitForSeconds(0.1f);
    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    // Debug.Log("Animation state after 0.1s: " + stateInfo.fullPathHash + 
    //           " IsName('AcidExplode'): " + stateInfo.IsName("AcidExplode"));
}

}