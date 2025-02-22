using UnityEngine;
using System.Collections;

public class ApproachPlayer : MonoBehaviour, IDamageable
{
    public float moveSpeed = 1f; // Speed of movement
    private bool shouldMove = false;
    public int currentHealth = 6;
    AudioSource audioSource;
    public AudioClip enemyDeath;
    Rigidbody2D rb;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

    }
    
    private void Update()
    {
        if (shouldMove)
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the "Player" tag
        {
            shouldMove = true;
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

    public void EnemyDied()
	{
		
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().TakeDamage(20);
        }
    }




}
