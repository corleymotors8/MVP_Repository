using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
	[Header("Spawner Settings")]
	public GameObject leftEnemyPrefab; // Enemy prefab for left spawn position
	public GameObject rightEnemyPrefab; // Enemy prefab for right spawn position
	public Transform leftSpawn; // Left spawn point
	public Transform rightSpawn; // Right spawn point
	public float safeSpawnDistance = 3f; // Minimum distance from player

	public int maxEnemies = 5; // Max enemies allowed at once
	public int totalSpawnable;
	private int totalSpawned;
	AudioSource audioSource;
	public AudioClip enemySpawn;

	private int currentEnemies = 0; // Tracks active enemies
	private bool playerInZone = false; // Tracks if player is in trigger area
    private bool isSpawning = false; // Tracks if spawning is in progress
    public float spawnDelay = 5f; // Adjustable time between spawns
	public bool hasTriggered = false; // Tracks if the spawner has been triggered
	private bool spawnFromLeft = true; // Track which side to spawn from


	private Transform player;


	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		player = GameObject.FindGameObjectWithTag("Player").transform; // Find player by tag
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && !hasTriggered) // Ensure player triggers the spawner
		{
			playerInZone = true;
			TrySpawnEnemy(); // Start spawning process
			hasTriggered = true; // Prevent multiple triggers
		}
	}

	public void EnemyDied()
	{
		currentEnemies--; // Reduce enemy count when one dies
		TrySpawnEnemy(); // Check if new enemy should be spawned
	}

	private void TrySpawnEnemy()
	{
	if (!playerInZone || currentEnemies >= maxEnemies || isSpawning) return; // Prevent multiple coroutines
	StartCoroutine(SpawnWithDelay());
	}

   private IEnumerator SpawnWithDelay()
{
	isSpawning = true; // Lock coroutine

	while (currentEnemies < maxEnemies)
	{
		yield return new WaitForSeconds(spawnDelay); // Wait before spawning!
		SpawnEnemy();
	}

	isSpawning = false; // Unlock when done
}


private void SpawnEnemy()
{
    Debug.Log("Total spawned = " + totalSpawned + " | Total spawnable = " + totalSpawnable);
    if (totalSpawned >= totalSpawnable) return;
    
    if (player == null) return; // Safety check

    // Alternate between left and right spawn points
    bool useLeftSpawn = spawnFromLeft;
    Transform spawnPoint = useLeftSpawn ? leftSpawn : rightSpawn;
    spawnFromLeft = !spawnFromLeft; // Toggle for next spawn
    
    // Select the appropriate enemy prefab based on spawn point
    GameObject prefabToSpawn = useLeftSpawn ? leftEnemyPrefab : rightEnemyPrefab;
    
    // Safety check for prefab
    if (prefabToSpawn == null)
    {
        Debug.LogError("Missing enemy prefab for " + (useLeftSpawn ? "left" : "right") + " spawn!");
        return;
    }
    
    Vector3 finalSpawnPosition = spawnPoint.position; // Start with the original spawn position

    // If the chosen spawn point is too close, offset it by 5 units on the X-axis
    if (Vector2.Distance(spawnPoint.position, player.position) < safeSpawnDistance)
    {
        finalSpawnPosition.x += (finalSpawnPosition.x < player.position.x) ? -5f : 5f;
    }

    // Instantiate enemy at the adjusted position
    GameObject newEnemy = Instantiate(prefabToSpawn, finalSpawnPosition, Quaternion.identity, transform); // ✅ Set parent to this spawner
    // ✅ Explicitly set the spawner as the parent after instantiation
    newEnemy.transform.SetParent(transform);

    // Ensure enemy faces correct direction
    newEnemy.transform.localScale = new Vector3(spawnPoint == leftSpawn ? 1 : -1, 1, 1);

    // Assign spawner reference to enemy
    EnemyLeap enemyScript = newEnemy.GetComponent<EnemyLeap>();
    if (enemyScript != null)
    {
        enemyScript.spawner = this;
    }

    currentEnemies++; // Track spawned enemies
    totalSpawned++; // Track total spawns
    Debug.Log("Total spawned = " + totalSpawned);
}

public void ResetSpawner()
{
    hasTriggered = false;
    playerInZone = false;
    isSpawning = false; // ✅ Ensure spawning can happen again
    currentEnemies = 0; // ✅ Reset enemy count
}


}
