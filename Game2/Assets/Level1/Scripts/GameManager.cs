using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
   // public GameObject playerPrefab;
   public AudioClip respawnSound;
   public AudioClip gameOver;
   public AudioClip winSound;
   public AudioClip deathSound;
   AudioSource audioSource;
   [HideInInspector]
   public Vector3 respawnPosition; // Updated by CameraPanTrigger
   public int playerLives = 1;
   private bool isRespawning = false;
   public bool playerCanMove = true; // Freeze player during dialogue
   public bool jetpackEnabled = false; // Player jump ability
   public bool playerCanDoubleJump = false; // Player double jump ability
   public bool playerCanAttack = false; // Player attack ability

   public Vector3 startingPosition;
   public static GameManager Instance; // Singleton

   public List<string> playerPowerups = new List<string>();

 
 
   public int enemiesKilled = 0;
   [HideInInspector]
   public int playerDeaths;
   public GameObject gameOverText;  // Assign your "GAME OVER" text GameObject here
   public GameObject youWinText;
   // private Timer timerUI;
   public float gameOverDelay = 7f;  // Time to wait before returning to the menu
   // Access player script
   Player player; // access player script

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across levels
        }
        else
        {
            Destroy(gameObject);
        }

        
    }
    
  
   void Start()
   {
       player = FindFirstObjectByType<Player>();

       // Load powerups
       LoadPowerups(); // Call this in Start()
        if (playerPowerups.Contains("jetpack_ability"))
        {
        GameManager.Instance.jetpackEnabled = true;
        Debug.Log("Player has Jetpack");
        }

        //Reset respawn position
        startingPosition = player.transform.position;
        respawnPosition = startingPosition;
        Debug.Log("Respawn position not set, defaulting to: " + respawnPosition);
      
       
       enemiesKilled = 0;

       //Find the timer
       // timerUI = FindFirstObjectByType<Timer>();
       // reset isGameWonOrLost
       // timerUI.isGameWonOrLost = false;
      
       // InitializeLives(playerLives);

        audioSource = GetComponent<AudioSource>();
       if (audioSource == null)
       {
           audioSource = gameObject.AddComponent<AudioSource>();
       }

       // gameOverText.SetActive(false);
       // youWinText.SetActive(false);
   }

   public void AddPowerup(string powerup)
    {
        if (!playerPowerups.Contains(powerup))
        {
            playerPowerups.Add(powerup);
            SavePowerups(); // Save after adding
        }
    }

    private void SavePowerups()
    {
        PlayerPrefs.SetString("PlayerPowerups", string.Join(",", playerPowerups)); // Save as CSV
        PlayerPrefs.Save();
    }

    public void LoadPowerups()
    {
        string savedPowerups = PlayerPrefs.GetString("PlayerPowerups", "");
        if (!string.IsNullOrEmpty(savedPowerups))
        {
            playerPowerups = new List<string>(savedPowerups.Split(',')); // Load as List
        }
    }
  
   public void PlayerDied() // Respawns player if lives > 0, else game over
   {
       if (isRespawning) return;  // Prevent multiple respawn calls
       
       isRespawning = true;
       
       // Respawn if player has lives (always true for now)
       if (playerLives > 0)
            {
            //playerLives--;
           
            audioSource.PlayOneShot(deathSound, 0.4f);
            playerDeaths++;
            Invoke("RespawnPlayer", 1.0f);
            }

   }
       
       //// *** GAME OVER ///
        //    else
            
            // Stop background music   
            // GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().Stop();


            //  // Play Game Over music
            // audioSource.PlayOneShot(gameOver, .4f);


            // // Hide all objects
            // HideAllGameObjects(); 
      
            // // Show Game Over text
            //  gameOverText.SetActive(true);


            //  // Start coroutine to handle delay and scene change
            // StartCoroutine(GameOverSequence());
            // }
   

   // Coroutine to handle delay and return to menu
   private IEnumerator GameOverSequence()
   {
       //Stop timer
      // timerUI.isGameWonOrLost = true;
      
       // Wait for a few seconds
       yield return new WaitForSeconds(gameOverDelay);


       // Return to the main menu scene
       // SceneManager.LoadScene("MainMenu");  // Replace with your actual menu scene name
   }


   public void WinGame()
   {
       // GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().Stop();
       audioSource.PlayOneShot(winSound, 0.4f);
       // youWinText.SetActive(true);
         // Stop the timer
  
       HideAllGameObjects();
       StartCoroutine(GameOverSequence());
   }

   public void IncrementEnemiesKilled()
   {
       enemiesKilled++;
       Debug.Log("Enemies killed: " + enemiesKilled);
   }

   void RespawnPlayer()
   {
        Debug.Log("Respawning player at: " + respawnPosition);
        if (!player.preventRespawn)
        {
       
       // Play respawn sound
       audioSource = GetComponent<AudioSource>();
       if (audioSource == null)
       {
           audioSource = gameObject.AddComponent<AudioSource>();
       }
        audioSource.PlayOneShot(respawnSound, 0.2f);

       // Respawn player
       player.transform.position = respawnPosition;  // Move player to respawn location
       player.GetComponent<SpriteRenderer>().enabled = true;  // Make the player visible after enemy script makes invisible
       isRespawning = false;  // Reset respawn flag
       player.currentHealth = player.maxHealth;  // Reset health

       //// Handle player immunity ////
       // player.GetComponent<PlayerScript>().isImmune = true;  // Activate immunity
       // StartCoroutine(HandleImmunity(player));  // Start flashing effect and immunity timer
        }
        else
        {
            Debug.Log("Player prevented from respawning");
        }
       
   }


   private IEnumerator HandleImmunity(GameObject player)
{
   SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
   float immunityDuration = 5f;
   float flashInterval = 0.2f;


   for (float timer = 0; timer < immunityDuration; timer += flashInterval)
   {
       playerRenderer.enabled = !playerRenderer.enabled;  // Toggle visibility
       yield return new WaitForSeconds(flashInterval);
   }


   playerRenderer.enabled = true;  // Ensure player is visible
   player.GetComponent<Player>().isImmune = false;  // End immunity
}


   void HideAllGameObjects()
{
   // Find all game objects with a Renderer component
   Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);


   // Loop through each renderer and disable it
   foreach (Renderer renderer in renderers)
   {
       renderer.enabled = false;
   }
}
  

}



