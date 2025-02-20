using System.Collections;
using UnityEngine;

public class EnemyLeap : MonoBehaviour, IDamageable, ICrushable
{
	[Header("Jump Settings")]
	public float baseJumpDistance = 5f; // Base horizontal jump distance
	public float baseJumpHeight = 5f;   // Base jump height
	public float baseJumpCooldown = 2f; // Base cooldown before jumping again

    [Header("Health")]
    public int maxHealth = 10; // Set enemy's health in Inspector
	private int currentHealth;
	public int attackDamage = 1; // Set damage amount in Inspector

	private Transform player; // Reference to player's transform

	AudioSource audioSource;
	public AudioClip enemyDeath;
	public AudioClip enemyJump;



	private Rigidbody2D rb;
    [HideInInspector]
    public EnemySpawner spawner;
	private bool isGrounded = true;
	private float jumpTimer;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		player = GameObject.FindGameObjectWithTag("Player").transform; // Find player by tag
		rb = GetComponent<Rigidbody2D>(); // Automatically get Rigidbody
		jumpTimer = GetRandomCooldown();  // Start with a random cooldown
        currentHealth = maxHealth; // Start at full health

	}

	void Update()
	{
		// Countdown before jumping again
		jumpTimer -= Time.deltaTime;
		if (jumpTimer <= 0f && isGrounded)
		{
			LeapTowardPlayer();
			jumpTimer = GetRandomCooldown(); // Reset with a new random cooldown
		}
	}

	void LeapTowardPlayer()
	{
		if (player == null || rb == null) return;

		// Calculate direction to player
		Vector2 direction = (player.position - transform.position).normalized;

		// Randomize jump parameters within ±1.5 range
		float jumpDistance = baseJumpDistance + Random.Range(0, 2.5f);
		float jumpHeight = baseJumpHeight + Random.Range(0, 2.5f);
		

		// Scale direction by randomized jump distance
		Vector2 jumpForce = new Vector2(direction.x * jumpDistance, jumpHeight);

		// Apply impulse force
		rb.linearVelocity = Vector2.zero; // Reset velocity for consistency
		rb.AddForce(jumpForce, ForceMode2D.Impulse);

		// Assume airborne until grounded
		isGrounded = false;
	}

	// Generate a random cooldown
	float GetRandomCooldown()
	{
		return baseJumpCooldown + Random.Range(0, 0.5f);
	}

	// Detect ground
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Ground")) // Ensure ground has correct tag
		{
			isGrounded = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop horizontal movement while keeping vertical speed
			// Play jump sound
			audioSource.PlayOneShot(enemyJump, 0.1f);
		}

		if (collision.gameObject.CompareTag("Player"))
		{
			Player playerScript = collision.gameObject.GetComponent<Player>();
			if (playerScript != null)
			{
				playerScript.TakeDamage(attackDamage); // Apply damage to player

				// Bounce enemy backwards
				Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
				rb.AddForce(bounceDirection * 10f, ForceMode2D.Impulse);
			}
		}
	}

    public void TakeDamage(int damage)
	{
		currentHealth -= damage;
		Debug.Log(name + " took " + damage + " damage. Health left: " + currentHealth);

        //Placeholder for better effect
        //Change sprite color randomly
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

		if (currentHealth <= 0)
		{
			EnemyDied();
		}
	}

	public void Crushed()
	{
		EnemyDied();
	}

    	public void EnemyDied()
	{
		if (spawner != null)
		{
			spawner.EnemyDied(); // Alert spawner that this enemy died
		}
		
		// Turn off sprite SpriteRenderer
		GetComponent<SpriteRenderer>().enabled = false;

		// Disable Collider2D
		GetComponent<Collider2D>().enabled = false;

		// Play death animation

		// Play death sound
		audioSource.PlayOneShot(enemyDeath, 0.3f);
		Debug.Log("Enemy died");

		// Destroy enemy after 4 seconds
		StartCoroutine(WaitAndDestroy());

	}


	private IEnumerator WaitAndDestroy()
	{
		yield return new WaitForSeconds(4f); // Wait for 1 second
		Destroy(gameObject); // Remove enemy
	}
}
