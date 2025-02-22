using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;          // Speed of platform movement
    public float moveLeftDistance = 5f;   // Distance to move left
    public float moveRightDistance = 5f;  // Distance to move right
    public bool startRight = true;     // Whether the platform starts moving right
    
    AudioSource audioSource;
    public AudioClip landSound;

    private Vector3 startPosition;
    private bool movingRight;
    public bool rideable = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //add component audiosource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        startPosition = transform.position;
        movingRight = startRight;
    }

    void Update()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;

            if (transform.position.x >= startPosition.x + moveRightDistance)
            {
                movingRight = false;
            }
        }
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;

            if (transform.position.x <= startPosition.x - moveLeftDistance)
            {
                movingRight = true;
            }
        }
    }

  private void OnTriggerEnter2D(Collider2D other)
{
    if (rideable && other.CompareTag("Player"))
    {
        StartCoroutine(SetParentAfterFrame(other.transform));
        audioSource.PlayOneShot(landSound, 0.2f);
    }
    if (!rideable && other.CompareTag("Player"))
    {
        speed = 2;
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (rideable && other.CompareTag("Player"))
    {
        StartCoroutine(ClearParentAfterFrame(other.transform));
    }
}

private void OnDisable()
{
    StopAllCoroutines(); // Stops any coroutines before the GameObject is deactivated
}


private IEnumerator SetParentAfterFrame(Transform player)
{
    yield return null; // Wait one frame
    player.SetParent(transform);
}

private IEnumerator ClearParentAfterFrame(Transform player)
{
    yield return null; // Wait one frame
    player.SetParent(null);
}


}