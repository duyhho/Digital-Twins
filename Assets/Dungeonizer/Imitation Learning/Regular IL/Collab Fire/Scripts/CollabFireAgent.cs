using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
public class CollabFireAgent : DungeonAgentFire
{
    public FireLifeScript fireLifeScript;
    float parentOffsetHeight;
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log("Collecting Observations");
        if (sensor == null)
        {
            Debug.LogError("Sensor is null.");
            return;
        }

        RaycastUpdateGrid(); //thiDynamicHallwayFireCollab doesn't collect any observations ,just updating grid cell status;
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
            //The key takeaway is that InverseTransformDirection uses the object's (in this case, the car's) current orientation to convert a direction from world space to that object's local space. 
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        }
    }
    protected override void RaycastUpdateGrid()
    {
        base.RaycastUpdateGrid();
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (MaxStep != 0)
        {
            AddReward(-1f / MaxStep);
        }

        MoveAgent(actionBuffers.DiscreteActions);

        // AddReward(CalculateRewardForDoor());


    }

    protected override void OnCollisionEnter(Collision col)
    {
        base.OnCollisionEnter(col);
    }
    protected override void OnCollisionStay(Collision col)
    {
        base.OnCollisionStay(col);
    }
    protected override IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.001f); // Wait for 1 second
        // if (!isEvaluation) //in training mode
        // {
        //     ResetEnvironment();
        // }

        // EndEpisode();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("fire"))
        {
            int agentCount = fireLifeScript.agentCount;
            if (agentCount > 1)
            {
                SetReward(2f * agentCount);
            }
            else
            {
                SetReward(2f);
            }
            // StartCoroutine(DelayedEndEpisode());
            if (isEvaluation)
            {
                UpdateModelStats();
                ResetEnvironment();
            }
            EndEpisode();
            // if (fireLifeScript != null)
            // {
            //     if (fireLifeScript.fireLife <= 0)
            //     {
            //         // StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            //     }

            // }
        }

    }

    protected override void UpdateModelStats()
    {
        // base.UpdateModelStats();
        if (modelIndex >= 0)
        {
            ModelStats currentStats = modelStatsList[modelIndex];
            float timeToReachFire = Time.time - episodeStartTime;
            currentStats.cumulativeTimeToReachFire += timeToReachFire;

            currentStats.successfulAttempts += 1; // Increment successful attempts
            float averageTime = currentStats.cumulativeTimeToReachFire / currentStats.successfulAttempts;
            currentStats.averageTime = averageTime;

            // Include the model set in your log messages
            // Debug.Log("Model Set: " + modelIndex);
            Debug.Log("Time to reach the fire: " + timeToReachFire + " seconds");
            Debug.Log("Average time to reach the fire successfully (Model Set " + modelIndex + "): " + averageTime + " seconds");
            Debug.Log("Success rate (Model Set " + modelIndex + "): " + ((float)currentStats.successfulAttempts / currentStats.attemptCount) * 100 + "% (" + currentStats.successfulAttempts + "-" + currentStats.attemptCount + ")");
        }
        else
        {
            Debug.Log("Reached Fire without water! Failed!");
        }

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }
    protected override void OnLayoutChange()
    {
        base.OnLayoutChange();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        ResetEnvironment();
        // parentOffsetHeight = modernRoomGenerator.parentOffsetHeight;
        // if (modernRoomGenerator.parentOffsetHeight <= -9000f)
        // {
        //     parentOffsetHeight = area.transform.position.y;
        // }
        parentOffsetHeight = area.transform.position.y;

        Vector3 newStartPosition = roomManager.startPoint + new Vector3(0f, parentOffsetHeight, 0f);
        newStartPosition.y = parentOffsetHeight;
        Vector3 randomFirePosition = roomManager.GetRandomGoalPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
        randomFirePosition.y = parentOffsetHeight;

        if (symbolOGoal)
        {
            fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
            // fireLifeScript.Reset();

            symbolOGoal.transform.position = randomFirePosition;
            // Debug.Log("randomFirePosition" + randomFirePosition);
        }
        transform.position = newStartPosition;
        transform.rotation = Quaternion.identity;

        // Debug.Log("newStartPosition" + newStartPosition);

        // Reset all doors in the scene

    }
    protected override void CheckCurrentEvaluationModels()
    {
        base.CheckCurrentEvaluationModels();
    }
    public override void ResetEnvironment()
    {
        // base.ResetEnvironment();
        // modernRoomGenerator.ClearOldDungeon();
        // modernRoomGenerator.Generate();
        // gridManager.ResetGrid();
        if (symbolOGoal)
        {
            fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
            fireLifeScript.Reset();
            // symbolOGoal.transform.position = randomFirePosition;
        }
        CollaborativeAgentScript[] allCollabAgents = area.GetComponentsInChildren<CollaborativeAgentScript>();
        foreach (CollaborativeAgentScript cAgent in allCollabAgents)
        {
            Vector3 randomPosition = roomManager.GetRandomObjectPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
            // Debug.Log("randomPosition: " + randomPosition);
            cAgent.gameObject.transform.position = randomPosition;
            cAgent.Reset();
        }
    }
    public override void PlayWaterAndStopFire()
    {
        base.PlayWaterAndStopFire();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    /// <summary>
    /// Configures the agent's neural network model based on the specified room configuration.
    /// </summary>
    /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    // protected override void ConfigureAgent(int config)
    // {
    //     base.ConfigureAgent(config);
    // }
    protected override void ConfigureAgent(int config)
    {
        if (!isEvaluation) //keep this for training logic
        {
            // Debug.Log("Config: " + config);
            // if (twoRoomBrain == null || threeRoomBrain == null || dynamicRoomBrain == null)
            // {
            //     Debug.LogError("CUSTOM ERROR: One or more brain models are null. Please assign brain models in the inspector.");
            //     return;
            // }
            // if (config == 2)
            // {
            //     SetModel("TwoRoom", twoRoomBrain);
            // }
            // else if (config == 3)
            // {

            //     SetModel("ThreeRoom", threeRoomBrain);
            // }
            // else if (config == 4)
            // {
            //     SetModel("FourRoom", fourRoomBrain);
            // }
            // else if (config == 5)
            // {
            //     SetModel("FiveRoom", fiveRoomBrain);
            // }
            // else
            // {
            //     SetModel("FiveRoom", dynamicRoomBrain);
            // }
        }
        else //use evaluationModelSets here
        {
            Debug.Log("Config: " + config);

            if (modelIndex >= 0)
            {
                if (evaluationModelSets.Count == 0 || evaluationModelSets[modelIndex].Models.Count == 0)
                {
                    Debug.LogError("CUSTOM ERROR: The evaluation model set is empty. Please assign model sets in the inspector.");
                    return;
                }
                // Debug.Log("modelIndex: " + modelIndex);
                // Fetch the appropriate NNModelSet based on the current modelIndex
                NNModelSet currentModelSet = evaluationModelSets[modelIndex];
                // Fetch the appropriate NNModel based on the config value
                string behaviorName = config switch
                {
                    2 => "RoomTwo",
                    3 => "RoomThree",
                    4 => "RoomFour",
                    5 => "RoomFive",
                    _ => "RoomFive",
                };

                int modelIndexInSet = config - 2;
                if (modelIndexInSet >= 0 && modelIndexInSet < currentModelSet.Models.Count)
                {
                    SetModel(behaviorName, currentModelSet.Models[modelIndexInSet]);
                }
                else
                {
                    SetModel(behaviorName, currentModelSet.Models[currentModelSet.Models.Count - 1]);
                }
            }


        }
    }
}
