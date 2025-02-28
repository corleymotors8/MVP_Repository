using System.Collections;
using UnityEngine;

public class InsectEnemy : MonoBehaviour
{
    // Movement parameters
    [Header("Movement")]
    public float flySpeed = 3.0f;
    public float flyUpDuration = 2.0f;
    public float flyDownDuration = 2.0f;
    public float idleTime = 3.0f;
    public float takeoffAnimTime = 1.0f;
    public float landAnimTime = 1.0f;

    // Audio parameters
    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip takeoffSound;
    public AudioClip flySound;
    public AudioClip landSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    // Component references
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 startPosition;

    // Animator parameter names
    private const string PARAM_TAKING_OFF = "TakingOff";
    private const string PARAM_FLYING = "Flying";
    private const string PARAM_LANDING = "Landing";

    private void Awake()
    {
        // Get component references
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Initialize audio source if missing
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = soundVolume;
        }
    }

    private void Start()
    {
        // Store starting position
        startPosition = transform.position;
        
        // Start behavior sequence
        StartCoroutine(InsectBehaviorSequence());
    }

    private IEnumerator InsectBehaviorSequence()
    {
        // Idle state (default)
        audioSource.PlayOneShot(idleSound, 0.1f);
        yield return new WaitForSeconds(idleTime);

        // Takeoff
        yield return StartCoroutine(TakeOff());

        // Fly up
        yield return StartCoroutine(FlyUp());

        // Fly down
        yield return StartCoroutine(FlyDown());

        // Land
        yield return StartCoroutine(Land());

        // Optional: Loop the behavior
        // StartCoroutine(InsectBehaviorSequence());
    }

    private IEnumerator TakeOff()
    {
        // Set takeoff animation
        animator.SetBool(PARAM_TAKING_OFF, true);
        
        // Play takeoff sound
        PlaySound(takeoffSound);
        
        // Wait for takeoff animation to play
        yield return new WaitForSeconds(takeoffAnimTime);
        
        // Switch to flying state
        animator.SetBool(PARAM_TAKING_OFF, false);
        animator.SetBool(PARAM_FLYING, true);
        
        // Play flying sound
        PlaySound(flySound, true);
    }

    private IEnumerator FlyUp()
    {
        Vector3 targetPosition = startPosition + Vector3.up * 5f; // Fly up 5 units
        float timer = 0;

        while (timer < flyUpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyUpDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    }

    private IEnumerator FlyDown()
    {
        Vector3 currentPosition = transform.position;
        float timer = 0;

        while (timer < flyDownDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyDownDuration;
            transform.position = Vector3.Lerp(currentPosition, startPosition, t);
            yield return null;
        }
    }

    private IEnumerator Land()
    {
        audioSource.Stop(); // Stop flying sound
        
        // Switch to landing animation
        animator.SetBool(PARAM_FLYING, false);
        animator.SetBool(PARAM_LANDING, true);
        
        // Play landing sound
        PlaySound(landSound);
        
        // Wait for landing animation to play
        yield return new WaitForSeconds(landAnimTime);
        
        // Return to idle state
        animator.SetBool(PARAM_LANDING, false);
        
        // Loop idle sound
        PlaySound(idleSound, true);
       

    }

    private void PlaySound(AudioClip clip, bool loop = false)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }
    }
}