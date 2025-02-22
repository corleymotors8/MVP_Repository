using System.Collections;
using UnityEngine;

public class Crusher : MonoBehaviour
{
    // Public variables to adjust timing and movement in the inspector
    AudioSource audioSource;
    public AudioClip landSound;
    public AudioClip riseSound;
    public AudioClip fallSound;

    public float landVolume = 0.1f;
    public float fallSpeed = 4f;  // How fast the block falls
    public float riseSpeed = 2f; // How fast the block rises
    public float waitTimeGround = 2f;  // How long the block waits on the ground
    public float waitTimeAir = 1f;  // How long the block waits in the air


    // Public variables to track landing position
    public Transform landingPlatform;  // Assign the platform in the Inspector
    private float fallingPosition;      // The vertical position where the block stops falling

    // Private variables to track state
    private Vector3 initialPosition;  // Starting position of the block
    public bool isFalling = false;    // Whether the block is currently falling
    public bool isRising = false;    // Whether the block is currently rising
    private bool isPlayingRisingSound = false;
    private bool hasTriggered = false;
    private bool isPlayingFallSound = false;
    //Add Header
    [Header("For NPC character")]
    public bool isTalking = false;

    // Find player class and trigger player died
    private Player player;
    
    public GameObject bloodPrefab;   // Blood prefab

    private Rigidbody2D rb;

    void Start()
    {
        // Find player class
        player = FindFirstObjectByType<Player>();

        //
        isFalling = false;
        // Debug.Log("Is falling? " + isFalling);
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
       ;

         // Ensure a platform is assigned
        if (landingPlatform != null)
    {
        Collider2D platformCollider = landingPlatform.GetComponent<Collider2D>();
        if (platformCollider != null)
        {
            // Set the falling position to the top of the platform
            fallingPosition = platformCollider.bounds.max.y;
        }
    }
    }
    void Update()
{
    if (isFalling)
    {
        // Play fall sound
        if (!isPlayingFallSound)
        {
       isPlayingFallSound = true;
       audioSource.PlayOneShot(fallSound, 0.1f);
        }
        
        // Move the block downward 
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Stop falling when it reaches the falling position
        if (GetComponent<Collider2D>().bounds.min.y <= fallingPosition)
        {
            isFalling = false;
            audioSource.PlayOneShot(landSound, landVolume);
            StartCoroutine(WaitAndRise());
            isPlayingFallSound = false;
        }
      
    }

    if (isRising)
    {
        if (!isPlayingRisingSound)
        // Play rise sound
        audioSource.PlayOneShot(riseSound, 0.1f);
        isPlayingRisingSound = true;

        
        // Move the block upward
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Non-Talking: Stop rising when the block reaches just below its initial position
        if (!isTalking && transform.position.y >= initialPosition.y - 0.5f)
        {
             StartCoroutine(WaitAndFall());
             isRising = false;
             //Stop rise sound
            audioSource.Stop();
            isPlayingRisingSound = false;
        }

        // Talking: Stop when at 15 on y axis
        if (isTalking && transform.position.y >= 15f)
        {
            isRising = false;
            //Stop rise sound
            audioSource.Stop();
            isPlayingRisingSound = false;
        }
    }
}

 IEnumerator WaitAndFall()
    {
        // Debug.Log("Waiting to fall");
        yield return new WaitForSeconds(waitTimeAir); // Wait in the air
        isFalling = true;
    }
 
 IEnumerator WaitAndRise()
    {
        yield return new WaitForSeconds(waitTimeGround); // Wait on the ground
        isRising = true;
    }

    void OnTriggerEnter2D(Collider2D collision) // Falling starts when player triggers collider
    {
        // Debug.Log("Triggering is falling");
        if (collision.CompareTag("Player") && !hasTriggered && !isTalking)
        {
            isFalling = true;
            hasTriggered = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling && collision.gameObject.CompareTag("Player") && !player.isFalling)
        {
            if (collision.contacts[0].normal.y > 0.5f)
        {
            player.PlayerHitByEnemy();
        }

        }
        if (isFalling && collision.gameObject.CompareTag("Enemy"))
        {
            ICrushable enemy = collision.gameObject.GetComponent<ICrushable>();
            if (enemy != null)
            {
                enemy.Crushed();
            }
        }
    }
}