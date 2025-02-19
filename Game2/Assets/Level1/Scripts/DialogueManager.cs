using UnityEngine;
using TMPro;
using Ink.Parsed;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine.EventSystems;


public class DialogueManager : MonoBehaviour
{
    //Cycle through 3 different sounds for dialogue
    public AudioClip dialoguePlaying1;
    public AudioClip dialoguePlaying2;
    public AudioClip dialoguePlaying3;
    private int currentDialogueSound = 0;
    AudioSource audioSource;

    public AudioClip endLevel;

    //
    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f; // Smaller values = faster typing
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;

    private TextMeshProUGUI[] choicesText;

    public Ink.Runtime.Story currentStory;

    [HideInInspector]
    public bool dialogueIsPlaying;

    private Coroutine displayLineCoroutine;

    private bool CanAdvanceDialogue = true;

    private static DialogueManager instance;
    private LevelManager levelManager;  // Access LevelManager
    private GameManager gameManager; // Access GameManager
    private Player player; // Access Player 

    
    private void Awake()
    {
       if (instance != null)
       {
        Debug.LogWarning("More than one instance of DialogueManager found!");
       }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        gameManager = FindFirstObjectByType<GameManager>();  // Add this line

        levelManager = FindFirstObjectByType<LevelManager>();  // Find and assign LevelManager
        player = FindFirstObjectByType<Player>();  // Find and assign Player
        
        dialoguePanel.SetActive(false);
        dialogueIsPlaying = false;

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

   // Check for input to advance dialogue
   if (Input.GetKeyDown(KeyCode.Return) && currentStory.currentChoices.Count == 0 && CanAdvanceDialogue)
    {
        Debug.Log("Return key pressed, advancing dialogue.");
        ContinueStory();
    }
    // If !CanAdvanceDialogue, game is waiting for specific event
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
        // empty the dialogue text
        dialogueText.text = "";

        // hide items while text is typing
        continueIcon.SetActive(false);
        HideChoices();
        
        // Display letters one at a time
        foreach (char letter in line.ToCharArray())
        {
            // // if the submit button is pressed, display the entire line  *** BUGGY - NOT USING ***
            // if (Input.GetKeyDown(KeyCode.Return))
            // {
            //     dialogueText.text = line;
            //     break;
            // }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Show continue icon when text is done typing
        continueIcon.SetActive(true);
        DisplayChoices();
    }


   public void EnterDialogueMode(TextAsset inkJSON)
{
    Debug.Log("EnterDialogueMode called");
    currentStory = new Ink.Runtime.Story(inkJSON.text);
    dialoguePanel.SetActive(true);
    dialogueIsPlaying = true;
    gameManager.playerCanMove = false; // Freeze player during dialogue
    levelManager.DisablePlayerSound(); // Disable player sounds
    Debug.Log("Player can move: " + gameManager.playerCanMove);

    ContinueStory();
}

    public void ExitDialogueMode()
    {
        Debug.Log("Exiting dialogue mode triggered");
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        gameManager.playerCanMove = true; // Unfreeze player after dialogue
        levelManager.EnablePlayerSound(); // Enable player sounds
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
    
    Debug.Log("Calling ContinueStory. Can continue: " + currentStory.canContinue);

    if (displayLineCoroutine != null)
{
    StopCoroutine(displayLineCoroutine);
}
    
    if (currentStory.canContinue)
    {
        // Old way of displaying text all at once 
        // dialogueText.text = currentStory.Continue();

        // Code to stop previous cowritten, allowing us to skip to end of line with input
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
        }

       displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));

       Invoke("PlayDialogueAudio", 0.2f);
    }
    //    Log all tags at each point in the story
       foreach (var tag in currentStory.currentTags)
       {
           Debug.Log("Tag found: " + tag);

            // TAG: Unfreeze player
            if (tag == "unfreeze")
            {
                gameManager.playerCanMove = true;
                Debug.Log("Player can move: " + gameManager.playerCanMove);
            }
            
            // TAG: Wait for player to jump off edge
            if (tag == "wait_for_leap")
            {
            StartCoroutine(WaitForLeap());
            CanAdvanceDialogue = false; // Ensure game waits for event
            }
        }
}

private void PlayDialogueAudio()
{
    AudioClip[] dialogueClips = { dialoguePlaying1, dialoguePlaying2, dialoguePlaying3 };

    if (currentDialogueSound >= dialogueClips.Length)
    {
        currentDialogueSound = 0; // Reset to the first clip after 3 lines
    }

    audioSource.PlayOneShot(dialogueClips[currentDialogueSound], 0.1f);
    currentDialogueSound++;
}


private void DisplayChoices()
{
    List<Ink.Runtime.Choice> currentChoices = currentStory.currentChoices;

    Debug.Log("Total choices available: " + currentChoices.Count); // Verify the number of choices
    foreach (var choice in currentChoices)
    {
        Debug.Log("Choice: " + choice.text); // Print each choice text
    }

    int index = 0;
    foreach (Ink.Runtime.Choice choice in currentChoices)
    {
        choices[index].gameObject.SetActive(true);
        choicesText[index].text = choice.text;
        Debug.Log($"Enabling choice button {index} with text: {choice.text}");
        index++;
    }

    for (int i = index; i < choices.Length; i++)
    {
        Debug.Log($"Disabling unused choice button {i}");
        choices[i].gameObject.SetActive(false);
    }

    if (currentChoices.Count > 0)
{
    StartCoroutine(SelectFirstChoice());
}
}
    private System.Collections.IEnumerator SelectFirstChoice()
    {
        Debug.Log("Selecting the first choice button");
        // Event System requires we clear it first, then Wait
        // for at least one frame before setting the current selected game object
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    // private System.Collections.IEnumerator WaitBeforeExit()
    // {
    // Debug.Log("Waiting for user input to exit...");
    // yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X)); // Wait for user input
    // Debug.Log("User input detected, exiting dialogue");
    // ExitDialogueMode();
    // }

   public void MakeChoice(int choiceIndex)
{
    Debug.Log($"MakeChoice called with index: {choiceIndex}");
    
    if (currentStory.currentChoices.Count > choiceIndex)
    {
        Debug.Log($"Choosing: {currentStory.currentChoices[choiceIndex].text}");
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
    else
    {
        Debug.LogError("Invalid choice index! Choice selection failed.");
    }
}

public void LoadNextScene()
{
    levelManager.LoadNextScene();
    Debug.Log("Loading next scene...");
}

void PlayEndLevelSound()
{
   audioSource.PlayOneShot(endLevel, 0.4f);
}


//// Event-specific coroutines

// Wait for player to leap off edge at end of level
    private System.Collections.IEnumerator WaitForLeap() // Wait for player to leap off edge
    {
        while (!LevelManager.Instance.PlayerLeapt)
        {
            yield return null;  // Keep waiting until the rock has hit the ground
        }
            // Access player script to prevent respawn
            player.preventRespawn = true;
            Debug.Log("Player has leapt off-screen, preventing respawn.");
            
            //Play next line in story
            Debug.Log("Continuing story");
            ContinueStory();
            
            // Fade to black
            Debug.Log("Player has leapt off-screen, fading to black...");
            FindFirstObjectByType<FadeController>().FadeToBlack(3f);

            // Fade down music
            levelManager.StartMusicFade(); // Assuming the method requires two float arguments

            // Exit dialogue
            Invoke("ExitDialogueMode", 4f);

            // Disable crusher sounds
            levelManager.DisableCrusher();

            //Disable snail enemy
            levelManager.DisableSnailEnemy();


            // Play end sounds
           Invoke("PlayEndLevelSound", 4f);


           // Invoke LoadNextLevel from scene Manager
           Invoke("LoadNextScene", 32f);

    }

}



