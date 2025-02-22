using UnityEngine;

public class WaitForPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Access MovingPlatorm script and enabled
            MovingPlatform movingPlatform = GetComponent<MovingPlatform>();
            if (movingPlatform != null)
            {
                movingPlatform.enabled = true;
            }
        }
    }
}
