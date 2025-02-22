using UnityEngine;
using TMPro;
using Ink.Parsed;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine.EventSystems;


public class DialogueManager2 : MonoBehaviour
{
   [Header("Params")]
   [SerializeField] private float typingSpeed = 0.04f; // Smaller values = faster typing
   [Header("Dialogue UI")]
   [SerializeField] private GameObject dialoguePanel;
   [SerializeField] private GameObject continueIcon;
   [SerializeField] private TextMeshProUGUI dialogueText;
   [SerializeField] private GameObject enterPrompt;
   [SerializeField] private GameObject exitPrompt;
   public string currentNPC { get; private set; } = "";


   [Header("Choices UI")]
   [SerializeField] private GameObject[] choices;


   private TextMeshProUGUI[] choicesText;


   public Ink.Runtime.Story currentStory;


   public bool dialogueIsPlaying { get; private set; }


   private Coroutine displayLineCoroutine;


   private bool CanAdvanceDialogue = true;
   
   // Handle NPC talking sounds
   AudioSource audioSource;
   private bool npc1Talking = false;
   private bool npc2Talking = false;
private bool npc3Talking = false;
   public AudioClip npc1;
   public AudioClip npc2;
   public AudioClip npc3;

private Coroutine continuePromptCoroutine;
private Coroutine exitPromptCoroutine;


   private static DialogueManager2 instance;
   // Declare gameManager
    private GameManager gameManager;
   // Declare crusher
    private Crusher crusher;

private float inputCooldown = 0.1f; // Adjust this value as needed
private float lastInputTime = 0f;

   


   private void Awake()
   {
      if (instance != null)
      {
       Debug.LogWarning("More than one instance of DialogueManager2 found!");
      }
       instance = this;
   }


   public static DialogueManager2 GetInstance()
   {
       return instance;
   }


   private void Start()
   {
       enterPrompt.SetActive(false);
       exitPrompt.SetActive(false);
       dialoguePanel.SetActive(false);
       dialogueIsPlaying = false;
        
        // Find audiosource
        audioSource = GetComponent<AudioSource>();
        
        // Find gameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Find crusher
        crusher = FindFirstObjectByType<Crusher>();


       // initalize the choices array
       choicesText = new TextMeshProUGUI[choices.Length];
       int index = 0;
       foreach (GameObject choice in choices)
       {
           choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
           index++;
       }


   }


private void Update()
{
    if (!dialogueIsPlaying)
    {
        return;
    }

    // Check for input to advance dialogue with cooldown
    if (Input.GetKeyDown(KeyCode.Return) && 
        currentStory.currentChoices.Count == 0 && 
        CanAdvanceDialogue && 
        Time.time - lastInputTime >= inputCooldown)
    {
        Debug.Log($"Return key pressed at time: {Time.time}");
        lastInputTime = Time.time;
        ContinueStory();
    }
    else if((Input.GetKeyDown(KeyCode.Return) && currentStory.currentChoices.Count == 0 && !CanAdvanceDialogue))
    {
        Debug.Log("Return key pressed, but dialogue is waiting for game event.");
    }
    else if (Input.GetKeyDown(KeyCode.X))
    {
        Debug.Log("X key pressed, exiting dialogue.");
        ExitDialogueMode();
    }
}


// Function for typewriter effect
   private System.Collections.IEnumerator DisplayLine (string line)
   {
    // Reset text and hide UI elements before typing
    dialogueText.text = "";
    continueIcon.SetActive(false);
    enterPrompt.SetActive(false);
    HideChoices();

    // Type out the dialogue character by character
    foreach (char letter in line.ToCharArray())
    {
        dialogueText.text += letter;
        yield return new WaitForSeconds(typingSpeed);
    }

    // After text is fully displayed, check conditions
    DisplayChoices();

    // **Cancel any existing prompt coroutines before deciding which to show**
    if (exitPromptCoroutine != null)
    {
        StopCoroutine(exitPromptCoroutine);
        exitPromptCoroutine = null;
    }
    if (continuePromptCoroutine != null)
    {
        StopCoroutine(continuePromptCoroutine);
    }

    // **Decide which prompt to show**
    if (currentStory.currentChoices.Count == 0 && !IsWaitingForEvent() && currentStory.canContinue)
    {
        continuePromptCoroutine = StartCoroutine(ShowContinuePromptWithDelay());
    }
    else if (currentStory.currentChoices.Count == 0 &&!currentStory.canContinue)
    {
        exitPromptCoroutine = StartCoroutine(ShowExitPromptWithDelay());
    }
}


public void EnterDialogueMode(TextAsset inkJSON)
{
    Debug.Log("=== EnterDialogueMode START ===");
    currentStory = new Ink.Runtime.Story(inkJSON.text);
    dialoguePanel.SetActive(true);
    dialogueIsPlaying = true;
    gameManager.playerCanMove = false;
    //Disable enter and exit prompts
    enterPrompt.SetActive(false);
    exitPrompt.SetActive(false);

    //Check enter and exit prompt canvas Status

    // Instead of calling Continue() immediately, check currentText first
    string firstLine = currentStory.currentText;

    if (string.IsNullOrEmpty(firstLine) && currentStory.canContinue)
    {
        firstLine = currentStory.Continue();  // Only call Continue() if necessary
    }

    if (displayLineCoroutine != null)
    {
        StopCoroutine(displayLineCoroutine);
    }
    displayLineCoroutine = StartCoroutine(DisplayLine(firstLine));
    Invoke("PlayDialogueAudio", 0.2f);

}

   public void ExitDialogueMode()
   {
       
       npc1Talking = false;
       npc2Talking = false;
       dialogueIsPlaying = false;
       dialoguePanel.SetActive(false);
        
        // Hide both prompts when dialogue ends
        enterPrompt.SetActive(false);
        exitPrompt.SetActive(false);
        gameManager.playerCanMove = true;
       dialogueText.text = "";
   }


   private void HideChoices()
   {
       foreach (GameObject choice in choices)
       {
           choice.SetActive(false);
       }
   }


private void ContinueStory()
{
     Debug.Log("=== ContinueStory START ===");

    // **RESET PANELS IMMEDIATELY BEFORE CHANGING ANYTHING**
    enterPrompt.SetActive(false);
    exitPrompt.SetActive(false);

    // **CHECK FOR CHOICES BEFORE CONTINUING**
    if (currentStory.currentChoices.Count > 0)
    {
        Debug.Log("Choices detected before continuing. Keeping continue panel hidden.");
        return; // Prevent displaying continue panel
    }

    // **CHECK IF THERE IS MORE DIALOGUE**
    if (currentStory.canContinue)
    {
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
        }

        string nextLine = currentStory.Continue();
        Debug.Log("Next line from story: " + nextLine);
        displayLineCoroutine = StartCoroutine(DisplayLine(nextLine));
        Invoke("PlayDialogueAudio", 0.2f);

    }

    Debug.Log("=== ContinueStory END ===");


//    Log all tags at each point in the story
       foreach (var tag in currentStory.currentTags)
       {
           Debug.Log("Tag found: " + tag);

            
            // TAG: Block raise
            if (tag == "block_raise")
            {
                // Enable crusher script
                crusher.enabled = true;
                crusher.isRising = true;
            }

            // TAG: Unfreeze player
            if (tag == "unfreeze_player")
            {
                gameManager.playerCanMove = true;
            }

            // TAG: Give jump ability
            if (tag == "jetpack_ability")
            {
                gameManager.jetpackEnabled = true;
                //Access jetpack script
                JetpackController jetpack = FindFirstObjectByType<JetpackController>();
                //Set max jump force to 25
                jetpack.maxJumpForce = 45f;
                GameManager.Instance.AddPowerup("jetpack_ability");

            }

            //TAG: Wait for jump
            if (tag == "wait_for_jetpack")
            {
                CanAdvanceDialogue = false;
                Debug.Log("Setting prompt to false");
                StartCoroutine(WaitForJetpack());
            }

            // TAG: Attack ability
            if (tag == "attack_ability")
            {
                gameManager.playerCanAttack = true;
                GameManager.Instance.AddPowerup("attack_ability");

            }

            // TAG: Wait for attack
            if (tag == "wait_for_attack")
            {
                CanAdvanceDialogue = false;
                Debug.Log("Setting prompt to false");
                StartCoroutine(WaitForAttack());
            }
       }

}

  // Handles which audio to play
  public void SetCurrentNPC(string npc)
    {
        currentNPC = npc;
        Debug.Log("Current NPC set to: " + currentNPC);
    }



private void PlayDialogueAudio()
{
    if (currentNPC == "npc1")
    {
        audioSource.PlayOneShot(npc1, 0.1f);
        Debug.Log("Playing npc1 voice.");
    }
    else if (currentNPC == "npc2")
    {
        audioSource.PlayOneShot(npc2, 0.1f);
        Debug.Log("Playing npc2 voice.");
    }
    else if (currentNPC == "npc3")
    {
        audioSource.PlayOneShot(npc3, 0.1f);
        Debug.Log("Playing npc2 voice.");
    }
    else
    {
        Debug.LogWarning("No valid NPC found for voice playback.");
    }
}



private void DisplayChoices()
{
   List<Ink.Runtime.Choice> currentChoices = currentStory.currentChoices;


   int index = 0;
   foreach (Ink.Runtime.Choice choice in currentChoices)
   {
       choices[index].gameObject.SetActive(true);
       choicesText[index].text = choice.text;
       index++;
   }


   for (int i = index; i < choices.Length; i++)
   {
       choices[i].gameObject.SetActive(false);
   }


   if (currentChoices.Count > 0)
{
   StartCoroutine(SelectFirstChoice());
}
}


   private System.Collections.IEnumerator SelectFirstChoice()
   {
       EventSystem.current.SetSelectedGameObject(null);
       yield return new WaitForEndOfFrame();
       EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
   }


   private System.Collections.IEnumerator WaitBeforeExit()
   {
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X)); // Player exits manually

//    yield return new WaitForSeconds(0.3f); // Not requiring player to hit X to exit dialogue
   ExitDialogueMode();
   }


   public void MakeChoice(int choiceIndex)
   {
   currentStory.ChooseChoiceIndex(choiceIndex);
   ContinueStory();
   }


 private System.Collections.IEnumerator ShowContinuePromptWithDelay()
{
    yield return new WaitForSeconds(2.5f); // Wait 2.5 seconds

    // **Ensure choices didn't appear during delay**
    if (currentStory.currentChoices.Count == 0 && currentStory.canContinue) 
    {
        enterPrompt.SetActive(true);
        continueIcon.SetActive(true);
        Debug.Log("Continue prompt enabled after delay.");
    }
    else
    {
        Debug.Log("Choices appeared or dialogue ended during delay, skipping continue prompt.");
    }
    continuePromptCoroutine = null; // Reset coroutine reference
}

private System.Collections.IEnumerator ShowExitPromptWithDelay()
{
    
    yield return new WaitForSeconds(2.5f); // Wait 2.5 seconds

    // **Only show exit prompt if there’s no more dialogue**
    if (!currentStory.canContinue)
    {
        exitPrompt.SetActive(true);
        Debug.Log("Exit prompt enabled after delay.");
    }
    else
    {
        Debug.Log("Dialogue unexpectedly continued, skipping exit prompt.");
    }
    exitPromptCoroutine = null; // Reset coroutine reference
}

private bool IsWaitingForEvent()
{
    foreach (string tag in currentStory.currentTags)
    {
        if (tag.StartsWith("wait_for_"))
        {
            Debug.Log("Detected wait tag: " + tag);
            return true;
        }
    }
    return false;
}

/// Level-specific coroutines
 private System.Collections.IEnumerator WaitForJetpack() // Wait for player to leap off edge
    {
        while (!Level2Manager.Instance.playerJumped)
        {
            enterPrompt.SetActive(false);
            continueIcon.SetActive(false);

            yield return null;  // Keep waiting until player has jumped
        }
          //Play next line in story
            Debug.Log("Continuing story");
            ContinueStory();

    }

     private System.Collections.IEnumerator WaitForAttack() // Wait for player to attack enemy
    {
       // Call spawnEnemy
        Level2Manager.Instance.spawnEnemy();

        while (!Level2Manager.Instance.playerKilledEnemy)
        {
            enterPrompt.SetActive(false);
            continueIcon.SetActive(false);
            gameManager.playerCanMove = true;

            yield return null;  // Keep waiting until player has jumped
        }
          //Play next line in story
            Debug.Log("Continuing story");
            ContinueStory();

    }


}


