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
    private Animator animator;
    private bool isExploding = false;
    
    public AudioClip splashSound;
    private AudioSource audioSource;
    
    void Awake()
    {
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
    
  public void Initialize(Vector3 dir, float spd, int dmg, float maxDist)
{
    direction = dir;
    speed = spd;
    damage = dmg;
    maxDistance = maxDist;
    startPosition = transform.position;
    
    // Make sure maxDistance is properly set
    Debug.Log($"Initialize called with maxDist: {maxDist} - Start position: {startPosition}");
    
    // Set velocity of rigidbody
    rb.linearVelocity = direction * speed;
    
    // Force the initial flying animation state
    if (animator != null)
    {
        animator.Play("AcidFlying", 0);
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
            Debug.Log($"Max distance exceeded: {currentDistance} > {maxDistance}");
            Explode();
        }
    }
}
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent multiple collisions while exploding
        if (isExploding)
            return;
            
        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Call the damage function on the player
                player.TakeDamage(damage); // Or custom damage method if player has one
                Debug.Log("Acid gob hit player for " + damage + " damage");
            }
            
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
        audioSource.PlayOneShot(splashSound);
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