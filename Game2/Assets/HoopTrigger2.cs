using UnityEngine;

public class HoopTrigger2 : MonoBehaviour
{

    HoopController hoopController;
    public bool PlayerHoop = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hoopController = FindFirstObjectByType<HoopController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //print debug when triggered2d
private void OnTriggerEnter2D(Collider2D other)
{
    if (PlayerHoop)
    {
        hoopController.ScorePointPlayer();
    }
    else
    {
       Debug.Log("Enemy Scored!");
        hoopController.ScorePointEnemy();
    }
    
    // Get BallController from THIS ball, not a stored reference
    BallController ballController = other.GetComponent<BallController>();
    if (ballController != null)
    {
        ballController.DestroyBall();
    }
    Debug.Log("Destroying ball");
}

}
