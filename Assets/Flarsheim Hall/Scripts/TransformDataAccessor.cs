using UnityEngine;

public class TransformDataAccessor : MonoBehaviour
{
    private Transform baseFootprintTransform;

    void Update()
    {
        if (baseFootprintTransform == null)
        {
            GameObject baseFootprint = GameObject.Find("odom/base_footprint");
            if (baseFootprint != null)
            {
                baseFootprintTransform = baseFootprint.transform;
            }
        }
        else
        {
            // Use baseFootprintTransform data for other GameObjects or calculations
            // For example:
            Debug.Log(baseFootprintTransform.position);
        }
    }
}
