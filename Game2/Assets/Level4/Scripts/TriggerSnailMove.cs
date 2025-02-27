using UnityEngine;

public class TriggerSnailMove : MonoBehaviour
{
    //Access parent game object script "AcidSnail"
    AcidSnail acidSnail;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        acidSnail = GetComponentInParent<AcidSnail>();
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            acidSnail.shouldMove = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
