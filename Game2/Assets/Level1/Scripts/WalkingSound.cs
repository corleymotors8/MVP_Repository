/////// PROBBALY WON'T USE --- JUST USE KEYFRAMES IN ANIMATION ////////


// using UnityEngine;

// public class PlayerSounds : MonoBehaviour
// {
//     // Get Player script
//     private Player player;
//     AudioSource audioSource;
   
//     // Walking sound clip
//     public AudioClip walkingClip;
//     public AudioClip climbingClip;
//     private bool isWalkingPlaying = false;
//     private bool isClimbingPlaying = false;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         player = FindFirstObjectByType<Player>();
//         audioSource = GetComponent<AudioSource>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // Sound for walking
//         if (player.isWalking)
//         {
//             if (!isWalkingPlaying)
//             {
//             Debug.Log("Playing walking sound");
//             audioSource.PlayOneShot(walkingClip);  
//             isWalkingPlaying = true; 
//             }
//         }
//         else
//         {
//             if (isWalkingPlaying)
//            {
//             Debug.Log("Stopping walking sound");
//             audioSource.Stop();
//             isWalkingPlaying = false;
//            }
//         }

//         // Sound for climbing
//         if (player.isClimbing)
//         {
//             if (!isWalkingPlaying)
//             {
//             Debug.Log("Playing climbing sound");
//             audioSource.PlayOneShot(climbingClip);
//             isClimbingPlaying = true;
//             }
//         }
//         else
//         {
//             if (isWalkingPlaying)
//            {
//             Debug.Log("Stopping climbing sound");
//             audioSource.Stop();
//             isClimbingPlaying = false;
//            }
//         }


        
//     }
// }
