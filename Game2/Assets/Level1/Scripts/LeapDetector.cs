using UnityEngine;

public class LeapDetector : MonoBehaviour
{

    // Get LevelManager script
    private LevelManager levelManager;
    private DialogueTrigger dialogueTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        dialogueTrigger = FindFirstObjectByType<DialogueTrigger>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void OnCollisionEnter2D (Collision2D other)
    {
        if (other.gameObject.tag == "Player" && dialogueTrigger.canTriggerLeapt)
        {
            Debug.Log("Player has entered the trigger zone -- PlayerLeapt = true");
            levelManager.PlayerLeapt = true;
        }
    }
}
