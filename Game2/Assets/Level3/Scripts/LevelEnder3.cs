using UnityEngine;

public class LevelEnder3 : MonoBehaviour
{

// Access Level3Manager
    Level3Manager level3Manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Access Level3Manager
        level3Manager = FindFirstObjectByType<Level3Manager>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("LevelEnder3 triggered");
            level3Manager.EndLevel();
        }
    }
}
