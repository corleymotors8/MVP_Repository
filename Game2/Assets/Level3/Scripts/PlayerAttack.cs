//Just used for level2 right now

using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //Declare player
    private Player player;
    AudioSource audioSource;
    public AudioClip squishSnail; 
    public int attackDamage = 3; // Set damage amount in Inspector



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get audioSource
        audioSource = GetComponent<AudioSource>();
        
        //Find player
        player = FindFirstObjectByType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player.isAttacking && collision.gameObject.tag == "Enemy")
        {

            IDamageable enemy = collision.GetComponent<IDamageable>();
            if (enemy != null) 
			    {
				enemy.TakeDamage(attackDamage); // Apply damage to enemy
                audioSource.PlayOneShot(squishSnail, 0.1f);

			    }
            // Knock enemy back a little if still has health
            if (enemy != null)
            {
                Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
                    Debug.Log("Enemy knocked back");
                }    
            }

            //Just for level2
            Debug.Log("Enemy hit");
            Level2Manager.Instance.playerKilledEnemy = true;
        }
    }
}
