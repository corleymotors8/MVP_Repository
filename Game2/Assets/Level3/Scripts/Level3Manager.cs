// Used for managing events in Level 3

using UnityEngine;
using System.Collections;
using UnityEditor.EventSystems;
using UnityEngine.SceneManagement;

public class Level3Manager : MonoBehaviour
{
 
 
  public static Level3Manager Instance;
  public bool playerCanMove = true; // Freeze player during dialogue
  public bool playerKilledEnemy = false;

  // Access Level2Fader
  public Level2Fader level2Fader;

// Access player 
  Player player; // access player script
//Access main camera
  Camera mainCamera;


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
    // Access Level2Fader
    level2Fader = FindFirstObjectByType<Level2Fader>();

    //Load Powerups
    if(GameManager.Instance.playerPowerups.Count > 0)
    {
        foreach(string powerup in GameManager.Instance.playerPowerups)
        {
            if(powerup == "jetpack_ability")
            {
                GameManager.Instance.jetpackEnabled = true;
                Debug.Log("Jetpack enabled from Level 2 choice");
            }
            else if(powerup == "attack_ability")
            {
                GameManager.Instance.playerCanAttack = true;
                Debug.Log("Attack enabled from Level 2 choice");
            }
        }
    }
  }

public void EndLevel()
{
  
  Debug.Log("Level 3 Ended");
  
  //Call fade to black
  Debug.Log("Fading to black");
 StartCoroutine(level2Fader.FadeOut());

  //Fade down music
  StartMusicFade();

  
   // Wait 4 seconds then end level
   Invoke("LoadNextLevel",7f);
}

   public void LoadNextLevel()
   {
       Debug.Log("Loading next level");
       /// Load scene 4
   }

   public void StartMusicFade()
{
   Debug.Log("Starting music fade");
   AudioSource music = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
   StartCoroutine(FadeMusic(music, 7f)); // Fades out over 4 seconds
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


 void Update()
  {

   
  }




}









