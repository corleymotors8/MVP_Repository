using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class AutoTriggerDialogue : MonoBehaviour
{
   [Header("Ink JSON")]
   [SerializeField] private TextAsset inkJSON;

   private bool playerInRange;
   private bool dialogueHasTriggered = false;


   private void Awake()
   {
       playerInRange = false;
   }

   private void Update()
   {
       if (playerInRange && !DialogueManager2.GetInstance().dialogueIsPlaying && !dialogueHasTriggered)
       {
               DialogueManager2.GetInstance().EnterDialogueMode(inkJSON);
                dialogueHasTriggered = true;
       }
   }




   private void OnTriggerEnter2D(Collider2D other)
   {
       if (other.gameObject.tag == "Player")
       {
           playerInRange = true;
          
       }
   }
   private void OnTriggerExit2D(Collider2D other)
   {
       if (other.gameObject.tag == "Player")
       {
           playerInRange = false;
        
       }
   }
}



