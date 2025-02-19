using UnityEngine;
using System.Collections;

public class CameraZoomController : MonoBehaviour
{
    public float targetSize = 8f;
    public float zoomSpeed = 2f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(SmoothZoom(targetSize));
        }
    }

    private IEnumerator SmoothZoom(float newSize)
    {
        float startSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < zoomSpeed)
        {
            elapsedTime += Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, newSize, elapsedTime / zoomSpeed);
            yield return null;
        }

        mainCamera.orthographicSize = newSize;
    }
}
