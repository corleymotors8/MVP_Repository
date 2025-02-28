using UnityEngine;

public class BallController : MonoBehaviour
{
    // References
    private GameObject player;
    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private bool isGrabbed = false;
    
    // Settings
    public float detectionRadius = 3f;
    public float heightOffset = 2f;
    
    // Store original physics materials
    private PhysicsMaterial2D originalMaterial;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<Collider2D>();
        
        // Store original physics material
        if (ballCollider != null)
        {
            originalMaterial = ballCollider.sharedMaterial;
        }
        
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private float lastReleaseTime = 0f;
    private float grabCooldown = 1f; // Wait 1 second before allowing re-grab
    
    void Update()
    {
        if (player == null) return;
        
        if (!isGrabbed)
        {
            // Only check for grab if cooldown has expired
            if (Time.time > lastReleaseTime + grabCooldown)
            {
                // Check if player is close enough to grab
                float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                
                if (distanceToPlayer <= detectionRadius)
                {
                    GrabBall();
                }
            }
        }
        else
        {
            // Position the ball above the player, maintaining Z position
            Vector3 newPosition = player.transform.position;
            newPosition.y += heightOffset;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
            
        }
    }
    
    private void GrabBall()
    {
        isGrabbed = true;
        
        // Disable physics when grabbed
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Temporarily remove physics material to disable bounce
        if (ballCollider != null)
        {
            // Either disable collider or set to trigger to prevent physics interaction
            ballCollider.isTrigger = true;
        }

        player.GetComponent<Player>().SetHeldBall(this);
    }
    
    // Optional: Method to release the ball if you need it later
    public void ReleaseBall()
    {
        Debug.Log("ReleaseBall method called");
        isGrabbed = false;
        lastReleaseTime = Time.time; // Set the release time for cooldown
        
        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        Debug.Log("Set RigidBody to Dynamic");
        
        // Restore original physics material
        if (ballCollider != null)
        {
            ballCollider.sharedMaterial = originalMaterial;
            ballCollider.isTrigger = false;
            Debug.Log("Collider trigger disabled, restored material");
        }
    }
    
    // Throw the ball in the direction the player is facing
    // private void ThrowBall()
    // {
    //     ReleaseBall();
        
        
    //     // Determine throw direction (assuming player might have a script that tracks facing)
    //     Vector2 throwDirection = Vector2.right; // Default right
        
    //     // If player has a component indicating direction, use that instead
    //     if (player.GetComponent<Player>() != null && player.GetComponent<Player>().isFacingRight == false)
    //     {
    //         throwDirection = Vector2.left;
    //     }
    //     else
    //     {
    //     }
        
    //     // Apply force at 45-degree angle
    //     Vector2 throwForce = new Vector2(throwDirection.x, 1f).normalized * 10f;
    //     Debug.Log("Applying force: " + throwForce);
    //     rb.AddForce(throwForce, ForceMode2D.Impulse);
        
    //     // Check velocity after force is applied
    //     Debug.Log("Ball velocity after throw: " + rb.linearVelocity);
    // }

    public void ThrowWithForce(Vector2 force)
{
    ReleaseBall();
    rb.AddForce(force, ForceMode2D.Impulse);
}
    
    // Visualize the grab radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}