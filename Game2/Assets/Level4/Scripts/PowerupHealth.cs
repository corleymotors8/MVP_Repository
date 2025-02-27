using System.Collections;
using UnityEngine;

public class PowerupHealth : MonoBehaviour
{
   public AudioClip touchSound;
   public AudioClip popSound;
   private AudioSource audioSource;
   private bool isCollected = false;
   private Vector3 originalPosition;

   [Header("Bounce Settings")]
   public float bounceHeight = 0.1f;  // How high it bounces
   public float bounceSpeed = 2f;    // How fast it bounces

   void Start()
   {
       audioSource = GetComponent<AudioSource>();
       originalPosition = transform.position;
   }

   void Update()
   {
       // Bounce effect using sine wave
       float newY = originalPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
       transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
       if (isCollected) return; // Prevent multiple activations

       Player player = other.GetComponent<Player>();
       if (player != null && player.currentHealth < player.maxHealth)
       {
           isCollected = true;
           StartCoroutine(RefillHealth(player));
       }
       else if (player.currentHealth >= player.maxHealth)
       {
           // Play sound and destroy if player is at max health
           audioSource.PlayOneShot(touchSound);
           Invoke("DestroyObject", 0.2f);
       }
   }

   private IEnumerator RefillHealth(Player player)
   {
       // Play touch sound
       audioSource.PlayOneShot(touchSound);

       // Wait briefly before starting the refill
       yield return new WaitForSeconds(0.5f);

       // Wait briefly to separate the pop sounds
       yield return new WaitForSeconds(0.1f);

       while (player.currentHealth < player.maxHealth)
       {
           player.currentHealth++;
           audioSource.PlayOneShot(popSound);
           yield return new WaitForSeconds(0.3f); // Delay between pop sounds
       }

       Invoke("DestroyObject", 0.3f);
   }

   private void DestroyObject()
   {
       Destroy(gameObject);
   }
}
