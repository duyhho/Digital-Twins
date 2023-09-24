using UnityEngine;

public class CollaborativeAgentScript : MonoBehaviour
{
    public Transform mainAgentTransform = null; // Initially null
    public Material inactiveMaterial; // Material to use when the agent is inactive
    public Material activeMaterial; // Material to use when the agent is active
    private bool isAwake = false;
    private Renderer agentRenderer; // Renderer to change the material
    protected HallwaySettings m_HallwaySettings;  // Changed to protected
    void Start()
    {
        m_HallwaySettings = FindObjectOfType<HallwaySettings>();
        agentRenderer = gameObject.GetComponentInChildren<Renderer>();
        agentRenderer.material = inactiveMaterial; // Set the initial material to inactive
    }

    void FixedUpdate()
    {
        if (isAwake && mainAgentTransform != null)
        {
            FollowMainAgent();
        }
    }


    void OnCollisionEnter(Collision col)
    {
        // Check if collided with the main agent to wake up and assign the main agent transform
        if (col.gameObject.CompareTag("agent"))
        {
            isAwake = true;
            mainAgentTransform = col.transform;
            agentRenderer.material = activeMaterial; // Change the material to active upon waking up
        }
    }

    void FollowMainAgent()
    {
        // Define the speed at which the agent should move
        float speed = m_HallwaySettings.agentRunSpeed * 7f;

        // Calculate the direction vector pointing from the collaborative agent towards the main agent
        Vector3 directionToMainAgent = mainAgentTransform.position - transform.position;

        // Get the distance to the main agent
        float distanceToMainAgent = directionToMainAgent.magnitude;

        // Define a minimum distance to maintain from the main agent
        float minimumDistance = 2.0f;

        // If the collaborative agent is closer to the main agent than the minimum distance, 
        // we adjust the target position to maintain the minimum distance
        if (distanceToMainAgent < minimumDistance)
        {
            return; // The collaborative agent does not move if it is within the minimum distance
        }

        // Calculate a target position that maintains a minimum distance from the main agent
        Vector3 targetPosition = mainAgentTransform.position - (directionToMainAgent.normalized * minimumDistance);

        // Move towards the target position at the defined speed
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Find the rotation towards the main agent
        Quaternion lookRotation = Quaternion.LookRotation(directionToMainAgent.normalized);

        // Apply the rotation towards the main agent over time with a defined rotation speed (here it is set to 10.0f, but you can adjust this value)
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10.0f * Time.deltaTime);
    }
    public void Reset()
    {
        isAwake = false;
        agentRenderer.material = inactiveMaterial; // Set the initial material to inactive
    }
}
