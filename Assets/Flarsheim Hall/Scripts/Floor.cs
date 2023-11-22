using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public Material normalMaterial;
    public Material successMaterial;
    public Material failMaterial;
    private Renderer floorRenderer;
    // Start is called before the first frame update
    void Start()
    {
        floorRenderer = GetComponent<Renderer>();
        // Set initial material to normal
        SetMaterial(normalMaterial);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Function to start blinking with the specified material
    public void BlinkMaterial(string materialType, float duration = 0.5f)
    {
        Material blinkMaterial;
        switch (materialType.ToLower())
        {
            case "success":
                blinkMaterial = successMaterial;
                break;
            case "fail":
                blinkMaterial = failMaterial;
                break;
            default:
                Debug.LogWarning("Invalid material type specified. Defaulting to normal material.");
                blinkMaterial = normalMaterial;
                break;
        }
        StartCoroutine(BlinkMaterialCoroutine(blinkMaterial, duration));
    }
    // Coroutine for blinking effect
    private IEnumerator BlinkMaterialCoroutine(Material blinkMaterial, float duration)
    {
        // Change to the blink material
        SetMaterial(blinkMaterial);
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);
        // Revert to the normal material
        SetMaterial(normalMaterial);
    }
    // Helper function to set the material
    private void SetMaterial(Material material)
    {
        floorRenderer.material = material;
    }
}