using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraFollowFall : MonoBehaviour 
{
    public float followSpeedMultiplier = 1.0f;
    public float fallGravity = 4f;

    private Transform player;
    private Rigidbody2D playerRb;
    private Camera mainCamera;
    private float originalGravity;
    private bool isFollowing = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            
            // Get the player's root GameObject
            GameObject playerRoot = other.gameObject;
            playerRb = playerRoot.GetComponent<Rigidbody2D>();

           


            if (playerRb != null)
            {
                playerRb.gravityScale = fallGravity;
                
                // Verify the change immediately
                
                isFollowing = true;
            }
            else
            {
                Debug.LogError("Could not find Rigidbody2D anywhere in player hierarchy!");
            }

            // Store transform for camera following
            player = other.transform;
            
            // Try to get and set Player script
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.preventRespawn = true;
                //Set animator FallingFast to true
                playerScript.animator.SetBool("FallingFast", true);
            }
            else
            {
                Debug.LogWarning("No Player script found!");
            }
        }
    }

     void LateUpdate()
    {
        if (isFollowing && player != null && playerRb != null)
        {
            // Periodically verify gravity scale hasn't been changed by something else
            if (playerRb.gravityScale != fallGravity)
            {
                playerRb.gravityScale = fallGravity;
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, Mathf.Max(playerRb.linearVelocity.y, -5f)); // -5f is max fall speed


            }

            if (playerRb.linearVelocity.y < 0)  // Changed from linearVelocity to velocity
            {
                Vector3 currentCameraPos = mainCamera.transform.position;
                float newY = currentCameraPos.y + (playerRb.linearVelocity.y * followSpeedMultiplier * Time.deltaTime);
                mainCamera.transform.position = new Vector3(currentCameraPos.x, newY, currentCameraPos.z);
            }
        }
    }
}