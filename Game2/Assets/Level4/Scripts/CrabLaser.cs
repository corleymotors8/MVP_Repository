using System.Collections;
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
   // Play fire sound
   if (audioSource != null && firingSound != null)
   {
       audioSource.Stop(); // Stop charge sound if playing
       // Set volume for fire sound
         audioSource.volume = 0.2f;
       audioSource.PlayOneShot(firingSound);
   }


   // Enable line renderer
   lineRenderer.enabled = true;
   lineRenderer.startWidth = 0.3f;
   lineRenderer.endWidth = 0.2f;
  
   // Calculate direction and positions
   Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
   Vector3 startPos = laserOrigin.position;
   Vector3 fullEndPos = startPos + (direction * maxLaserDistance);
  
   // Extension animation
   float extendTime = 0.1f; // Time to extend fully
   float timer = 0f;
  
   while (timer < extendTime)
   {
       // Update start position in case crab is moving
       startPos = laserOrigin.position;
      
       // Calculate current extension percentage
       float t = timer / extendTime;
      
       // Set current end position based on percentage
       Vector3 currentEndPos = Vector3.Lerp(startPos, startPos + (direction * maxLaserDistance), t);
      
       // Update line renderer
       lineRenderer.SetPosition(0, startPos);
       lineRenderer.SetPosition(1, currentEndPos);
      
       timer += Time.deltaTime;
       yield return null;
   }
  
   // Hold the laser at full length, but keep updating start position
   float duration = 0f;
   while (duration < laserDuration)
   {
       // Update start position
       startPos = laserOrigin.position;
      
       // Check for all hits along the ray
      // Check for all hits along the ray
RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, maxLaserDistance);
RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxLaserDistance);
Vector3 currentEndPos = hit.collider != null ? hit.point : startPos + (direction * maxLaserDistance);
bool hitSomething = false;

// Calculate current knockback force based on charge time
float knockbackForce = Mathf.Lerp(baseKnockbackForce, maxKnockbackForce, currentChargeTime / maxChargeTime);

// Process all hits
foreach (RaycastHit2D rayHit in hits)
{
    // Skip player hits
    if (rayHit.collider.gameObject.CompareTag("Player"))
    {
        continue;
    }

    // Found a non-player hit
    hitSomething = true;
    currentEndPos = rayHit.point;

    // Calculate damage based on charge
    float damage = Mathf.Lerp(baseLaserDamage, maxLaserDamage, currentChargeTime / maxChargeTime);

    // Try interface first
    IDamageable damageable = rayHit.collider.GetComponent<IDamageable>();
    if (damageable != null)   
    {
        damageable.TakeDamage((int)damage);
        
        // Apply knockback if possible
        ApplyKnockback(rayHit.collider.gameObject, direction, knockbackForce);
        break; // Stop after first hit
    }

    // Then try calling TakeDamage directly for LeapingEnemy
    var enemyScript = rayHit.collider.GetComponent<EnemyLeap>();
    if (enemyScript != null)
    {
        enemyScript.TakeDamage((int)damage);
        
        // Apply knockback
        ApplyKnockback(rayHit.collider.gameObject, direction, knockbackForce);
        break; // Stop after first hit
    }

    // Then for SnailEnemy
    var snailScript = rayHit.collider.GetComponent<SnailEnemy>();
    if (snailScript != null)
    {
        snailScript.TakeDamage((int)damage);
        
        // Apply knockback
        ApplyKnockback(rayHit.collider.gameObject, direction, knockbackForce);
        break; // Stop after first hit
    }

    // For AcidSnail
        var acidSnailScript = rayHit.collider.GetComponent<AcidSnail>();
    if (acidSnailScript != null)
    {

    acidSnailScript.TakeDamage((int)damage);
    Debug.Log("Laser hit AcidSnail, applying damage: " + (int)damage);
    // Debug.Log(" - Health: " + acidSnailScript.currentHealth);
    
        // Apply knockback
        ApplyKnockback(rayHit.collider.gameObject, direction, knockbackForce);
        break; // Stop after first hit
    }
}
      
       // Update line renderer
     lineRenderer.SetPosition(0, startPos);
    lineRenderer.SetPosition(1, currentEndPos);
      
       duration += Time.deltaTime;
       yield return null;
   }
  
   // Disable laser
   lineRenderer.enabled = false;
  
   // Cooldown period
   yield return new WaitForSeconds(cooldownTime);
  
   canFire = true;
}

   // New method to apply knockback to enemies
   private void ApplyKnockback(GameObject target, Vector3 direction, float force)
   {
       // Try to get rigidbody first
       Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
       if (rb != null)
       {
           // Apply impulse force in the laser direction
           rb.linearVelocity = Vector2.zero; // Reset velocity first for consistent knockback
           rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

           // Play knockback sound
              if (audioSource != null && knockbackSound != null)
              {
                audioSource.PlayOneShot(knockbackSound, 0.15f);
              }
           
           // Optionally start a coroutine to reset velocity after knockback duration
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
}