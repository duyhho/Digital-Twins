using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using System;
public class CollabFireAgent : DungeonAgentFire
{
    public FireLifeScript fireLifeScript;
    float parentOffsetHeight;
    public int agentCount = 1;
    public bool shouldRandomize = false;
    public override void Initialize()
    {
        base.Initialize();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
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
            sensor.AddObservation(agentCount);
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
        // if (col.gameObject.CompareTag("collab_agent"))
        // {
        //     CollaborativeAgentScript collabComponent = col.gameObject.GetComponent<CollaborativeAgentScript>();
        //     if (collabComponent.isAwake == false)
        //     {
        //         agentCount++;
        //     }
        // }
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
            Debug.Log("Agent Count: " + agentCount);

            SetReward(2f * agentCount);

            // StartCoroutine(DelayedEndEpisode());
            if (isEvaluation)
            {
                UpdateModelStats();
            }
            else
            {
                shouldRandomize = true;
            }
            // ResetEnvironment();
            EndEpisode();
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
        // base.OnEpisodeBegin();
        if (isEvaluation)
        {
            CheckCurrentEvaluationModels();
        }
        episodeStartTime = Time.time;
        m_Configuration = modernRoomGenerator.maximumRoomCount;
        ResetEnvironment();
    }
    protected override void CheckCurrentEvaluationModels()
    {
        // base.CheckCurrentEvaluationModels();
        if (isEvaluation)
        {
            // If all models have been tested in the current layout
            if (modelIndex >= totalModelSets - 1)
            {
                modelIndex = -1;  // Reset the index to -1
                shouldRandomize = true;
                // ResetEnvironment(); // Generate a new environment layout here
            }
            modelIndex += 1;  // Increment modelIndex at the start

            // Ensure modelIndex is in bounds before using it to index into modelStatsList
            if (modelIndex < totalModelSets)
            {
                modelStatsList[modelIndex].attemptCount += 1;
            }

            Debug.Log("ON EPISODE END! Current Model Set: " + modelIndex);


        }

        if (isEvaluation)
        {
            bool allAttemptsExceeded = true;
            foreach (var modelStats in modelStatsList)
            {
                if (modelStats.attemptCount < maxAttempts)
                {
                    allAttemptsExceeded = false;
                    break;
                }
            }

            if (allAttemptsExceeded)
            {
                // Print a summary of all metrics
                for (int i = 0; i < modelStatsList.Count; i++)
                {
                    var modelStats = modelStatsList[i];
                    float averageTime = modelStats.cumulativeTimeToReachFire / modelStats.successfulAttempts;
                    float successRate = ((float)modelStats.successfulAttempts / modelStats.attemptCount) * 100;

                    Debug.Log("Model Set " + i + " Summary:");
                    Debug.Log(" - Average Time: " + averageTime + " seconds");
                    Debug.Log(" - Success Rate: " + successRate + "% (" + modelStats.successfulAttempts + "/" + modelStats.attemptCount + ")");

                }
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

        }

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
        if (!isEvaluation)
        {
            if (shouldRandomize)
            {
                float curriculumRoomCount = m_ResetParams.GetWithDefault("room_count", -1);
                int roundedCurriculumRoomCount = (int)Math.Round(curriculumRoomCount);

                Debug.Log("curriculumRoomCount: " + curriculumRoomCount);
                if (curriculumRoomCount != -1)
                {
                    if (modernRoomGenerator.maximumRoomCount != curriculumRoomCount)
                    {
                        modernRoomGenerator.maximumRoomCount = (int)roundedCurriculumRoomCount;
                        modernRoomGenerator.ClearOldDungeon();
                        modernRoomGenerator.Generate();
                    }
                }

                float curriculumShouldRandomize = m_ResetParams.GetWithDefault("should_randomize", -1);
                Debug.Log("shouldRandomize: " + shouldRandomize);
                if (shouldRandomize == true && curriculumShouldRandomize != -1)
                {
                    shouldRandomize = curriculumShouldRandomize == 1;
                }

                Debug.Log($"Current Curriculum - Room Count: {roundedCurriculumRoomCount}, Should Randomize: {shouldRandomize}");

            }
            else
            {
                Debug.Log("Failed or just started: Retrying...");
            }
            if (shouldRandomize)
            {
                modernRoomGenerator.ClearOldDungeon();
                modernRoomGenerator.Generate();
                foreach (CollaborativeAgentScript cAgent in allCollabAgents)
                {
                    Vector3 randomPosition = roomManager.GetRandomObjectPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
                    // Debug.Log("randomPosition: " + randomPosition);
                    cAgent.gameObject.transform.position = randomPosition;
                    cAgent.Reset();
                }
                Vector3 randomFirePosition = roomManager.GetRandomGoalPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
                randomFirePosition.y = parentOffsetHeight + 0.1f;

                if (symbolOGoal)
                {
                    fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
                    // fireLifeScript.Reset();

                    symbolOGoal.transform.position = randomFirePosition;
                    // Debug.Log("randomFirePosition" + randomFirePosition);
                }
            }
            else
            {
                foreach (CollaborativeAgentScript cAgent in allCollabAgents)
                {
                    Debug.Log("cAgent.sleepPosition " + cAgent.sleepPosition);

                    cAgent.gameObject.transform.position = cAgent.sleepPosition;
                    cAgent.Reset();
                }
            }
        }
        else
        {
            if (modelIndex >= totalModelSets - 1)
            {
                if (shouldRandomize)
                {
                    modernRoomGenerator.ClearOldDungeon();
                    modernRoomGenerator.Generate();
                }
                foreach (CollaborativeAgentScript cAgent in allCollabAgents)
                {
                    Vector3 randomPosition = roomManager.GetRandomObjectPosition() + new Vector3(0f, parentOffsetHeight, 0f);
                    // Debug.Log("randomPosition: " + randomPosition);
                    cAgent.gameObject.transform.position = randomPosition;
                    cAgent.Reset();
                }
                Vector3 randomFirePosition = roomManager.GetRandomGoalPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
                randomFirePosition.y = parentOffsetHeight;

                if (symbolOGoal)
                {
                    fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
                    // fireLifeScript.Reset();

                    symbolOGoal.transform.position = randomFirePosition;
                    // Debug.Log("randomFirePosition" + randomFirePosition);
                }
            }
            else
            {
                foreach (CollaborativeAgentScript cAgent in allCollabAgents)
                {
                    cAgent.gameObject.transform.position = cAgent.sleepPosition;
                    cAgent.Reset();
                }

            }

        }
        // Reset all doors in the scene
        DoorController[] allDoors = area.GetComponentsInChildren<DoorController>();
        foreach (DoorController door in allDoors)
        {
            door.Reset();
        }
        parentOffsetHeight = area.transform.position.y;

        Vector3 newStartPosition = roomManager.startPoint + new Vector3(0f, parentOffsetHeight, 0f);
        newStartPosition.y = parentOffsetHeight;

        transform.position = newStartPosition;
        transform.rotation = Quaternion.identity;
        // transform.position = roomManager.GetStartPoint() + new Vector3(0f, 0.5f, 0f);
        // transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;
        agentCount = 1;
        shouldRandomize = false;
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
                // Debug.Log("Current Model: " + currentModelSet.Models[modelIndexInSet]);
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
