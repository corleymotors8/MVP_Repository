using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    // References
    private GameObject player;
    BallSpawner ballSpawner;
    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private bool isGrabbed = false;
    
   [Header("Pick-up behavior")]
    public float detectionRadius = 3f;
    public float heightOffset = 2f;

    [Header("Audio Settings")]
    public AudioClip destroySound;

    private AudioSource audioSource;
    private bool isBeingDestroyed = false;
    
    // Store original physics materials
    private PhysicsMaterial2D originalMaterial;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Store original physics material
        if (ballCollider != null)
        {
            originalMaterial = ballCollider.sharedMaterial;
        }
        
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");

        // Find the ball spawner
        ballSpawner = GameObject.FindFirstObjectByType<BallSpawner>();
    }

    private float lastReleaseTime = 0f;
    private float grabCooldown = 1f; // Wait 1 second before allowing re-grab
    
    void Update()
    {
        if (player == null) return;
        
        if (!isGrabbed)
        {
            // Only check for grab if cooldown has expired
            if (Time.time > lastReleaseTime + grabCooldown)
            {
                // Check if player is close enough to grab
                float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                
                if (distanceToPlayer <= detectionRadius)
                {
                    GrabBall();
                }
            }
        }
        else
        {
            // Position the ball above the player, maintaining Z position
            Vector3 newPosition = player.transform.position;
            newPosition.y += heightOffset;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
            
        }
    }
    
    private void GrabBall()
    {
        isGrabbed = true;
        
        // Disable physics when grabbed
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Temporarily remove physics material to disable bounce
        if (ballCollider != null)
        {
            // Either disable collider or set to trigger to prevent physics interaction
            ballCollider.isTrigger = true;
        }

        player.GetComponent<Player>().SetHeldBall(this);
    }
    
    // Optional: Method to release the ball if you need it later
    public void ReleaseBall()
    {
        isGrabbed = false;
        lastReleaseTime = Time.time; // Set the release time for cooldown
        
        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Restore original physics material
        if (ballCollider != null)
        {
            ballCollider.sharedMaterial = originalMaterial;
            ballCollider.isTrigger = false;
        }
    }

    public void ThrowWithForce(Vector2 force)
{
    ReleaseBall();
    rb.AddForce(force, ForceMode2D.Impulse);
}
    
    // Visualize the grab radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void DestroyBall()
    {
        Debug.Log("Destroying ball");
        // Only start the destroy sequence if it hasn't already started
        if (!isBeingDestroyed)
        {
            StartCoroutine(DestroySequence());
        }
    }

        private IEnumerator DestroySequence()
    {
        isBeingDestroyed = true;

        // Wait 0.6 seconds
        yield return new WaitForSeconds(0.6f);

        // Play destroy sound
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound, 0.4f);
        }

         // Notify ball spawner that the ball is destroyed
        if (ballSpawner != null)
        {
            ballSpawner.BallDestroyed();
        }

         isBeingDestroyed = false;

         //Hide ball sprite
        GetComponent<SpriteRenderer>().enabled = false;

       Invoke("DestroyBallObject", 0.6f);
    }

    private void DestroyBallObject()
    {
        Destroy(gameObject);
    }

   
}