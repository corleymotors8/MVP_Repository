using UnityEngine;
using System.Collections;

public class SnailEnemy : MonoBehaviour, IDamageable
{
    public float speed;
    public float moveAmount = 3f;
    public int maxHealth = 1;
    private int currentHealth;
   
    public bool shouldMove;
    private Rigidbody2D rb;
    
    private bool moveLeft = true;
    private int direction = 1;
    private float moved = 0f;
    
    private float leftBound;
    private float rightBound;

    [Header("Chasing Enemies")]
    public float chaseDuration = 5f;
    public bool chasePlayer = false;
    [Tooltip("If true, enemy will chase player until chaseDuration is reached")]
    private Vector3 startingPosition;
    private float chaseTimer = 0f;
    public bool returningToPoint = false;

    public AudioSource audioSource;
    public AudioClip enemyDeath;
    
    public void Start()
    {
        
       //Get audioSource
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.localPosition;
    }
    
    void FixedUpdate()
    {
        
        
        // Patrolling Behavior
        if (!chasePlayer && !returningToPoint)
        {
            transform.localPosition += Vector3.right * speed * direction * Time.deltaTime;
            moved += speed * Time.deltaTime;

            if (moved >= moveAmount)
            {
                direction *= -1;
                Flip();
                moved = 0f;
            }
        }
        // Chasing Behavior
        else if (chasePlayer && shouldMove)
        {            
            UpdateFacing();
            
            chaseTimer += Time.deltaTime;

            Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            if (transform.parent != null)
            {
                playerPosition = transform.parent.InverseTransformPoint(playerPosition);
            }
            
            Vector3 directionToPlayer = (playerPosition - transform.localPosition).normalized;
            rb.linearVelocity = transform.parent != null 
                ? transform.parent.TransformDirection(directionToPlayer) * speed 
                : directionToPlayer * speed;

            if (chaseTimer >= chaseDuration)
            {
                ResetChaseState();
                returningToPoint = true;
            }
        }

        // Returning to StartingPosition
        if (returningToPoint)
        {
            Vector3 directionToPoint = (startingPosition - transform.localPosition).normalized;
            
            Vector3 worldDirection = transform.parent != null 
                ? transform.parent.TransformDirection(directionToPoint) 
                : directionToPoint;
                
            rb.linearVelocity = worldDirection * speed;
            
            UpdateFacing();

            if (Vector3.Distance(transform.localPosition, startingPosition) < 0.1f) 
            {
                CompleteReturn();
            }
        }
    }

    private void ResetChaseState()
    {
        chasePlayer = false;
        shouldMove = false;
        chaseTimer = 0f;
    }

    private void CompleteReturn()
    {
        // Reset to initial state
        ResetChaseState();
        returningToPoint = false;
        
        // Reset position and velocity
        transform.localPosition = startingPosition;
        rb.linearVelocity = Vector2.zero;
        
        // Reset direction if needed
        direction = 1;
        moved = 0f;
        
        // Make sure we're ready for the next chase
        chasePlayer = true;  // Enable chase capability

        // Reset sprite direction
        transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);

    }

    private void UpdateFacing()
    {
        if (rb.linearVelocity.x > 0) 
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        else if (rb.linearVelocity.x < 0) 
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
    }

      private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.PlayerHitByEnemy();
            Debug.Log("Player hit by enemy");
        }
    }

    // For chasing snales
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered chase trigger");
            shouldMove = true;
            audioSource.Play();
        }
    }

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
		
		// Turn off sprite SpriteRenderer
		GetComponent<SpriteRenderer>().enabled = false;

		// Disable Collider2D
		GetComponent<Collider2D>().enabled = false;

		// Play death sound
        audioSource.PlayOneShot(enemyDeath, 0.1f);

		
		Debug.Log("Enemy died");

        // Wait and destroy
        StartCoroutine(WaitAndDestroy());
	}

    private IEnumerator WaitAndDestroy()
	{
		yield return new WaitForSeconds(4f); // Wait for 1 second
		Destroy(gameObject); // Remove enemy
	}

  
}