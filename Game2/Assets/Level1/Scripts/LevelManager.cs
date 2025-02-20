// Used for managing events in Level 1

using UnityEngine;
using System.Collections;
using UnityEditor.EventSystems;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // *** CURRENTLY HANDLING THESE WITH DIALOGUE TRIGGER ON LAST PLATFORM ***
    // [Header("Part 1 Dialogue")]
    // [SerializeField] private TextAsset inkJSON; // Ink file for part 1

    // [Header("Part 2 Dialogue")]
    // [SerializeField] private TextAsset inkJSON2; // Ink file for part 2
    public static LevelManager Instance;
    private bool hasTriggeredDialogue = false;
    
    // public bool playerCanMove = true; // Moved to GameManager

    // Checks if player has leapt off screen to end level
    [HideInInspector]
    public bool PlayerLeapt = false; // Determines if player leapt
    private Rigidbody2D playerRb; // Access player rigidbody
    
    private DialogueTrigger dialogueTrigger; // Access DialogueTrigger
   
   private void Awake()
   {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
        Instance = this;
        }
   }

   private void Start()
    {
        //Access game Manager
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        // Reset playercan jump and player can attack
        gameManager.jetpackEnabled = false;
        gameManager.playerCanAttack = false;
        
        // Automatically find the player's Rigidbody2D
        playerRb = FindFirstObjectByType<Player>().GetComponent<Rigidbody2D>();

        //Access DialogueTrigger
        dialogueTrigger = FindFirstObjectByType<DialogueTrigger>();

        if (playerRb == null)
        {
            Debug.LogError("Player Rigidbody2D not found in scene!");
        }
    }

public void DisablePlayerSound()
    {
        // Disable player audio component
        AudioSource playerAudio = FindFirstObjectByType<Player>().GetComponent<AudioSource>();
        if (playerAudio != null)
        {
            playerAudio.enabled = false;
        }
    }

public void EnablePlayerSound()
    {
        // Enable player audio component
        AudioSource playerAudio = FindFirstObjectByType<Player>().GetComponent<AudioSource>();
        if (playerAudio != null)
        {
            playerAudio.enabled = true;
        }
    }
public void DisableCrusher()
    {
   foreach (var crusher in FindObjectsByType<Crusher>(FindObjectsSortMode.None))
		crusher.gameObject.SetActive(false);
    }

public void DisableSnailEnemy()
    {
   foreach (var snail in FindObjectsByType<SnailEnemy>(FindObjectsSortMode.None))
        snail.gameObject.SetActive(false);
    }

public void StartMusicFade()
{
	Debug.Log("Starting music fade");
    AudioSource music = GameObject.Find("Music").GetComponent<AudioSource>();
	StartCoroutine(FadeMusic(music, 4f)); // Fades out over 4 seconds
}

IEnumerator FadeMusic(AudioSource music, float duration)
    {
	float startVolume = music.volume;
	for (float t = 0; t < duration; t += Time.deltaTime)
	{
		music.volume = Mathf.Lerp(startVolume, 0, t / duration);
		yield return null;
	}
	music.volume = 0;
    }

    // Load next level
   
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
   
   void Update()
    {
       
    }
    

}
