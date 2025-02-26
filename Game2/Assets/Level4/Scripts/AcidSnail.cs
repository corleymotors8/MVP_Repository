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

    // State tracking
    public bool shouldMove = false;
    private bool isMovingAway = false;
    private bool isShooting = false;
    private int shotsRemaining = 0;
    private GameObject player;
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 targetMoveAwayPosition;
    
    void Start()
    {
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

    void FixedUpdate()
{
    if (player == null)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return; // Still no player found
    }
    
    if (!shouldMove || isShooting) return;  // Don't move if not triggered or currently shooting
    
    // Calculate distance to player
    float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
    
    // If we're moving away after shooting
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
    
    // If we're close enough to shoot
    if (distanceToPlayer <= attackFromDistance && !isShooting && shotsRemaining <= 0)
    {
        rb.linearVelocity = Vector2.zero;  // Stop moving
        StartCoroutine(ShootSequence());
        return;
    }
    
    // Move toward player - X direction only
    Vector2 direction = (player.transform.position - transform.position).normalized;
    rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);
    UpdateFacing();
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
            audioSource.PlayOneShot(shootSound);
        }
        
        Debug.Log("Acid snail shot acid gob at player");
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

    // Called when the player enters the detection range
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Player detected by acid snail");
            shouldMove = true;
            // if (audioSource != null)
            // {
            //     audioSource.Play();
            // }
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
    }
    
    // IDamageable implementation
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(name + " took " + damage + " damage. Health left: " + currentHealth);

        if (currentHealth <= 0)
        {
            EnemyDied();
        }
    }
    
    public void EnemyDied()
    {
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
            AudioClip deathSound = audioSource.clip; // Use the main audio clip as death sound
            if (deathSound != null)
            {
                audioSource.PlayOneShot(deathSound, 0.5f);
            }
        }

        Debug.Log("Acid snail died");
        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay());
    }
    
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}