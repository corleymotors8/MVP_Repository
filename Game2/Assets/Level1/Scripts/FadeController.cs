using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    private CanvasGroup fadePanel;

    private void Start()
    {
    fadePanel = GetComponentInChildren<CanvasGroup>();
    if (fadePanel == null)
    {
        Debug.LogError("CanvasGroup not found on FadePanel! Make sure it has one.");
    }
    }

    private void Update()
    {
        // // Let the player press y key fade to black
        // if (Input.GetKeyDown(KeyCode.Y))
        // {
        //     FadeToBlack(4f);
        // }
      
    }

    public void FadeToBlack(float duration)
    {
        StartCoroutine(FadeInCoroutine(duration));
    }

    private IEnumerator FadeInCoroutine(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            fadePanel.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadePanel.alpha = 1; // Ensure full black at the end
    }

    public IEnumerator FadeIn() 
    {
		fadePanel.alpha = 1;
		while (fadePanel.alpha > 0) 
            {
			fadePanel.alpha -= Time.deltaTime / 10;
			yield return null;
	        }
    }
}