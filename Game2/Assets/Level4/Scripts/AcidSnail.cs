using UnityEngine;
using System.Collections;

public class AcidSnail : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 1;
    public int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackFromDistance = 5f;
    public float moveAwayDistance = 3f;  // Maximum distance to move away after shooting

    [Header("Shooting")]
    public GameObject acidGobPrefab;
    public Transform shootPoint;
    public float gobSpeed = 5f;
    public int gobDamage = 1;
    public int shootHowMany = 3;
    public float timeBetweenShots = 0.5f;
    public AudioClip shootSound;

    [Header("Platform Boundaries")]
    public Transform platformTransform; // Drag the platform transform in the inspector
    private float leftBoundary;
    private float rightBoundary;
    private bool hasLoggedNearBoundary = false;
    private float lastYPosition = 0f;

    // State tracking
    public bool shouldMove = false;
    [HideInInspector]
    public bool isMovingAway = false;
    private bool isShooting = false;
    private int shotsRemaining = 0;
    private GameObject player;
    private Rigidbody2D rb;
    private Animator animator;
    EnemySpawner enemySpawner;
    private AudioSource audioSource;
    public Vector3 targetMoveAwayPosition;
    private Camera mainCamera;
    public float offscreenMargin = -2f; // How far below screen to check
    private bool isFallingDestroyed = false; // Flag to prevent multiple calls

    public AcidSnailSpawner spawner;


    // Sounds
    public AudioClip deathSound;
    public AudioClip snailFall;
    
    void Start()
    {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        spawner = FindFirstObjectByType<AcidSnailSpawner>();
        mainCamera = Camera.main;

         // Calculate platform boundaries based on the platform's collider
        if (platformTransform != null)
        {
            Collider2D platformCollider = platformTransform.GetComponent<Collider2D>();
            if (platformCollider != null)
            {
                leftBoundary = platformCollider.bounds.min.x;
                rightBoundary = platformCollider.bounds.max.x;
            }
        }
       
       GetComponent<Collider2D>().enabled = true;
       
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }


// void Update()
// {
//     // Skip check if already being destroyed
//     if (isFallingDestroyed) return;
    
//     // Store position for debugging
//     lastYPosition = transform.position.y;
    
//     // Check if the object has fallen below the bottom of the screen
//     if (mainCamera != null && rb.linearVelocity.y < 0)
//     {
//         Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        
//         // Debug logging
//         if (viewportPosition.y < 0.1f && !hasLoggedNearBoundary)
//         {
//             // Debug.Log($"Snail approaching boundary: ViewportY={viewportPosition.y}, WorldY={transform.position.y}, Velocity={rb.linearVelocity.y}");
//             hasLoggedNearBoundary = true;
//         }
        
//         // Check if below screen with reduced margin
//         // Try a much smaller margin first to see if detection improves
//         if (viewportPosition.y < 0)  // Changed from -offscreenMargin to just 0
//         {
//             // Debug.Log($"Snail detected as fallen: ViewportY={viewportPosition.y}, WorldY={transform.position.y}, Velocity={rb.linearVelocity.y}");
//             isFallingDestroyed = true; // Set flag to prevent repeated calls
            
//             if (audioSource != null && snailFall != null)
//             {
//                 audioSource.PlayOneShot(snailFall, 0.4f);
//             }
            
//             // Debug.Log("Acid snail destroyed for falling out of bounds");
//             Invoke("EnemyDied", 1);
//         }
//     }
// }

  void FixedUpdate()
{
    if (player == null)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
    }
    
    // Allow movement during moving away state
    if (isMovingAway)
    {
        // Move toward the target position - X direction only
        Vector2 moveDirection = (targetMoveAwayPosition - transform.position).normalized;
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, 0);
        UpdateFacing();
        
        // Check if we've reached the target position - check X only
        if (Mathf.Abs(transform.position.x - targetMoveAwayPosition.x) < 0.1f)
        {
            isMovingAway = false;
            rb.linearVelocity = Vector2.zero;
        }
        return;
    }
    
    if (!shouldMove || isShooting) return;
    
    // Calculate distance to player
    float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
    
    // Platform boundary proximity check
    if (platformTransform != null)
    {
        // Check if within 1 units of either boundary
        bool nearLeftBoundary = transform.position.x <= leftBoundary + 1f;
        bool nearRightBoundary = transform.position.x >= rightBoundary - 1f;
        
        if (nearLeftBoundary || nearRightBoundary)
        {
            // Debug.Log("Near boundary, stopping movement.");
            // Stop moving
            rb.linearVelocity = Vector2.zero;
            moveSpeed = 0;
        
            // Face the player explicitly when stationary
            FacePlayer();
            
            // Always start shooting sequence when at boundary
            StartCoroutine(ShootWhileStationary());
            return;
        }
    }
    
    // If close enough to shoot
    if (distanceToPlayer <= attackFromDistance && !isShooting && shotsRemaining <= 0)
    {
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(ShootSequence());
        return;
    }
    
    // Move toward player - X direction only
    Vector2 direction = (player.transform.position - transform.position).normalized;
    rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);
    UpdateFacing();

    // If snail goes outside of camera viewport while falling, destroy it

}
    
    IEnumerator ShootSequence()
    {
        isShooting = true;
        shotsRemaining = shootHowMany;
        
        // Shoot the specified number of projectiles
        while (shotsRemaining > 0)
        {
            ShootAcidGob();
            shotsRemaining--;
            yield return new WaitForSeconds(timeBetweenShots);
        }
        
        isShooting = false;
        
        // After shooting, move away in the opposite direction of the player
        Vector3 awayDirection = (transform.position - player.transform.position).normalized;
        float randomDistance = Random.Range(moveAwayDistance * 0.5f, moveAwayDistance);
        targetMoveAwayPosition = transform.position + awayDirection * randomDistance;
        isMovingAway = true;
    }
    
    private void ShootAcidGob()
    {
        if (player == null || acidGobPrefab == null) return;
        
        // Calculate direction to player
        Vector3 direction = (player.transform.position - shootPoint.position).normalized;
        
        // Create the acid gob
        GameObject acidGob = Instantiate(acidGobPrefab, shootPoint.position, Quaternion.identity);
        
        // Get or add the AcidGob component
        AcidGob gobComponent = acidGob.GetComponent<AcidGob>();
        if (gobComponent == null)
        {
            gobComponent = acidGob.AddComponent<AcidGob>();
        }
        
        // Initialize the gob properties
        gobComponent.Initialize(direction, gobSpeed, gobDamage, attackFromDistance * 2);
        
        // Play sound effect
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, 0.15f);
        }
        
    }

IEnumerator ShootWhileStationary()
{
    // Debug.Log("Shooting while stationary");
    isShooting = true;
    shotsRemaining = shootHowMany;
    
    // Shoot the specified number of projectiles
    while (shotsRemaining > 0)
    {
        ShootAcidGob();
        shotsRemaining--;
        yield return new WaitForSeconds(1);
    }
    
    isShooting = false;
}

    
private void UpdateFacing()
{
    // Instead of rotation or scale, use a sprite-based approach
    if (rb.linearVelocity.x > 0.1f)
    {
        // Moving right
        transform.rotation = Quaternion.identity; // Reset rotation
        GetComponent<SpriteRenderer>().flipX = false;
    }
    else if (rb.linearVelocity.x < -0.1f)
    {
        // Moving left
        transform.rotation = Quaternion.identity; // Reset rotation
        GetComponent<SpriteRenderer>().flipX = true;
    }
}

private void FacePlayer()
// Called when snail is stationary
{
    // Determine facing direction based on player's position relative to snail
    if (player != null)
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        
        // Use sign of x to determine facing
        if (directionToPlayer.x > 0.1f)
        {
            // Player is to the right
            transform.rotation = Quaternion.identity; // Reset rotation
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (directionToPlayer.x < -0.1f)
        {
            // Player is to the left
            transform.rotation = Quaternion.identity; // Reset rotation
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}
    
    // Handle collisions with the player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.PlayerHitByEnemy();
            }
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
    {
        HandleWallCollision(collision);
    }
    }
    
    // IDamageable implementation
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Debug.Log(name + " took " + damage + " damage. Health left: " + currentHealth);

        if (currentHealth <= 0)
        {
            EnemyDied();
            Debug.Log(name + " died.");
        }
    }
    
 void OnBecameInvisible()
{
    if (rb.linearVelocity.y < 0) // Only destroy if it's falling
    {
       audioSource.PlayOneShot(snailFall, 0.4f);
       Invoke("EnemyDied", 1);
    }
}

    public void EnemyDied()
    {
        StopAllCoroutines();
        
        // Tell EnemySpawner enemy died
        if (spawner != null)
        {
            spawner.EnemyDied();
        }
        
        // Turn off sprite renderer
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        shouldMove = false;
        isShooting = false;
        isMovingAway = false;

        // Play death sound if available
        if (audioSource != null)
        {
            audioSource.Stop();
            if (deathSound != null)
            {
                audioSource.PlayOneShot(deathSound, 0.5f);
            }
        }

        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay());
    }
    
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }


 private void HandleWallCollision(Collision2D collision)
{
    // Get the collision normal
    Vector2 normal = collision.contacts[0].normal;
    
    
    // The normal already points in the direction to move away
    // normal.x = 1 means "move right"
    // normal.x = -1 means "move left"
    
    // Stop current movement
    rb.linearVelocity = Vector2.zero;
    
    // Cancel any shooting sequence
    StopAllCoroutines();
    isShooting = false;
    isMovingAway = true;
    
    // Set target position in the direction of the normal (away from wall)
    targetMoveAwayPosition = new Vector3(
        transform.position.x + (normal.x * moveAwayDistance),
        transform.position.y,
        transform.position.z
    );
    
    // Update sprite facing
    GetComponent<SpriteRenderer>().flipX = (normal.x < 0);
    
    // Reset behavior after delay
    Invoke("ResetBehavior", 0.5f);
}

private void ResetBehavior()
{
    shouldMove = true;
    isMovingAway = false;
}
}