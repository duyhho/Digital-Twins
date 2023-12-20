using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using System;
using System.IO;
using TMPro;
public class CollabFireAgent : DungeonAgentFire
{
    public FireLifeScript fireLifeScript;
    float parentOffsetHeight;
    public int agentCount = 1;
    public bool shouldRandomize = false;
    List<CollabModelStats> modelStatsList = new List<CollabModelStats>();
    [SerializeField]
    TMP_Text attemptText;

    public class CollabModelStats : ModelStats
    {
        // public float cumulativeTimeToReachFire = 0f;
        // public float averageTime = 0f;
        // public int successfulAttempts = 0;
        // public int attemptCount = 0;
        public float cumulativeTimeToCureFire = 0f;
        public float averageTimeCureFire = 0f;
        public float totalAgentsCollab = 0f;
        public float averageAgentsCollab = 0f;
    }
    public override void Initialize()
    {
        base.Initialize();
        for (int i = 0; i < totalModelSets; i++)
        {
            modelStatsList.Add(new CollabModelStats());
        }
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

        if (useVectorObs)
        {
            sensor.AddObservation(agentCount);
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
            CollabModelStats currentStats = modelStatsList[modelIndex];
            float timeToReachFire = Time.time - episodeStartTime;
            currentStats.cumulativeTimeToReachFire += timeToReachFire;

            currentStats.successfulAttempts += 1; // Increment successful attempts
            float averageTime = currentStats.cumulativeTimeToReachFire / currentStats.successfulAttempts;
            currentStats.averageTime = averageTime;

            currentStats.totalAgentsCollab += agentCount;
            currentStats.averageAgentsCollab = currentStats.totalAgentsCollab / currentStats.attemptCount;

            // Adjust additional time based on agent count
            float additionalTime = 0;
            // switch (agentCount)
            // {
            //     case 1:
            //         additionalTime = 300;
            //         break;
            //     case 2:
            //         additionalTime = 200;
            //         break;
            //     case 3:
            //         additionalTime = 100;
            //         break;
            //         // You can add more cases if needed
            // }
            additionalTime = 300 / agentCount;
            currentStats.cumulativeTimeToCureFire += timeToReachFire + additionalTime;
            float averageTimeCureFire = currentStats.cumulativeTimeToCureFire / currentStats.successfulAttempts;
            currentStats.averageTimeCureFire = averageTimeCureFire;

            // Include the model set in your log messages
            // Debug.Log("Model Set: " + modelIndex);
            Debug.Log("Time to reach the fire: " + timeToReachFire + " seconds");
            Debug.Log("Average time to reach the fire successfully (Model Set " + modelIndex + "): " + averageTime + " seconds");
            Debug.Log("Success rate (Model Set " + modelIndex + "): " + ((float)currentStats.successfulAttempts / currentStats.attemptCount) * 100 + "% (" + currentStats.successfulAttempts + "/" + currentStats.attemptCount + ")");
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
            //maxAttempts = how many scenarios/layouts
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

                    Debug.Log(" - Agent Count: " + modelStats.averageAgentsCollab);
                    Debug.Log(" - Average Time to Cure Fire: " + modelStats.averageTimeCureFire + " seconds");


                }
                ExportModelStatsToCSV(modelStatsList);
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

        }
        if (isEvaluation)
        {
            // If all models have been tested in the current layout
            if (modelIndex >= totalModelSets - 1)
            {
                modelIndex = -1;  // Reset the index to -1
                shouldRandomize = true;
                // Debug.Log("shouldRandomize: true");
                // ResetEnvironment(); // Generate a new environment layout here
            }
            else
            {
                shouldRandomize = false;
            }
            modelIndex += 1;  // Increment modelIndex at the start

            // Ensure modelIndex is in bounds before using it to index into modelStatsList
            if (modelIndex < totalModelSets)
            {
                modelStatsList[modelIndex].attemptCount += 1;
                if (attemptText != null)
                {
                    attemptText.text = modelStatsList[modelIndex].attemptCount + " / " + maxAttempts;
                }
            }

            Debug.Log("ON EPISODE END! Current Model Set: " + modelIndex);


        }

    }
    public override void ResetEnvironment()
    {
        Debug.Log("Reset Environment!");
        bool randomizeCAgents = false;
        if (symbolOGoal)
        {
            fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
            fireLifeScript.Reset();
            // symbolOGoal.transform.position = randomFirePosition;
        }
        CollaborativeAgentScript[] allCollabAgents = area.GetComponentsInChildren<CollaborativeAgentScript>();
        if (!isEvaluation) //isTraining
        {
            if (shouldRandomize)
            {
                randomizeCAgents = true;
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

                Vector3 randomFirePosition = roomManager.GetRandomGoalPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
                randomFirePosition.y = parentOffsetHeight + 0.1f;

                if (symbolOGoal)
                {
                    fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
                    // fireLifeScript.Reset();

                    symbolOGoal.transform.position = randomFirePosition;
                    // Debug.Log("randomFirePosition" + randomFirePosition);
                }
                shouldRandomize = false;

            }
            if (randomizeCAgents)
            {
                foreach (CollaborativeAgentScript cAgent in allCollabAgents)
                {
                    Vector3 randomPosition = roomManager.GetRandomObjectPosition();
                    randomPosition.y = parentOffsetHeight + 0.1f;
                    // Debug.Log("randomPosition: " + randomPosition);
                    cAgent.gameObject.transform.position = randomPosition;
                    cAgent.Reset();
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
            Debug.Log("is eval!!");


            if (shouldRandomize)
            {
                Debug.Log("New Generation!");
                modernRoomGenerator.ClearOldDungeon();
                modernRoomGenerator.Generate();
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
        if (m_Configuration != -1)
        {
            ConfigureAgent(m_Configuration);
            m_Configuration = -1;
        }
    }

    /// <summary>
    /// Configures the agent's neural network model based on the specified room configuration.
    /// </summary>
    /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    protected override void ConfigureAgent(int config)
    {
        if (isEvaluation) //keep this for training logic
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
                string behaviorName = "CollabRLCu_NoLearning";

                int modelIndexInSet = config - 2;
                // Debug.Log("Current Model: " + currentModelSet.Models[modelIndexInSet]);
                if (modelIndexInSet >= 0 && modelIndexInSet < currentModelSet.Models.Count)
                {
                    SetModel(behaviorName, currentModelSet.Models[modelIndexInSet]);
                    Debug.Log("Current Model: " + currentModelSet.Models[modelIndexInSet]);

                }
                else
                {
                    SetModel(behaviorName, currentModelSet.Models[currentModelSet.Models.Count - 1]);
                    Debug.Log("Current Model: " + currentModelSet.Models[currentModelSet.Models.Count - 1]);

                }
            }


        }
    }
    public void ExportModelStatsToCSV(List<CollabModelStats> modelStatsList)
    {
        // Define the file path relative to the Assets directory
        string relativePath = "Dungeonizer/Imitation Learning/Regular IL/Collab Fire/Curriculum/No Learning/Exports/ModelStats.csv";
        string filePath = Path.Combine(Application.dataPath, relativePath);

        // Ensure the directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (StreamWriter file = new StreamWriter(filePath))
        {
            // Write the header line
            file.WriteLine("Model Set, Average Time, Successful Attempts, Attempt Count, Average Agents Collaboration, Success Rate, Average Time to Cure Fire");

            // Write data for each model
            for (int i = 0; i < modelStatsList.Count; i++)
            {
                var modelStats = modelStatsList[i];
                float averageTime = modelStats.cumulativeTimeToReachFire / modelStats.successfulAttempts;
                float successRate = ((float)modelStats.successfulAttempts / modelStats.attemptCount) * 100;

                // Calculate the average time to cure fire
                float averageTimeToCureFire = modelStats.successfulAttempts > 0 ? modelStats.cumulativeTimeToCureFire / modelStats.successfulAttempts : 0;

                // Create a line of CSV text
                string line = $"{i}, {averageTime}, {modelStats.successfulAttempts}, {modelStats.attemptCount}, {modelStats.averageAgentsCollab}, {successRate}, {averageTimeToCureFire}";

                // Write the line to the file
                file.WriteLine(line);
            }
        }
    }


}
