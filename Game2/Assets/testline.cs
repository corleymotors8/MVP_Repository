using UnityEngine;

public class TestLineRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Material lineMaterial;
    
    void Start()
    {
        // Create a line renderer if it doesn't exist
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Set up the line renderer with very visible settings
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        
        // Try different material settings
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // Create a default material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = Color.red;
        }
        
        // Force high sorting order
        lineRenderer.sortingOrder = 100;
        
        // Set positions for a visible line
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + new Vector3(5f, 0f, 0f));
        
        // Debug output
        Debug.Log("TestLineRenderer created with positions: " + 
                 transform.position + " to " + 
                 (transform.position + new Vector3(5f, 0f, 0f)));
    }

    // Toggle the line on keypress
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            lineRenderer.enabled = !lineRenderer.enabled;
            Debug.Log("Line renderer toggled: " + lineRenderer.enabled);
        }
        
        // Press T to test with different settings
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestDifferentSettings();
        }
    }
    
    void TestDifferentSettings()
    {
        // Try alternate renderer settings
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        
        // Try a different shader
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.material.color = Color.green;
        
        // Try different positions
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + new Vector3(0f, 5f, 0f));
        
        Debug.Log("Testing alternate settings");
    }
}