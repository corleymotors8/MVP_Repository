using UnityEngine;
using TMPro;
using Ink.Parsed;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine.EventSystems;




public class DialogueManager3 : MonoBehaviour
{
  [Header("Params")]
  [SerializeField] private float typingSpeed = 0.04f; // Smaller values = faster typing
  [Header("Dialogue UI")]
  [SerializeField] private GameObject dialoguePanel;
  [SerializeField] private GameObject continueIcon;
  [SerializeField] private TextMeshProUGUI dialogueText;
  [SerializeField] private GameObject enterPrompt;
//   [SerializeField] private GameObject exitPrompt;

  [Header("Choices UI")]
  [SerializeField] private GameObject[] choices;

  private TextMeshProUGUI[] choicesText;
  public Ink.Runtime.Story currentStory;

  public bool dialogueIsPlaying { get; private set; }

  private Coroutine displayLineCoroutine;

  private bool CanAdvanceDialogue = true;
 
  // Handle NPC talking sounds
  AudioSource audioSource;

  private static DialogueManager3 instance;
  // Declare gameManager
   private GameManager gameManager;
  // Declare crusher
   private Crusher crusher;

  private void Awake()
  {
     if (instance != null)
     {
      Debug.LogWarning("More than one instance of DialogueManager2 found!");
     }
      instance = this;
  }



  public static DialogueManager3 GetInstance()
  {
      return instance;
  }




  private void Start()
  {
      enterPrompt.SetActive(false);
    //   exitPrompt.SetActive(false);
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
      enterPrompt.SetActive(false);
      HideChoices();
    
      // Display letters one at a time
      foreach (char letter in line.ToCharArray())
      {
          dialogueText.text += letter;
          yield return new WaitForSeconds(typingSpeed);
      }

      // Show continue icon when text is done typing
      continueIcon.SetActive(true);
      DisplayChoices();
      enterPrompt.SetActive(true);
  }

 public void EnterDialogueMode(TextAsset inkJSON)
{
  Debug.Log("EnterDialogueMode called");
  currentStory = new Ink.Runtime.Story(inkJSON.text);
  dialoguePanel.SetActive(true);
  dialogueIsPlaying = true;
  gameManager.playerCanMove = false;

  ContinueStory();
}

  public void ExitDialogueMode()
  {
     
      dialogueIsPlaying = false;
      dialoguePanel.SetActive(false);
       // Hide both prompts when dialogue ends
       enterPrompt.SetActive(false);
    //    exitPrompt.SetActive(false);
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
  Debug.Log("Calling ContinueStory. Can continue: " + currentStory.canContinue);

  if (currentStory.canContinue)
  {
      if (displayLineCoroutine != null)
      {
          StopCoroutine(displayLineCoroutine);
      }

     displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));
    Invoke("PlayDialogueAudio", 0.2f);


     
      // Show "Press Enter to Continue" prompt
       // enterPrompt.SetActive(true);
    //    exitPrompt.SetActive(false);
 }
 else if (currentStory.currentChoices.Count == 0)
  {
     Debug.Log("No more choices, exiting dialogue mode from ContinueStory.");
     StartCoroutine(WaitBeforeExit());
      // Show "Press X to Exit" prompt
       enterPrompt.SetActive(false);
       // exitPrompt.SetActive(true);
  }


//    Log all tags at each point in the story
      foreach (var tag in currentStory.currentTags)
      {
          Debug.Log("Tag found: " + tag);


        //    // TAG: NPC1 talking
        //    if (tag == "npc1")
        //    {
        //        npc1Talking = true;
        //    }
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


/// Level-specific coroutines



}
