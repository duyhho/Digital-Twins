using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using System;
using UnityEngine.Networking;

public class JetBotBasicNavigation : JetBotAgent
{
    public List<Transform> jetbotSpawnLocations;
    public GameObject currentGoalInstance;
    public List<Transform> goalSpawnLocations;
    public GameObject floor;
    // Override the MoveAgent method if specific movement logic is needed
    public override void Initialize()
    {
        base.Initialize(); //initialize the jetbot rb;
    }
    public override void MoveAgent(ActionSegment<int> act)
    {
        // Implement specific basic navigation logic here
        base.MoveAgent(act);

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log("Collecting Observations");
        if (sensor == null)
        {
            Debug.LogError("Sensor is null.");
            return;
        }
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
            //The key takeaway is that InverseTransformDirection uses the object's (in this case, the car's) current orientation to convert a direction from world space to that object's local space. 
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (MaxStep != 0)
        {
            AddReward(-1f / MaxStep);
            // Check for time-based fail condition
            if (StepCount >= MaxStep)
            {
                SetReward(-1f); // Negative reward for taking too long
                EndEpisode(); // End the episode
                floor.GetComponent<Floor>().BlinkMaterial("fail"); // Blink floor with fail material
            }
        }

        MoveAgent(actionBuffers.DiscreteActions);
    }
    protected void OnCollisionEnter(Collision col)
    {

    }
    protected void OnCollisionStay(Collision col)
    {

    }
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("goal"))
        {
            SetReward(2f);
            EndEpisode();
            //Blink floor 
            floor.GetComponent<Floor>().BlinkMaterial("success");
        }

    }
    public override void OnEpisodeBegin()
    {
        lastAction = -1;
        m_AgentRb.velocity *= 0f;
        // Ensure there are available spawn locations
        if (jetbotSpawnLocations != null && jetbotSpawnLocations.Count > 0)
        {
            // Choose a random index from the list
            int randomIndex = UnityEngine.Random.Range(0, jetbotSpawnLocations.Count);
            // randomIndex = 0;
            // Get the random location's position and rotation
            Transform randomLocation = jetbotSpawnLocations[randomIndex];
            Vector3 spawnPosition = randomLocation.position;
            Quaternion spawnRotation = randomLocation.rotation;

            // Set the JetBot's position and rotation
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
        }
        else
        {
            Debug.LogWarning("JetBot spawn locations list is empty or null!");
        }

        // Ensure there are available goal spawn locations
        if (goalSpawnLocations != null && goalSpawnLocations.Count > 0)
        {
            // Choose a random index from the list
            int randomIndex = UnityEngine.Random.Range(0, goalSpawnLocations.Count);

            // Get the random location's position and rotation
            Transform randomLocation = goalSpawnLocations[randomIndex];
            Vector3 spawnPosition = randomLocation.position;
            Quaternion spawnRotation = randomLocation.rotation;
            // Move the existing goal object to the new location
            currentGoalInstance.transform.position = spawnPosition;
            currentGoalInstance.transform.rotation = spawnRotation;

        }
        else
        {
            Debug.LogWarning("Goal spawn locations list is empty or null!");
        }

    }
}
