// Used for Level 1

using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Part 1 Dialogue")]
    [SerializeField]
    private TextAsset inkJSON;
    public bool canTriggerLeapt = false;
    public bool hasPlayedDialogue = false;
    AudioSource audioSource;
    public AudioClip dialogueStart;
    Player player;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>(); 
        player = FindFirstObjectByType<Player>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
         if (collision.gameObject.tag == "Player")
         {
         
         
            if (hasPlayedDialogue) // For some reason dialogue is playing twice. Solving for this bug
            {
             return;
            }
            else 
            {
            Debug.Log("DialogueTrigger triggered");
            StartCoroutine(WaitForTwoCycles()); // Wait a bit before displaying dialogue
            canTriggerLeapt = true; // Allow player to trigger end of level by jumping off screen
            hasPlayedDialogue = true;
            player.preventRespawn = true; // Prevent player from respawning
            Debug.Log("Can player respawn? " + player.preventRespawn);
            }
         }
    }

    IEnumerator WaitForTwoCycles()
    {
        yield return new WaitForSeconds(3.5f);
        //Find moving platform script and disable it
        MovingPlatform movingPlatform = FindFirstObjectByType<MovingPlatform>();
        if (movingPlatform != null)
        {
            movingPlatform.enabled = false;
        }
        audioSource.PlayOneShot(dialogueStart, 0.2f);
        DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        
    }
}
