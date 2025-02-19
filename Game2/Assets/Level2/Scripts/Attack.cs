//Just used for level2 right now

using Unity.VisualScripting;
using UnityEngine;

public class Attack : MonoBehaviour
{
    //Declare player
    private Player player;
    AudioSource audioSource;
    public AudioClip squishSnail; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get audioSource
        audioSource = GetComponent<AudioSource>();
        
        //Find player
        player = FindFirstObjectByType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Attack.cs: OnTriggerEnter2D");
        if (player.isAttacking && collision.gameObject.tag == "Enemy")
        {
            //Just for level2
            Debug.Log("Enemy hit");
            Level2Manager.Instance.playerKilledEnemy = true;
           
        }
    }
}
