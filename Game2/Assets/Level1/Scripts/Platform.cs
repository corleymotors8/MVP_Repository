// Mainly used right now for handling ladder climbing and platform collision

using UnityEngine;

public class Platform : MonoBehaviour
{

[HideInInspector]
public Collider2D platformCollider;

[HideInInspector]
public Player player;
public bool atLadderTop = false;  // Flag to track if the player is at the ladder top
public bool collisionEnabled = true;


void Start() 
{
player = FindFirstObjectByType<Player>();
platformCollider = GetComponent<Collider2D>();
}

void Update() 
{

}

}


