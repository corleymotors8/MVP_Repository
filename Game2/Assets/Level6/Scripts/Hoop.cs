using UnityEngine;

public class HoopController : MonoBehaviour
{
    [Header("Settings")]
    [HideInInspector]
    public AudioClip scoreSound;
    
    [Header("Stats")]
    public int pointsScored = 0;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);
        if (other.CompareTag("Ball"))
        {
            ScorePoint();
        }
        
        
    }
    
    public void ScorePoint()
    {
        // Increment score
        pointsScored++;
        
        // Play sound effect
        if (scoreSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreSound, 0.3f);
        }
        
        // Debug log for testing
        Debug.Log("Score! Total points: " + pointsScored);
    }
}