using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    AudioSource audioSource;
    public float spawnDelay = 2f;
    public AudioClip spawnSound;
    public int maxBalls = 1;

    [Header("Stats")]
    public int currentBallCount = 1;

    private bool isSpawningActive = true;
    private Coroutine spawnCoroutine;

    // Called by the ball when it's destroyed

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void BallDestroyed()
    {
        currentBallCount--;
        
        // Only start spawning if we're not already spawning
        if (isSpawningActive && currentBallCount < maxBalls && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        // Wait for the spawn delay
        yield return new WaitForSeconds(spawnDelay);
        
        // Spawn a new ball
        SpawnBall();
        
        // Clear the coroutine reference
        spawnCoroutine = null;
    }

    public void SpawnBall()
    {
        
        
        // Make sure we have valid references
        if (ballPrefab == null || spawnPoint == null)
        {
            return;
        }

        // Spawn the ball at the spawn point position and rotation
        GameObject newBall = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
        audioSource.PlayOneShot(spawnSound, 0.08f);
        
        // Increment the counter
        currentBallCount++;
    }

    
}