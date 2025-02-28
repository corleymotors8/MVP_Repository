using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFlightPathfinding : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float moveSpeed = 3f;
    public float reachDistance = 0.5f;
    
    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;
    public float raycastDistance = 2f;
    public float avoidanceStrength = 2f;
    public float steeringSpeed = 3f;
    public float minHeightAboveGround = 1f;
    
    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.5f;
    public float stuckDistance = 0.2f;
    public float stuckTimeout = 2.0f;
    public float clearPathPriority = 3.0f;
    
    [Header("Debug")]
    public bool showDebugRays = true;
    public Color rayHitColor = Color.red;
    public Color rayClearColor = Color.green;
    public Color stuckRayColor = Color.yellow;

    // Escape detection
    private float escapeTimer = 0f;
    private float escapeCooldown = 2f; // Time to follow escape path
    
    // Movement variables
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Vector2 desiredDirection;
    
    // Stuck detection variables
    private Vector2 lastPosition;
    private float timeSinceLastCheck = 0f;
    private float stuckTime = 0f;
    private bool isStuck = false;
    private Vector2 lastClearDirection = Vector2.zero;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0; // Flying enemies shouldn't have gravity
        }
    }
    
    void Start()
    {
        if (target == null)
        {
            // Try to find player if no target is set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No target found for pathfinding!");
            }
        }
        
        // Configure the rigidbody for better movement
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent unwanted rotation
            }
        }
        
        // Initialize direction
        if (target != null)
        {
            desiredDirection = (target.position - transform.position).normalized;
            currentDirection = desiredDirection;
        }
        
        // Increase raycast distance to detect obstacles earlier
        if (raycastDistance < 3f)
        {
            raycastDistance = 3f;
            Debug.Log("Increased raycast distance to 3 for better obstacle detection");
        }
        
        // Initialize stuck detection
        lastPosition = transform.position;
    }
    
    void Update()
    {
        // Press E key to force escape behavior for testing
if (Input.GetKeyDown(KeyCode.E))
{
    // Force random escape direction
    lastClearDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
    isStuck = true;
    stuckTime = stuckTimeout + 1f;
    Debug.Log("Forcing escape in direction: " + lastClearDirection);
}
        
        if (target == null) return;
        
        // Check for stuck behavior
        CheckIfStuck();
        
        // Check if we've reached the target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget <= reachDistance)
        {
            // Target reached
            rb.linearVelocity = Vector2.zero;
            isStuck = false;
            stuckTime = 0f;
            
            if (showDebugRays)
            {
                Debug.DrawLine(transform.position, target.position, Color.yellow, 0.1f);
                Debug.Log("Target reached! Distance: " + distanceToTarget);
            }
            return;
        }
        
       // Check if we're in escape cooldown
        if (escapeTimer > 0)
        {
            escapeTimer -= Time.deltaTime;
            // Just maintain the last clear direction during cooldown
            desiredDirection = lastClearDirection;
    
            if (showDebugRays)
            {
            Debug.DrawRay(transform.position, desiredDirection * 3f, Color.magenta);
            }
            }
        else
        {
        // Normal targeting behavior
        desiredDirection = (target.position - transform.position).normalized;
        }
        
        // Detect obstacles and adjust path
        Vector2 avoidanceDirection = DetectAndAvoidObstacles();
        
        // Apply movement logic based on stuck state
        if (isStuck && lastClearDirection != Vector2.zero)
        {
            // When stuck, prioritize the last known clear direction
            currentDirection = Vector2.Lerp(currentDirection, lastClearDirection, Time.deltaTime * steeringSpeed * 2);
            
            if (showDebugRays)
            {
                // Show the escape direction
                Debug.DrawRay(transform.position, lastClearDirection * 3f, stuckRayColor);
                Debug.DrawLine(transform.position, (Vector2)transform.position + lastClearDirection * 3f, stuckRayColor);
            }
        }
        else
        {
            // Normal steering behavior
            currentDirection = Vector2.Lerp(currentDirection, avoidanceDirection, Time.deltaTime * steeringSpeed);
        }
        
        // Apply movement
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            // For dynamic body, use forces
            rb.gravityScale = 0;
            
            rb.AddForce(currentDirection * moveSpeed * 2f, ForceMode2D.Force);
            
            // Limit maximum velocity
            if (rb.linearVelocity.magnitude > moveSpeed * (isStuck ? 1.5f : 1f))
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * (isStuck ? 1.5f : 1f);
            }
        }
        else
        {
            // For kinematic, directly set velocity
            rb.linearVelocity = currentDirection * moveSpeed * (isStuck ? 1.5f : 1f);
        }
        
        // Draw debug lines
        if (showDebugRays)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + desiredDirection * 2f, Color.blue);
            Debug.DrawLine(transform.position, (Vector2)transform.position + currentDirection * 2f, new Color(1f, 0.5f, 0f)); // Orange
        }
        
        // Update facing direction (if the velocity is significant)
        if (Mathf.Abs(rb.linearVelocity.x) > 0.5f)
        {
            // Preserve original y and z scale, only flip the x component
            float xScale = Mathf.Abs(transform.localScale.x) * Mathf.Sign(rb.linearVelocity.x);
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }

        // Near the end of the method, after updating lastPosition
        float distanceMoved = Vector2.Distance(lastPosition, transform.position);

        Debug.Log($"Distance moved: {distanceMoved}, Stuck time: {stuckTime}, Is stuck: {isStuck}");
    }
    
    private void CheckIfStuck()
    {
        // Update check timer
        timeSinceLastCheck += Time.deltaTime;
        
        if (timeSinceLastCheck >= stuckCheckInterval)
        {
            timeSinceLastCheck = 0f;
            
            // Check distance moved since last check
            float distanceMoved = Vector2.Distance(lastPosition, transform.position);
            
            if (distanceMoved < stuckDistance)
            {
                // We haven't moved much, might be stuck
                stuckTime += stuckCheckInterval;
                
                if (stuckTime >= stuckTimeout && !isStuck)
                {
                    // We're officially stuck
                    isStuck = true;
                    Debug.Log("Entity is stuck! Activating escape behavior.");
                }
            }
            else
            {
                // We've moved enough, reset stuck timer
                stuckTime = 0f;
                if (isStuck)
                {
                    isStuck = false;
                    Debug.Log("Entity has escaped!");
                }
            }
            
            // Update last position
            lastPosition = transform.position;
        }
    }
    
    private Vector2 DetectAndAvoidObstacles()
    {
        // Start with the desired direction toward target
        Vector2 adjustedDirection = desiredDirection;
        
        // Cast rays in multiple directions to detect obstacles
        bool obstacleDetected = false;
        Vector2 avoidanceVector = Vector2.zero;
        
        // Track clear paths for stuck detection
        List<Vector2> clearPaths = new List<Vector2>();
        
        // Cast multiple rays from different positions on the body
        Vector2[] rayOrigins = {
            (Vector2)transform.position,
            (Vector2)transform.position + Vector2.up * 0.5f,
            (Vector2)transform.position + Vector2.down * 0.5f,
            (Vector2)transform.position + Vector2.left * 0.5f,
            (Vector2)transform.position + Vector2.right * 0.5f
        };
        
        foreach (Vector2 origin in rayOrigins)
        {
            // Forward ray from this origin
            RaycastHit2D forwardHit = Physics2D.Raycast(origin, desiredDirection, raycastDistance, obstacleLayer);
            if (forwardHit.collider != null)
            {
                if (showDebugRays) Debug.DrawRay(origin, desiredDirection * raycastDistance, rayHitColor);
                obstacleDetected = true;
                
                // Calculate avoidance vector based on hit normal
                avoidanceVector += (Vector2)Vector3.Reflect(desiredDirection, forwardHit.normal);
                
                // Add stronger avoidance for close obstacles
                float closenessMultiplier = 1f - (forwardHit.distance / raycastDistance);
                avoidanceVector += -desiredDirection * closenessMultiplier * 2f;
            }
            else if (showDebugRays)
            {
                Debug.DrawRay(origin, desiredDirection * raycastDistance, rayClearColor);
                // Track this as a clear path
                clearPaths.Add(desiredDirection);
            }
        }
        
        // If any obstacles detected, try additional angle rays from center
        if (obstacleDetected)
        {
            TryCastAngleRays(ref avoidanceVector, ref obstacleDetected, ref clearPaths);
        }
        
        // Apply ground avoidance to prevent getting too close to the ground
        ApplyGroundAvoidance(ref avoidanceVector, ref obstacleDetected, ref clearPaths);
        
        // If obstacle detected, adjust direction
        if (obstacleDetected)
        {
            avoidanceVector.Normalize();
            adjustedDirection = Vector2.Lerp(desiredDirection, avoidanceVector, avoidanceStrength);
            adjustedDirection.Normalize();
        }
        
        // Check if we're stuck and have clear paths available
        if (isStuck && clearPaths.Count > 0)
        {
            // Find the most promising clear path
            Vector2 bestClearPath = Vector2.zero;
            float bestScore = -1f;
            
            foreach (Vector2 path in clearPaths)
            {
                // Score based on similarity to desired direction and difference from current stuck direction
                float alignmentScore = Vector2.Dot(path, desiredDirection) * 0.5f + 0.5f; // 0-1 range
                float noveltyScore = 1f - (Vector2.Dot(path, -rb.linearVelocity.normalized) * 0.5f + 0.5f); // Prefer paths different from current stuck direction
                float horizontalScore = Mathf.Abs(path.x) / (Mathf.Abs(path.x) + Mathf.Abs(path.y)); // Higher for more horizontal paths
                float score = alignmentScore * 0.2f + noveltyScore * 0.3f + horizontalScore * 0.5f; // Weight horizontal movement highest
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestClearPath = path;
                }
            }
            
            if (bestClearPath != Vector2.zero)
            {
                lastClearDirection = bestClearPath;
                
                // When stuck, prioritize clear paths
                adjustedDirection = Vector2.Lerp(adjustedDirection, bestClearPath, clearPathPriority * Time.deltaTime);
                
                if (showDebugRays)
                {
                    // Draw the chosen path with a special color
                    Debug.DrawRay(transform.position, bestClearPath * raycastDistance * 1.5f, stuckRayColor);
                }
            }
        }
        Debug.Log($"Found {clearPaths.Count} clear paths, Is stuck: {isStuck}, Last clear direction: {lastClearDirection}");
        return adjustedDirection;
    }
    
    private void TryCastAngleRays(ref Vector2 avoidanceVector, ref bool obstacleDetected, ref List<Vector2> clearPaths)
    {
        // Try a few angles to find a clear path
        float[] angles = { 45f, 90f, 135f, -45f, -90f, -135f };
        bool foundClearPath = false;
        
        foreach (float angle in angles)
        {
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * desiredDirection;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, raycastDistance, obstacleLayer);
            
            if (hit.collider == null)
            {
                // Found clear path
                if (showDebugRays) Debug.DrawRay(transform.position, rayDirection * raycastDistance, rayClearColor);
                avoidanceVector += rayDirection * 2; // Prioritize this direction
                
                // Add to clear paths for stuck detection
                clearPaths.Add(rayDirection);
                
                // Remember this direction as an escape route
                if (isStuck && lastClearDirection == Vector2.zero)
                {
                    lastClearDirection = rayDirection;
                }
            }
            else if (showDebugRays)
            {
                Debug.DrawRay(transform.position, rayDirection * raycastDistance, rayHitColor);
                // Add a weak avoidance away from this obstacle too
                avoidanceVector += (Vector2)Vector3.Reflect(rayDirection, hit.normal) * 0.5f;
            }
        }
        
        if (!foundClearPath)
        {
            // If no clear path in those directions, try up
            Vector2 upRayDir = Vector2.up;
            RaycastHit2D upHit = Physics2D.Raycast(transform.position, upRayDir, raycastDistance, obstacleLayer);
            
            if (upHit.collider == null)
            {
                if (showDebugRays) Debug.DrawRay(transform.position, upRayDir * raycastDistance, rayClearColor);
                avoidanceVector += upRayDir * 3; // Strongly prioritize going up to avoid obstacles
                
                // Add to clear paths
                clearPaths.Add(upRayDir);
                
                // Remember this direction as an escape route
                if (isStuck && lastClearDirection == Vector2.zero)
                {
                    lastClearDirection = upRayDir;
                }
            }
            else if (showDebugRays)
            {
                Debug.DrawRay(transform.position, upRayDir * raycastDistance, rayHitColor);
            }
        }
    }
    
    private void ApplyGroundAvoidance(ref Vector2 avoidanceVector, ref bool obstacleDetected, ref List<Vector2> clearPaths)
    {
        // Cast ray downward to detect ground
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, minHeightAboveGround, obstacleLayer);
        
        if (groundHit.collider != null)
        {
            if (showDebugRays) Debug.DrawRay(transform.position, Vector2.down * minHeightAboveGround, rayHitColor);
            obstacleDetected = true;
            
            // Add upward force to avoid ground
            float groundFactor = 1.0f - (groundHit.distance / minHeightAboveGround);
            avoidanceVector += Vector2.up * groundFactor * 3.0f;
        }
        else
        {
            if (showDebugRays) Debug.DrawRay(transform.position, Vector2.down * minHeightAboveGround, rayClearColor);
            // Add down to clear paths
            clearPaths.Add(Vector2.down);
        }
        
        // Also cast up to check for ceiling
        RaycastHit2D ceilingHit = Physics2D.Raycast(transform.position, Vector2.up, minHeightAboveGround, obstacleLayer);
        
        if (ceilingHit.collider != null)
        {
            if (showDebugRays) Debug.DrawRay(transform.position, Vector2.up * minHeightAboveGround, rayHitColor);
            obstacleDetected = true;
            
            // Add downward force to avoid ceiling
            float ceilingFactor = 1.0f - (ceilingHit.distance / minHeightAboveGround);
            avoidanceVector += Vector2.down * ceilingFactor * 2.0f;
        }
        else
        {
            if (showDebugRays) Debug.DrawRay(transform.position, Vector2.up * minHeightAboveGround, rayClearColor);
            // Add up to clear paths
            clearPaths.Add(Vector2.up);
        }
    }
    
    // Handle collisions for additional escape mechanism
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"Collision detected, Is stuck: {isStuck}, Layer check: {collision.gameObject.layer == Mathf.Log(obstacleLayer.value, 2)}");


        
        if (isStuck && collision.gameObject.layer == Mathf.Log(obstacleLayer.value, 2))
        {
            // We're stuck and in contact with an obstacle - apply an emergency escape force
            Vector2 contactNormal = collision.contacts[0].normal;
            Vector2 escapeDirection = contactNormal.normalized;
            
            // Apply a strong impulse in the escape direction
            rb.AddForce(escapeDirection * moveSpeed * 50f, ForceMode2D.Impulse);

            
            if (showDebugRays)
            {
                Debug.DrawRay(collision.contacts[0].point, escapeDirection * 2f, Color.red, 0.5f);
                Debug.Log("Emergency escape applied in direction: " + escapeDirection);
            }
            
            // Reset stuck state after applying emergency escape
            stuckTime = 0f;
        }
    }
    
    // Reset stuck detection when hitting something with significant force
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > moveSpeed * 0.8f)
        {
            // Reset the stuck timer if we hit something with force
            stuckTime *= 0.5f;
            // Activate escape cooldown
            escapeTimer = escapeCooldown;
        }
    }
}