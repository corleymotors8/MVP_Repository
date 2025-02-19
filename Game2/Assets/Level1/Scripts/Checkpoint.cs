using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private GameManager gameManager;
    public bool isReached = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Access GameManager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {   
        if (other.CompareTag("Player"))
        {
            if (!isReached)
            {
            // Set respawn point to object's current transform in GameManager
                if (gameManager != null)
                {
                gameManager.respawnPosition = transform.position;
                Debug.Log("Checkpoint reached, setting respawn to: " + gameManager.respawnPosition);
                isReached = true;
                }
            }
        }
    }
}
