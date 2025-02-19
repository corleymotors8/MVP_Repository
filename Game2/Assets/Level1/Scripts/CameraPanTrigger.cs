using UnityEngine;
using System.Collections;

public class CameraTriggerController : MonoBehaviour
{
public Vector3 targetCameraPosition; // New position to pan to
public float panDuration = 3f;       // Duration of the pan

private GameManager gameManager; // Used to set respawn point
private float startSize;
public float targetSize; // New size of the camera
public bool hasTriggered = false;
private bool isPanning = false;
private static Vector3 lastCameraPosition;
private static float lastCameraSize;

    private void Start()
    {
        //Access game Manager
        gameManager = FindFirstObjectByType<GameManager>();
        startSize = Camera.main.orthographicSize;
    }

    private void OnTriggerEnter2D(Collider2D other) // or OnTriggerEnter for 3D
    {
        if (other.CompareTag("Player")) // Ensure player has the correct tag
        {
            StartCoroutine(PanCamera());
        }
    }

private IEnumerator PanCamera()
{
    if (!hasTriggered && !isPanning)
    {
        isPanning = true; // Prevent overlapping pan animations

        // Use last known position or current position if it's the first time
        Vector3 startPosition = lastCameraPosition != Vector3.zero ? lastCameraPosition : Camera.main.transform.position;
        float startSize = lastCameraSize > 0 ? lastCameraSize : Camera.main.orthographicSize;

        float elapsedTime = 0;

        while (elapsedTime < panDuration)
        {
            float t = elapsedTime / panDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth transition

            // Smoothly adjust camera size and position
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetCameraPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position & size is exactly set
        Camera.main.orthographicSize = targetSize;
        Camera.main.transform.position = targetCameraPosition;

        // Update last position and size
        lastCameraPosition = targetCameraPosition;
        lastCameraSize = targetSize;

        hasTriggered = true;
        isPanning = false; // Reset for the next trigger
    }
}

}