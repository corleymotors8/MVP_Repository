using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // References to the UI elements
    [SerializeField] private List<GameObject> healthIcons = new List<GameObject>();
    [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();
    
    // References to player and game manager
    private Player player;
    private GameManager gameManager;
    
    // Track previous values to avoid unnecessary updates
    private int lastHealthValue = -1;
    private int lastShieldValue = -1;
    
    void Start()
    {
        // Find the player and game manager references
        player = FindFirstObjectByType<Player>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (player == null)
        {
            Debug.LogError("UIManager: Player reference not found!");
        }
        
        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager reference not found!");
        }
        
        // Initialize UI
        UpdateHealthDisplay();
        UpdateShieldDisplay();
    }
    
    void Update()
    {
        // Only update when values change for better performance
        if (player != null)
        {
            if (player.currentHealth != lastHealthValue)
            {
                UpdateHealthDisplay();
            }
            
            if (player.currentShieldHealth != lastShieldValue)
            {
                UpdateShieldDisplay();
            }
        }
    }
    
    void UpdateHealthDisplay()
    {
        if (player != null)
        {
            lastHealthValue = player.currentHealth;
            
            // Update each heart icon
            for (int i = 0; i < healthIcons.Count; i++)
            {
                // Show heart if player has this health point, hide otherwise
                healthIcons[i].SetActive(i < player.currentHealth);
            }
        }
    }
    
   void UpdateShieldDisplay()
    {
        if (player != null)
        {
            int shieldValue = player.shieldActive ? player.currentShieldHealth : 0;
            lastShieldValue = shieldValue;
            
            // Update each shield icon
            for (int i = 0; i < shieldIcons.Count; i++)
            {
                // Show shield if player has this shield point, hide otherwise
                shieldIcons[i].SetActive(i < shieldValue);
            }
        }
    }
}