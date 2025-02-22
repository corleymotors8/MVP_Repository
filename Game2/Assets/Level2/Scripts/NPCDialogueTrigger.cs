using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class NPCDialogueTrigger : MonoBehaviour
{
   [Header("Visual Cue")]
   [SerializeField] private GameObject visualCue;

   [Header("Ink JSON")]
   [SerializeField] private TextAsset inkJSON;
   
   [Header("NPC Identifier")]
    public string npcIdentifier;  // Set this in the Inspector for each NPC

   private bool playerInRange;


   private void Awake()
   {
       playerInRange = false;
       visualCue.SetActive(false);
   }

   private void Update()
   {
       if (playerInRange && !DialogueManager2.GetInstance().dialogueIsPlaying)
       {
           visualCue.SetActive(true);
           if (Input.GetKeyDown(KeyCode.E))
           {
               DialogueManager2.GetInstance().SetCurrentNPC(npcIdentifier);

               DialogueManager2.GetInstance().EnterDialogueMode(inkJSON);
           }
       }
       else
       {
           visualCue.SetActive(false);
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



