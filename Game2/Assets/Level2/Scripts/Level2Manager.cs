// Used for managing events in Level 2


using UnityEngine;
using System.Collections;
using UnityEditor.EventSystems;
using UnityEngine.SceneManagement;


public class Level2Manager : MonoBehaviour
{
   // *** CURRENTLY HANDLING THESE WITH DIALOGUE TRIGGER ON LAST PLATFORM ***
   // [Header("Part 1 Dialogue")]
   // [SerializeField] private TextAsset inkJSON; // Ink file for part 1


   // [Header("Part 2 Dialogue")]
   // [SerializeField] private TextAsset inkJSON2; // Ink file for part 2
   
   public static Level2Manager Instance;
   private bool hasTriggeredDialogue = false;
  
   public bool playerCanMove = true; // Freeze player during dialogue
   public bool playerJumped = false; // Checks if player has jumped


   // Checks if player has leapt off screen to end level
   [HideInInspector]
   private Rigidbody2D playerRb; // Access player rigidbody
  
   private DialogueTrigger dialogueTrigger; // Access DialogueTrigger
   private CanvasGroup fadePanel; // Access fade panel

  Level2Fader fadeController; // Access FadeController

  // Create slot for enemy prefab
    public GameObject enemyPrefab;

    public bool playerKilledEnemy = false;
 
  private void Awake()
  {

       fadeController = FindFirstObjectByType<Level2Fader>();
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
    ResetPowerups();	
   
//    { 
//     //Wait a frame before starting the fade to ensure everything is initialized
        StartCoroutine(StartLevelFade());
//     }

   }

   void ResetPowerups()
    {
    GameManager.Instance.playerPowerups.Clear();
    GameManager.Instance.jetpackEnabled = false;
    GameManager.Instance.playerCanAttack = false;
    PlayerPrefs.DeleteKey("PlayerPowerups"); // Clears stored power-ups
    Debug.Log("Power-ups reset!");
    }

   private IEnumerator StartLevelFade()
{
    yield return null;  // Wait one frame
    
    if (fadeController != null)
    {
        StartCoroutine(fadeController.FadeIn());
    }
    else
    {
        Debug.LogError("FadeController not found!");
    }
}

public void spawnEnemy()
{
    // Spawn enemy prefab
    GameObject enemyObject = Instantiate(enemyPrefab, new Vector3(31.3f, -4.5f, 0), Quaternion.identity);
    SnailEnemy enemy = enemyObject.GetComponent<SnailEnemy>();
    // Change x scale to -1
    enemyPrefab.transform.localScale = new Vector3(-1, 1, 1);
    //Access enemy script and set move speed to 0 
    enemy.speed = 0;
   
}

public void EndLevel()
{
    //Fade down music
    StartMusicFade();

    //Start fade out
    StartCoroutine(fadeController.FadeOut());
    
    // Wait 4 seconds then end level
    Invoke("LoadNextLevel", 4f);
}


public void StartMusicFade()
{
   Debug.Log("Starting music fade");
   AudioSource music = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
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

    public void LoadNextLevel()
    {
         SceneManager.LoadScene("Level3");
    }


  void Update()
   {
     
   }
  
 


}



