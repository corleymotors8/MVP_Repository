// using UnityEngine;

// public class HoopTrigger : MonoBehaviour
// {
//     HoopController hoopController;
//     BallController ballController;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         hoopController = FindFirstObjectByType<HoopController>();
//         ballController = FindFirstObjectByType<BallController>();
//         Debug.Log("Found ball controller: " + (ballController != null));
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }

//     //print debug when triggered2d
//    private void OnTriggerEnter2D(Collider2D other)
// {
//     if (ballController == null)
// {
//     Debug.LogError("BallController not found!");
// }
// else
// {
//     ballController.DestroyBall();
//     Debug.Log("Destroying ball");
// }
//     if (ballController != null) {
//         ballController.DestroyBall();
//     }
// }

// }
