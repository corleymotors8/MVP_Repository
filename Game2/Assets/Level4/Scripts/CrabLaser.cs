using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CrabLaserAttack : MonoBehaviour
{
   [Header("Laser Settings")]
   [SerializeField] private Transform laserOrigin; // Position where laser starts (on cap)
   [SerializeField] private float maxLaserDistance = 10f;
   [SerializeField] private float minAngle = -45f; // Starting angle (pointing down)
   [SerializeField] private float maxAngle = 30f;  // Max angle when fully charged
   [SerializeField] private float maxChargeTime = 1.5f; // Time to reach max angle
   [SerializeField] private float laserDuration = 0.5f; // How long laser stays active
   [SerializeField] private float cooldownTime = 0.8f; // Time before can fire again
  
   [Header("Laser Appearance")]
   [SerializeField] private float laserWidth = 0.1f;
   [SerializeField] private Material laserMaterial;
   [SerializeField] private Gradient laserColor;


   [Header("Laser Sounds")]
   [SerializeField] private AudioClip chargingSound;
   [SerializeField] private AudioClip firingSound;
   [SerializeField] private AudioClip knockbackSound;

   AudioSource audioSource;
  
   [Header("Effects")]
   [SerializeField] private GameObject chargingEffectPrefab;
   [SerializeField] private GameObject impactEffectPrefab;


   [Header("Damage Settings")]
   [SerializeField] private float baseLaserDamage = 10f;
   [SerializeField] private float maxLaserDamage = 30f;
   
   [Header("Knockback Settings")]
   [SerializeField] private float baseKnockbackForce = 5f; // Base knockback force
   [SerializeField] private float maxKnockbackForce = 15f; // Knockback at max charge
   [SerializeField] private float knockbackDuration = 0.2f; // How long the knockback lasts
  
   // Components
   private LineRenderer lineRenderer;
   private LineRenderer aimLine;
   private bool canFire = true;
   private bool isCharging = false;
   private float currentChargeTime = 0f;
   private Coroutine firingCoroutine;
  
   // References to other components
   private Animator animator; // If you have one
  
   private void Awake()
   {
       if (laserMaterial == null) {
       laserMaterial = new Material(Shader.Find("Particles/Additive"));
       laserMaterial.color = Color.red;
}
      
       // Set up the main laser LineRenderer
      lineRenderer = gameObject.AddComponent<LineRenderer>();
       lineRenderer.startWidth = 0.3f;
       lineRenderer.endWidth = 0.3f;
       lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
       lineRenderer.material.color = Color.red;
       lineRenderer.sortingOrder = 100;
       lineRenderer.enabled = false;
      
       // Set up aiming line
       GameObject aimObj = new GameObject("AimLine");
       aimObj.transform.parent = transform;
       aimLine = aimObj.AddComponent<LineRenderer>();
       aimLine.startWidth = laserWidth * 0.5f;
       aimLine.endWidth = laserWidth * 0.25f;
       aimLine.material = laserMaterial;
      
       // Set up a simpler color for aim line
       Gradient aimGradient = new Gradient();
       GradientColorKey[] colorKeys = new GradientColorKey[2];
       colorKeys[0].color = Color.white;
       colorKeys[0].time = 0f;
       colorKeys[1].color = new Color(1f, 1f, 1f, 0.5f);
       colorKeys[1].time = 1f;
      
       GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
       alphaKeys[0].alpha = 0.5f;
       alphaKeys[0].time = 0f;
       alphaKeys[1].alpha = 0.2f;
       alphaKeys[1].time = 1f;
      
       aimGradient.SetKeys(colorKeys, alphaKeys);
       aimLine.colorGradient = aimGradient;
       aimLine.enabled = false;
      
       // Get animator if it exists
       // animator = GetComponent<Animator>();
   }


   private void Start()
   {
      audioSource = GetComponent<AudioSource>();
   }


   private void Update()
   {
       if (canFire)
       {
           // Start charging
           if (Input.GetKeyDown(KeyCode.Return))
           {
               StartCharging();
           }
          
           // Continue charging
           if (Input.GetKey(KeyCode.Return) && isCharging)
           {
               ContinueCharging();
           }
          
           // Fire laser when key is released
           if (Input.GetKeyUp(KeyCode.Return) && isCharging)
           {
               FireLaser();
           }
       }
   }
  
   private void StartCharging()
   {
      
       isCharging = true;
       currentChargeTime = 0f;
       aimLine.enabled = true;


         // Play charge sound
       if (audioSource != null && chargingSound != null)
           {
           audioSource.clip = chargingSound;
          // Set volume for charge sound
              audioSource.volume = 0.2f;
           audioSource.Play();
           }
      
       // Play charging animation/effect
       if (animator != null)
           // animator.SetTrigger("StartCharge");
          
       if (chargingEffectPrefab != null)
       {
           Instantiate(chargingEffectPrefab, laserOrigin.position, Quaternion.identity, laserOrigin);
       }
   }
  
private void ContinueCharging()
{
   currentChargeTime += Time.deltaTime;
   currentChargeTime = Mathf.Clamp(currentChargeTime, 0, maxChargeTime);
  
   // Calculate current angle based on charge time
   float currentAngle = Mathf.Lerp(minAngle, maxAngle, currentChargeTime / maxChargeTime);
  
   // Get player script to check facing direction
   Player player = GetComponent<Player>();
   bool isFacingRight = player.isFacingRight;
  
   // Flip angle if facing left
   if (!isFacingRight)
   {
       currentAngle = 180 - currentAngle;
   }
  
   // Update aim line
   UpdateAimLine(currentAngle);
}
  
private void FireLaser()
{
   isCharging = false;
   canFire = false;
   aimLine.enabled = false;
  
   // Calculate final angle
   float finalAngle = Mathf.Lerp(minAngle, maxAngle, currentChargeTime / maxChargeTime);
  
   // Get player script to check facing direction
   Player player = GetComponent<Player>();
   bool isFacingRight = player.isFacingRight;
  
   // Flip angle if facing left
   if (!isFacingRight)
   {
       finalAngle = 180 - finalAngle;
   }
  
   // Start the firing coroutine
   if (firingCoroutine != null)
       StopCoroutine(firingCoroutine);
      
   firingCoroutine = StartCoroutine(FireLaserCoroutine(finalAngle));
}
  
   private void UpdateAimLine(float angle)
   {
       
       // Calculate direction based on angle
       Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;


       // Set explicit start and end points with visible distance
       lineRenderer.SetPosition(0, laserOrigin.position);
       lineRenderer.SetPosition(1, laserOrigin.position + (direction * 5f));
      
       // Set aim line positions
       aimLine.SetPosition(0, laserOrigin.position);
       aimLine.SetPosition(1, laserOrigin.position + direction * maxLaserDistance * 0.75f);
      
       // Update color based on charge (optional)
       Color chargeColor = Color.Lerp(Color.green, Color.red, currentChargeTime / maxChargeTime);
      
       Gradient gradient = new Gradient();
       GradientColorKey[] colorKeys = new GradientColorKey[2];
       colorKeys[0].color = chargeColor;
       colorKeys[0].time = 0f;
       colorKeys[1].color = new Color(chargeColor.r, chargeColor.g, chargeColor.b, 0.5f);
       colorKeys[1].time = 1f;
      
       GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
       alphaKeys[0].alpha = 0.5f;
       alphaKeys[0].time = 0f;
       alphaKeys[1].alpha = 0.2f;
       alphaKeys[1].time = 1f;
      
       gradient.SetKeys(colorKeys, alphaKeys);
       aimLine.colorGradient = gradient;
   }
  
private IEnumerator FireLaserCoroutine(float angle)
{
    HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    
    // Play fire sound
    if (audioSource != null && firingSound != null)
    {
        audioSource.Stop();
        audioSource.volume = 0.2f;
        audioSource.PlayOneShot(firingSound);
    }

    // Setup LineRenderer safely
    if (lineRenderer == null)
    {
        lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
    }
    
    // Configure LineRenderer
    lineRenderer.positionCount = 2;
    lineRenderer.startWidth = 0.3f;
    lineRenderer.endWidth = 0.2f;
    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    lineRenderer.material.color = Color.red;
    lineRenderer.sortingOrder = 100;
    lineRenderer.enabled = true;
    
    // Calculate direction
    Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
    
    // Hold the laser at full length
    float duration = 0f;
    while (duration < laserDuration)
    {
        // Always update start position in case player moves
        Vector3 startPos = laserOrigin.position;
        
        // Get all hits
        RaycastHit2D[] allHits = Physics2D.RaycastAll(startPos, direction, maxLaserDistance)
            .Where(hit => !hit.collider.gameObject.CompareTag("Player"))
            .OrderBy(hit => hit.distance)
            .ToArray();
        
        // Default end position (full distance)
        Vector3 endPos = startPos + (direction * maxLaserDistance);
        
        // Process hits
        bool hitProcessed = false;
        foreach (RaycastHit2D hit in allHits)
        {
            // Ensure the object still exists
            if (hit.collider == null || hit.collider.gameObject == null)
                continue;

            // Set endpoint to the hit point
            endPos = hit.point;
            
            // Only process if not already hit
            if (!hitObjects.Contains(hit.collider.gameObject))
            {
                float damage = Mathf.Lerp(baseLaserDamage, maxLaserDamage, currentChargeTime / maxChargeTime);
                float knockbackForce = Mathf.Lerp(baseKnockbackForce, maxKnockbackForce, currentChargeTime / maxChargeTime);
                
                // Comprehensive enemy type checks
                var acidSnail = hit.collider.GetComponent<AcidSnail>();
                if (acidSnail != null)
                {
                    acidSnail.TakeDamage((int)damage);
                    hitObjects.Add(hit.collider.gameObject);
                    ApplyKnockback(hit.collider.gameObject, direction, knockbackForce);
                    hitProcessed = true;
                }
                
                var enemyLeapScript = hit.collider.GetComponent<EnemyLeap>();
                if (enemyLeapScript != null)
                {
                    enemyLeapScript.TakeDamage((int)damage);
                    hitObjects.Add(hit.collider.gameObject);
                    ApplyKnockback(hit.collider.gameObject, direction, knockbackForce);
                    hitProcessed = true;
                }
                
                var snailScript = hit.collider.GetComponent<SnailEnemy>();
                if (snailScript != null)
                {
                    snailScript.TakeDamage((int)damage);
                    hitObjects.Add(hit.collider.gameObject);
                    ApplyKnockback(hit.collider.gameObject, direction, knockbackForce);
                    hitProcessed = true;
                }

                // If we processed a hit, continue to the next iteration
                if (hitProcessed)
                    break;
            }
        }
        
        // Always update line renderer with current start and end positions
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        duration += Time.deltaTime;
        yield return null;
    }
  
    // Ensure the line renderer is disabled at the end
    lineRenderer.enabled = false;
    
    // Wait for cooldown
    yield return new WaitForSeconds(cooldownTime);
    
    // Allow firing again
    canFire = true;
}

// Helper method to list components (already in your original script)

   // New method to apply knockback to enemies
   private void ApplyKnockback(GameObject target, Vector3 direction, float force)
{
         // Try to get rigidbody first
    Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        // For AcidSnail, use a completely different approach
        AcidSnail snail = target.GetComponent<AcidSnail>();
        if (snail != null)
        {
            // Temporarily disable snail's own movement control
            snail.shouldMove = false;
            snail.isMovingAway = false;
            
            // Apply strong direct force regardless of facing
            rb.linearVelocity = Vector2.zero; // Clear any existing velocity
            rb.AddForce(direction.normalized * force * 2f, ForceMode2D.Impulse);
            
            // Debug.Log("Applied direct knockback to AcidSnail: " + (direction.normalized * force * 2f));
        }
        else
        {
            // Regular knockback for other enemies
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }

        // Play knockback sound
        if (audioSource != null && knockbackSound != null)
        {
            audioSource.PlayOneShot(knockbackSound, 0.8f);
        }
        
        // Reset velocity and re-enable snail movement after knockback
        StartCoroutine(ResetVelocityAfterKnockback(rb));
        return;
    }
       
       // If no rigidbody, check if target has a custom knockback method
       IKnockbackable knockbackable = target.GetComponent<IKnockbackable>();
       if (knockbackable != null)
       {
           knockbackable.ApplyKnockback(direction.normalized, force);
       }
   }
   
   // Coroutine to reset velocity after knockback
   private IEnumerator ResetVelocityAfterKnockback(Rigidbody2D rb)
   {
       yield return new WaitForSeconds(knockbackDuration);
       
       // Optional: gradually slow down instead of immediate stop
       float slowdownTime = 0.1f;
       Vector2 initialVelocity = rb.linearVelocity;
       float elapsed = 0;
       
       while (elapsed < slowdownTime)
       {
           rb.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsed / slowdownTime);
           elapsed += Time.deltaTime;
           yield return null;
       }

        // If it's an AcidSnail, make sure its shouldMove property is re-enabled
        AcidSnail snail = rb.GetComponent<AcidSnail>();
            if (snail != null)
            {
                // Re-enable the snail's movement
                 snail.shouldMove = true;
                }
       
       // Ensure velocity is zero at the end
       rb.linearVelocity = Vector2.zero;
   }
  
   private float CalculateDamage()
   {
       // You can make damage based on charge time if you want
       return 10f + (20f * currentChargeTime / maxChargeTime);
   }
  
   // Interface for damageable objects
   public interface IDamageable
   {
       void TakeDamage(float damage);
   }
   
   // New interface for objects that can receive knockback
   public interface IKnockbackable
   {
       void ApplyKnockback(Vector3 direction, float force);
   }


   private string GetGameObjectPath(GameObject obj)
{
   string path = obj.name;
   Transform parent = obj.transform.parent;
   while (parent != null)
   {
       path = parent.name + "/" + path;
       parent = parent.parent;
   }
   return path;
}


private string ListComponents(GameObject obj)
{
   Component[] components = obj.GetComponents<Component>();
   string result = "";
   foreach (Component component in components)
   {
       result += component.GetType().Name + ", ";
   }
   return result;
}

private void DebugRaycast(Vector3 start, Vector3 direction, float distance)
{
    RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);
    // Debug.Log($"Raycast from {start} in direction {direction} hit {hits.Length} objects");
    
    foreach (RaycastHit2D hit in hits)
    {
        // Debug.Log($"Hit: {hit.collider.gameObject.name} at distance {hit.distance}");
    }
    
    // Visual debug
    // Debug.DrawRay(start, direction * distance, Color.red, 2f);
}

}