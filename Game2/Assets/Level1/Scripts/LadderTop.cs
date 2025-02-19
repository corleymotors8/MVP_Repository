//// ** PRESERVING BUT NOT USING ** JUST HAVE GAP IN PLATFORM ///

// using UnityEngine;

// public class LadderTopTrigger : MonoBehaviour
// {
//     private Platform platform;
//     private Transform player;

//     void Start()
//     {
//         platform = GetComponentInParent<Platform>();  // Find the parent Platform script
//     }

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             player = other.transform; // Store player's transform
//             platform.platformCollider.enabled = false;  // Disable platform collision
//             Debug.Log("Platform disabled: Player is passing through.");
//         }
//     }

//     void Update()
//     {
//         if (player != null)
//         {
//             // Re-enable platform when player moves above OR below the platform
//             if (player.position.y > platform.transform.position.y || player.position.y < platform.transform.position.y - 0.5f)
//             {
//                 platform.platformCollider.enabled = true;
//                 Debug.Log("Platform re-enabled.");
//                 player = null; // Stop checking after re-enabling
//             }
//         }
//     }
// }