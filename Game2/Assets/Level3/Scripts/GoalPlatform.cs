using UnityEngine;

public class GoalPlatform : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip goalSound;
    private bool hasPlayed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    // On TriggerEnter2D play goalSound
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
            audioSource.PlayOneShot(goalSound, 0.2f);
            hasPlayed = true;
            // Change color to green
            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }


    
        
} 

