

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Level2Fader : MonoBehaviour
{
private CanvasGroup fadePanel;


   private void Awake()
   {  fadePanel = GetComponent<CanvasGroup>();
    if (fadePanel == null)
    {
        Debug.LogError($"CanvasGroup not found on {gameObject.name}! Make sure it has one.");
    }
   }
   

   private void Update()
   {

   }

   // Fade in at start of level
   public IEnumerator FadeIn()
  {
    if (fadePanel == null)
    {
        Debug.LogError("Attempting to fade with null CanvasGroup!");
        yield break;
    }

    fadePanel.alpha = 1;
    float duration = 14f;
    float elapsedTime = 0;
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        
        // Custom curve that spends more time between 1.0 and 0.98
        if (t < 0.2f)  // First 20% of the time
        {
            // Slow fade from 1.0 to 0.99
            fadePanel.alpha = Mathf.Lerp(1f, 0.99f, t / 0.2f);
        }
        else
        {
            // Faster fade from 0.98 to 0
            float remainingT = (t - 0.2f) / 0.8f;
            fadePanel.alpha = Mathf.Lerp(0.98f, 0f, remainingT);
        }
        
        yield return null;
    }
    
    fadePanel.alpha = 0;
}

// Reverse fade in to restore the black panel
public IEnumerator FadeOut()
{
    Debug.Log("Level2Fader begun. Fading out");
    if (fadePanel == null)
    {
        Debug.LogError("Attempting to fade with null CanvasGroup!");
        yield break;
    }

    fadePanel.alpha = 0;
    float duration = 4f;
    float elapsedTime = 0;

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        fadePanel.alpha = Mathf.Lerp(0f, 1f, t);

        yield return null;
    }

    fadePanel.alpha = 1;

}

}


