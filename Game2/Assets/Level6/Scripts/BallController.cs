using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    // References
    private GameObject player;
    BallSpawner ballSpawner;
    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private GameObject enemy;
    private bool isGrabbed = false;
    private GameObject currentHolder;
    
   [Header("Pick-up behavior")]
    public float detectionRadius = 3f;
    public float heightOffset = 2f;

    [Header("Audio Settings")]
    public AudioClip destroySound;
    public AudioClip pickUpSound;
    public AudioClip hitBarrier;
    public AudioClip hitGround;
    private int hitSoundTracker;
    

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

        // Find the enemy
        enemy = GameObject.FindGameObjectWithTag("Enemy");

        // Find the ball spawner
        ballSpawner = GameObject.FindFirstObjectByType<BallSpawner>();
    }

    private float lastReleaseTime = 0f;
    private float grabCooldown = 2f; // Wait 10 second before allowing re-grab
    
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
                GrabBall(); // Use original method for player
                return; // Early return so enemy doesn't immediately steal the ball
            }
            
            // Only try enemy grab if player didn't grab the ball
            if (enemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy <= detectionRadius)
                {
                    GrabBallByEnemy(enemy); // Use new method for enemy
                }
            }
        }
    }
  else
{
    // Position the ball above whoever is holding it
    if (currentHolder != null)
    {
        Vector3 newPosition = currentHolder.transform.position;
        newPosition.y += heightOffset;
        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }
}
}
    
    private void GrabBall()
    {
        currentHolder = player;
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

        // Play sound
        if (pickUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickUpSound, 0.15f);
        }
    }

    private void GrabBallByEnemy(GameObject enemyObj)
{
    currentHolder = enemy;
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

    // Notify the enemy that it's holding the ball
    EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
    if (enemyController != null)
    {
        enemyController.SetHeldBall(this);
    }

    // Play sound
    if (pickUpSound != null && audioSource != null)
    {
        audioSource.PlayOneShot(pickUpSound, 0.15f);
    }
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
    lastReleaseTime = Time.time;
    ReleaseBall();
    rb.AddForce(force, ForceMode2D.Impulse);
}
    
    // Visualize the grab radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {

   
            StartCoroutine(ResetHitSoundTracker());
            hitSoundTracker ++;             
            // Play sound
            if (hitBarrier != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitGround, 0.25f/hitSoundTracker);
            }
    }

    private IEnumerator ResetHitSoundTracker()
    {
        yield return new WaitForSeconds(1.0f);
        hitSoundTracker = 1;
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