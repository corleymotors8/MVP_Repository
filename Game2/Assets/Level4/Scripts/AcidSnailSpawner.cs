using UnityEngine;
using System.Collections;

public class AcidSnailSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject leftEnemyPrefab; // Enemy prefab for left spawn position
    public GameObject rightEnemyPrefab; // Enemy prefab for right spawn position
    public Transform leftSpawn; // Left spawn point
    public Transform rightSpawn; // Right spawn point
    public float safeSpawnDistance = 3f; // Minimum distance from player

    public int maxEnemies = 5; // Max enemies allowed at once (only spawns more if enemies killed)
    public int totalSpawnable; // Total enemies allowed
    private int totalSpawned;
    AudioSource audioSource;
    public AudioClip enemySpawn;

    private int currentEnemies = 0; // Tracks active enemies
    private bool playerInZone = false; // Tracks if player is in trigger area
    private bool isSpawning = false; // Tracks if spawning is in progress
    public float spawnDelay = 5f; // Hard code time between spawns
    public bool hasTriggered = false; // Tracks if the spawner has been triggered
    private bool spawnFromLeft = true; // Track which side to spawn from

    [Header("AcidSnail Properties")]
    [Tooltip("Override the AcidSnail properties in the prefab")]
    public bool overrideAcidSnailProperties = true;

    [Header("Health Settings")]
    public int snailMaxHealth = 1;

    [Header("Movement Settings")]
    public float snailMoveSpeed = 2f;
    public float snailAttackDistance = 5f;
    public float snailMoveAwayDistance = 3f;

    [Header("Shooting Settings")]
    public GameObject acidGobPrefab;
    public float snailGobSpeed = 5f;
    public int snailGobDamage = 1;
    public int snailShootHowMany = 3;
    public float snailTimeBetweenShots = 0.5f;
    public AudioClip snailShootSound;

    [Header("Audio Settings")]
    public AudioClip snailDeathSound;
    public AudioClip snailFallSound;

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
            // Debug.Log("Player entered spawner zone");
            playerInZone = true;
            TrySpawnEnemy(); // Start spawning process
            hasTriggered = true; // Prevent multiple triggers
        }
    }

    public void EnemyDied()
    {
        currentEnemies--; // Reduce enemy count when one dies
        Debug.Log("Enemy died, current enemies left: " + currentEnemies);
        TrySpawnEnemy(); // Check if new enemy should be spawned
    }

    private void TrySpawnEnemy()
    {
        if (!playerInZone || currentEnemies >= maxEnemies || isSpawning) return; // Prevent multiple coroutines
        StartCoroutine(SpawnWithDelay());
        Debug.Log("Spawn coroutine started");
    }

    private IEnumerator SpawnWithDelay()
    {
        isSpawning = true; // Lock coroutine

         while (currentEnemies == 0 && totalSpawned < totalSpawnable)
        {
            yield return new WaitForSeconds(1); // Wait before spawning!
            SpawnEnemy();
        }

        while (currentEnemies >=1 && currentEnemies < maxEnemies && totalSpawned < totalSpawnable)
        {
            yield return new WaitForSeconds(spawnDelay); // Wait before spawning!
            SpawnEnemy();
        }

        isSpawning = false; // Unlock when done
    }

    private void SpawnEnemy()
    {
        if (totalSpawned >= totalSpawnable) 
        {
            Debug.Log("Total spawnable reached. Done spawning.");
            return;
        }
        
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
        GameObject newEnemy = Instantiate(prefabToSpawn, finalSpawnPosition, Quaternion.identity, transform);
        
        // Preserve the original scale from the prefab
        newEnemy.transform.localScale = prefabToSpawn.transform.localScale;

        // Apply AcidSnail property overrides if enabled
        if (overrideAcidSnailProperties)
        {
            ApplyAcidSnailProperties(newEnemy);
        }

        // Assign spawner reference to AcidSnail
        AcidSnail acidSnail = newEnemy.GetComponent<AcidSnail>();
        if (acidSnail != null)
        {
            // Set the direct reference for better performance
            acidSnail.spawner = this;
        }

        // Play spawn sound
        if (audioSource != null && enemySpawn != null)
        {
            audioSource.PlayOneShot(enemySpawn, 0.5f);
        }

        currentEnemies++; // Track spawned enemies
        totalSpawned++; // Track total spawns
    }

    private void ApplyAcidSnailProperties(GameObject enemy)
    {
        AcidSnail acidSnail = enemy.GetComponent<AcidSnail>();
        if (acidSnail != null)
        {
            // Apply health settings
            acidSnail.maxHealth = snailMaxHealth;
            acidSnail.currentHealth = snailMaxHealth;

            // Apply movement settings
            acidSnail.moveSpeed = snailMoveSpeed * Random.Range(0.8f, 1.2f);
            acidSnail.attackFromDistance = snailAttackDistance * Random.Range(0.8f, 1.2f);
            acidSnail.moveAwayDistance = snailMoveAwayDistance * Random.Range(0.8f, 1.2f);

            // Apply shooting settings
            if (acidGobPrefab != null)
            {
                acidSnail.acidGobPrefab = acidGobPrefab;
            }
            acidSnail.gobSpeed = snailGobSpeed * Random.Range(0.8f, 1.2f);
            acidSnail.gobDamage = snailGobDamage;
            acidSnail.shootHowMany = snailShootHowMany + Random.Range(-1, 2);
            acidSnail.timeBetweenShots = snailTimeBetweenShots * Random.Range(0.6f, 1.6f);
            
            // Apply audio settings
            if (snailShootSound != null)
            {
                acidSnail.shootSound = snailShootSound;
            }
            if (snailDeathSound != null)
            {
                acidSnail.deathSound = snailDeathSound;
            }
            if (snailFallSound != null)
            {
                acidSnail.snailFall = snailFallSound;
            }

            // Enable movement immediately
            acidSnail.shouldMove = true;
        }
    }

    public void ResetSpawner()
    {
        hasTriggered = false;
        playerInZone = false;
        isSpawning = false; // ✅ Ensure spawning can happen again
        currentEnemies = 0; // ✅ Reset enemy count
        totalSpawned = 0;   // Reset total spawned count
    }
}