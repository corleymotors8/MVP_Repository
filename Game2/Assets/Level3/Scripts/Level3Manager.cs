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
  }

public void EndLevel()
{
  
   // Wait 4 seconds then end level
   Invoke("LoadNextLevel", 4f);
}

   public void LoadNextLevel()
   {
        SceneManager.LoadScene("Level3");
   }

 void Update()
  {

   
  }




}









