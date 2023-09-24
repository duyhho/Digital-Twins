using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLifeScript : MonoBehaviour
{
    public float maxFireLife = 3f;
    public float fireLife = 3f;
    public int agentCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("agent") || other.CompareTag("collab_agent"))
        {
            agentCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("agent") || other.CompareTag("collab_agent"))
        {
            agentCount--;
        }
    }

    private void Update()
    {
        if (agentCount > 0 && fireLife > 0)
        {
            fireLife -= agentCount * Time.deltaTime;
        }

        if (fireLife <= 0)
        {
            fireLife = 0f;
            // Fire is extinguished, end the episode
            // Note: Implement your episode ending logic here
            // Debug.Log("Fire extinguished");
        }
    }
    public void Reset()
    {
        fireLife = maxFireLife;
    }
}
