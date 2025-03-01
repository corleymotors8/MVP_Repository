using UnityEngine;

public class HoopController : MonoBehaviour
{
    [Header("Settings")]
    public AudioClip scoreSound;
    public AudioClip enemyScoreSound;
    
    [Header("Stats")]
    public int pointsScored = 0;
    public int enemyPoints = 0;
    
    private AudioSource audioSource;
   
    
    private void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
    }
    
    
    public void ScorePointPlayer()
    { 
        // Increment score
        pointsScored++;
        
        // Play sound effect
        if (scoreSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreSound, 0.2f);
        }
        
        // Debug log for testing
        Debug.Log("Score! Total Player points: " + pointsScored);
    }

    public void ScorePointEnemy()
    {
    enemyPoints++;
    Debug.Log("Enemy Scored! Total points: " + enemyPoints);

        audioSource.PlayOneShot(enemyScoreSound, 0.3f);
    }
}


