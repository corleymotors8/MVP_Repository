using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    
    [Header("Ball Handling")]
    public float ballDetectionRadius = 3f; // Should match the ball's detection radius
    
    [Header("Shooting")]
    public Transform shootPosition;
    public Transform targetHoop;
    public float throwForce = 8f;
    public float accuracyVariance = 1.5f; // Higher = more misses
    
    [Header("State Machine")]
    public float shootingDelay = 1f; // Time to wait at shoot position before throwing
    
    // References
    private GameObject ballObject;
    private BallController heldBall;
    private Rigidbody2D rb;
    private float originalScaleX;
    private bool recentlyThrown = false;

    
    // State tracking
    private enum EnemyState { SeekingBall, MovingToShootPosition, Shooting, Idle }
    private EnemyState currentState = EnemyState.Idle;
    private bool forcedHoldingBall = false;
    
    private void Start()
    {
         originalScaleX = Mathf.Abs(transform.localScale.x);
        rb = GetComponent<Rigidbody2D>();
        // Start by looking for a ball
        FindBall();
    }
    
    private void Update()
    {       
         // Facing direction logic
    if (rb.linearVelocity.x < 0)
    {
        // Moving left
        transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
    }
    else if (rb.linearVelocity.x > 0)
    {
        // Moving right
        transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
    }
        
        // Always try to find the ball if we don't have one
        if (ballObject == null)
        {
            FindBall();
            return;
        }
        
  switch (currentState)
{
    case EnemyState.Idle:
        Debug.Log("Enemy State: Idle");
        if (ballObject != null)
        {
            Debug.Log("Enemy transitioning from Idle to SeekingBall");
            currentState = EnemyState.SeekingBall;
        }
        break;
    
    case EnemyState.SeekingBall:
        Debug.Log("Enemy State: SeekingBall");
        SeekBall();
        break;
    
    case EnemyState.MovingToShootPosition:
        Debug.Log("Enemy State: MovingToShootPosition");
        MoveToShootPosition();
        break;
    
    case EnemyState.Shooting:
        Debug.Log("Enemy State: Shooting");
        break;
}
}
    
    // Find any ball in the scene with tag "Ball"
    private void FindBall()
    {        
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballObject = ball;
            currentState = EnemyState.SeekingBall;
        }
    }
    
    // Check if we're holding a ball by checking if the ball's position matches ours
   private bool IsHoldingBall()
{
    if (recentlyThrown) return false;
    
    // If we forced holding state, always return true
    if (forcedHoldingBall) return true;
    
    if (ballObject == null) return false;
    
    BallController ballController = ballObject.GetComponent<BallController>();
    return ballController != null && Vector2.Distance(ballObject.transform.position, transform.position) < 2f;
}
    
    private void SeekBall()
    {
   if (ballObject == null)
    {
        Debug.Log("No ball found, returning to Idle");
        currentState = EnemyState.Idle;
        return;
    }
    
    // Debug.Log($"Seeking ball. Distance to ball: {Vector2.Distance(ballObject.transform.position, transform.position)}");
    
    if (IsHoldingBall())
    {
        // Debug.Log("Is Holding Ball - Attempting to move to shoot position");
        heldBall = ballObject.GetComponent<BallController>();
        currentState = EnemyState.MovingToShootPosition;
        forcedHoldingBall = true;
        return;
    }
        
        // Calculate direction to ball
        Vector2 directionToBall = ballObject.transform.position - transform.position;
        float distanceToBall = directionToBall.magnitude;
        
        // Normalize for movement
        directionToBall.Normalize();
        
        // Move towards ball
        rb.linearVelocity = directionToBall * moveSpeed;

        if (directionToBall.x < 0)
        {
        // Moving left
        transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        }
        else
        {
        // Moving right
        transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
        }
         
        // The ball's own script will handle grabbing when we're close enough
    }
    
    private void MoveToShootPosition()
    {
    // Debug.Log($"Moving to shoot position. Current position: {transform.position}, Target: {shootPosition.position}");
        if (shootPosition == null)
        {
            Debug.LogError("Enemy: No shoot position assigned!");
            currentState = EnemyState.Idle;
            return;
        }
        
        // Calculate direction to shooting position
        Vector2 directionToShootPos = shootPosition.position - transform.position;
        float distanceToShootPos = directionToShootPos.magnitude;
        
        // Normalize for movement
        directionToShootPos.Normalize();
        
        // Move towards shooting position
        // Debug.Log("Enemy: Actually moving to shoot position");
        rb.linearVelocity = directionToShootPos * moveSpeed;

        
        
        // Check if at shooting position
        if (distanceToShootPos < 1f) // Changed from 0.5f to 1f
{
    // Debug.Log($"Reached shoot position. Distance was: {distanceToShootPos}");
    rb.linearVelocity = Vector2.zero;
    currentState = EnemyState.Shooting;
    StartCoroutine(ShootBall());
}
    }
    
    private IEnumerator ShootBall()
    {
   // Determine if we need to flip based on current facing and hoop position
    if (targetHoop != null)
    {
        if (targetHoop.position.x < transform.position.x && transform.localScale.x < 0)
        {
            // Hoop is to the left, but enemy is facing right - flip to left
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (targetHoop.position.x > transform.position.x && transform.localScale.x > 0)
        {
            // Hoop is to the right, but enemy is facing left - flip to right
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    // Stop moving
    rb.linearVelocity = Vector2.zero;

    // Wait before throwing
    yield return new WaitForSeconds(0.5f);

    // Stop moving
    rb.linearVelocity = Vector2.zero;

    // Wait before throwing
    yield return new WaitForSeconds(0.5f);

        // Wait a moment before shooting
        yield return new WaitForSeconds(shootingDelay);
        
        // Double check we still have the ball
        if (IsHoldingBall() && heldBall != null && targetHoop != null)
        {
            Debug.Log("Conditions met for shooting ball");
            // Calculate direction to hoop with some randomness for accuracy
            Vector2 directionToHoop = targetHoop.position - transform.position;
            
            // Add random variance to the shot
            directionToHoop += new Vector2(
                Random.Range(-accuracyVariance, accuracyVariance),
                Random.Range(-accuracyVariance, accuracyVariance)
            );
            
            // Normalize and apply throw force
            directionToHoop.Normalize();
            Vector2 throwVector = directionToHoop * throwForce;
            
            // Use the ball's existing throw method
            heldBall.ThrowWithForce(throwVector);

            recentlyThrown = true;
            StartCoroutine(ResetThrowCooldown());


            // Reset state
            forcedHoldingBall = false;
            heldBall = null;
            ballObject = null;
            currentState = EnemyState.Idle;
            
            // Give the enemy a moment before seeking the ball again
            yield return new WaitForSeconds(1f);
            FindBall();
        }
        else
        {
            Debug.Log($"Shoot conditions not met. Holding ball: {IsHoldingBall()}, Held Ball: {heldBall}, Target Hoop: {targetHoop}");

            currentState = EnemyState.Idle;
        }
    }

    public void SetHeldBall(BallController ball)
{
    Debug.Log("SetHeldBall called. Updating ball and state.");
    heldBall = ball;
    ballObject = ball.gameObject;

    // Explicitly update the state to MovingToShootPosition
    currentState = EnemyState.MovingToShootPosition;
    forcedHoldingBall = true;
}
    
    // This will tag the enemy so the ball knows it's an enemy
    private void OnEnable()
    {
        // Make sure this object has the "Enemy" tag so the ball can detect it
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning("EnemyController: This GameObject should have the 'Enemy' tag for the ball to detect it.");
        }
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Show ball detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ballDetectionRadius);
        
        // Show shoot position if set
        if (shootPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(shootPosition.position, 0.3f);
            Gizmos.DrawLine(transform.position, shootPosition.position);
        }
        
        // Show target hoop if set
        if (targetHoop != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetHoop.position);
        }
    }

    private IEnumerator ResetThrowCooldown()
{
    yield return new WaitForSeconds(1.5f); // Adjust cooldown time as needed
    recentlyThrown = false;
}

    
}