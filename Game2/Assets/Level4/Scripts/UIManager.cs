using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    // References to the UI elements
    [SerializeField] private List<GameObject> healthIcons = new List<GameObject>();
    [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI playerScoreText; 
    [SerializeField] private TextMeshProUGUI enemyScoreText; 

    
    // References to player and game manager
    private Player player;
    private GameManager gameManager;
    HoopController hoopController;
    
    // Track previous values to avoid unnecessary updates
    private int lastHealthValue = -1;
    private int lastShieldValue = -1;
     private int lastScoreValue = -1; 
     private int lastEnemyScoreValue = -1; 
    
    void Start()
    {
        // Find the player and game manager references
        player = FindFirstObjectByType<Player>();
        gameManager = FindFirstObjectByType<GameManager>();
        hoopController = FindFirstObjectByType<HoopController>();

        
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

         // Check if score has changed
        if (hoopController != null && hoopController.pointsScored != lastScoreValue)
        {
            UpdateScoreDisplay();
        }
        if (hoopController != null && hoopController.enemyPoints != lastEnemyScoreValue)
        {
            UpdateScoreDisplay();
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

     void UpdateScoreDisplay()
    {
        if (hoopController != null && playerScoreText != null)
        {
            lastScoreValue = hoopController.pointsScored;
            lastEnemyScoreValue = hoopController.enemyPoints; 
            playerScoreText.text = "Player score: " + lastScoreValue;
            enemyScoreText.text = "Enemy score: " + lastEnemyScoreValue;
        }
    }
}