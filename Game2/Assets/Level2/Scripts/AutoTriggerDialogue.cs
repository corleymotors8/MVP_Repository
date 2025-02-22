using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class AutoTriggerDialogue : MonoBehaviour
{
   [Header("Ink JSON")]
   [SerializeField] private TextAsset inkJSON;
   [Header("NPC Identifier")]
    public string npcIdentifier;  // Set this in the Inspector for each NPC

   private bool playerInRange;
   private bool dialogueHasTriggered = false;
   AudioSource audioSource;
   public AudioClip dialogueStart;


   private void Start()
   {
       audioSource = GetComponent<AudioSource>();
   }
   
   private void Awake()
   {
       playerInRange = false;
   }

   private void Update()
   {
       if (playerInRange && !DialogueManager2.GetInstance().dialogueIsPlaying && !dialogueHasTriggered)
       {
               audioSource.PlayOneShot(dialogueStart, 0.2f);
               DialogueManager2.GetInstance().SetCurrentNPC(npcIdentifier);
               Debug.Log("Auto-triggered dialogue for: " + npcIdentifier);
               DialogueManager2.GetInstance().EnterDialogueMode(inkJSON);
               dialogueHasTriggered = true;
       }
   }




  private void OnTriggerEnter2D(Collider2D other)
{
    if (other.gameObject.tag == "Player")
    {
        Debug.Log("=== Trigger Enter ===");
        Debug.Log("Player in range: " + playerInRange);
        Debug.Log("Dialogue is playing: " + DialogueManager2.GetInstance().dialogueIsPlaying);
        Debug.Log("Dialogue has triggered: " + dialogueHasTriggered);
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



