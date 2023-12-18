using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
public class DungeonAgentFire : Agent
{
    [System.Serializable]
    public class NNModelSet
    {
        public List<NNModel> Models = new List<NNModel>();
    }
    [SerializeField]
    List<ModelStats> modelStatsList = new List<ModelStats>();

    [System.Serializable]
    public class ModelStats
    {
        public float cumulativeTimeToReachFire = 0f;
        public float averageTime = 0f;
        public int successfulAttempts = 0;
        public int attemptCount = 0;
    }

    protected List<Renderer> m_GroundRenderers = new List<Renderer>();
    public GameObject area;
    public GameObject symbolOGoal;

    public ModernRoomGenerator modernRoomGenerator;
    public bool useVectorObs;

    protected Rigidbody m_AgentRb;  // Changed to protected
    protected Material m_GroundMaterial;  // Changed to protected
    protected Renderer m_GroundRenderer;  // Changed to protected
    protected ParticleSystem fireParticleSystem;  // Changed to protected
    protected ParticleSystem waterParticleSystem;  // Changed to protected
    protected RoomManager roomManager;  // Changed to protected

    protected HallwaySettings m_HallwaySettings;  // Changed to protected
    protected int m_Selection;  // Changed to protected
    protected openandclosedoor DoorComponent;  // Changed to protected

    protected Material m_GoalMaterial;  // Changed to protected
    protected Renderer m_GoalRenderer;  // Changed to protected
    public int m_Configuration;

    protected EnvironmentParameters m_ResetParams;
    [Header("Evaluation Metrics")]
    public int maxAttempts = 5;
    public bool isEvaluation = false;
    protected float episodeStartTime;
    [SerializeField]
    protected int modelIndex = -1;
    [SerializeField]
    protected int totalModelSets = 0;
    [SerializeField]
    protected List<NNModelSet> evaluationModelSets = new List<NNModelSet>();
    public override void Initialize()
    {
        // Debug.Log("Init");
        // symbolOGoal = modernRoomGenerator.ReturnExitGameObject();
        roomManager = modernRoomGenerator.GetComponent<RoomManager>();
        m_HallwaySettings = FindObjectOfType<HallwaySettings>();
        m_AgentRb = GetComponent<Rigidbody>();
        // m_GroundRenderer = ground.GetComponent<Renderer>();
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("ground");

        // Get the Renderer component from each GameObject and add it to our list
        foreach (GameObject groundObject in groundObjects)
        {
            Renderer rend = groundObject.GetComponent<Renderer>();
            if (rend != null)
            {
                m_GroundRenderers.Add(rend);
            }
        }
        // m_GroundMaterial = m_GroundRenderer.material;

        m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
        m_GoalMaterial = m_GoalRenderer.material;

        totalModelSets = evaluationModelSets.Count;
        for (int i = 0; i < totalModelSets; i++)
        {
            modelStatsList.Add(new ModelStats());
        }


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
        }

    }


    protected virtual IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        if (m_GroundRenderers.Count == 0)
        {
            GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("ground");

            // Get the Renderer component from each GameObject and add it to our list
            foreach (GameObject groundObject in groundObjects)
            {
                Renderer rend = groundObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    m_GroundRenderers.Add(rend);
                }
            }
            // m_GroundMaterial = m_GroundRenderer.material; // Note: You might need a mechanism to set this.
        }

        List<Renderer> validRenderers = new List<Renderer>();

        foreach (Renderer rend in m_GroundRenderers)
        {
            if (rend) // Check if the Renderer is still valid
            {
                validRenderers.Add(rend);
                rend.material = mat;
            }
        }

        yield return new WaitForSeconds(time);

        foreach (Renderer rend in validRenderers)
        {
            if (rend) // Check if the Renderer is still valid
            {
                rend.material = m_GroundMaterial;
            }
        }
    }




    protected virtual IEnumerator SwapGoalMaterial(Material mat, float time)
    {
        if (m_GoalRenderer)
        {
            m_GoalRenderer.material = mat;
            yield return new WaitForSeconds(time);
            m_GoalRenderer.material = m_GoalMaterial;
        }
        else
        {

        }
    }
    public virtual void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
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


    protected virtual void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("wall"))
        {


        }
    }
    protected virtual void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("ground"))
        {

        }
        if (col.gameObject.CompareTag("wall"))
        {

        }
    }
    protected virtual IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.01f); // Wait for 1 second
        if (!isEvaluation)
        {
            ResetEnvironment();
            // if (curriculumTraining)
            // {
            //     float newRoomCount = m_ResetParams.GetWithDefault("room_count", -1);
            //     Debug.Log("newRoomCount: " + newRoomCount);
            //     modernRoomGenerator.maximumRoomCount = (int)newRoomCount;
            // }

        }

        EndEpisode();

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("door_switch"))
        {

        }

        // Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("symbol_O_Goal") || other.gameObject.CompareTag("fire"))
        {
            if (isEvaluation)
            {
                UpdateModelStats();

            }
            SetReward(2f);
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            // StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));
            StartCoroutine(DelayedEndEpisode()); // Use the coroutine here
        }

    }

    protected virtual void UpdateModelStats()
    {
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
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }
    protected virtual void OnLayoutChange()
    {
        foreach (var modelStats in modelStatsList)
        {
            modelStats.cumulativeTimeToReachFire = 0f;
            modelStats.attemptCount = 0;
            modelStats.averageTime = 0f;
            modelStats.successfulAttempts = 0;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (isEvaluation)
        {
            CheckCurrentEvaluationModels();

            if (modelIndex >= 0) //
            {
                Debug.Log("ON EPISODE BEGIN"
                        + "- Model Set: " + modelIndex
                        + "- Attempt Count: " + modelStatsList[modelIndex].attemptCount
                        + "- Success: " + modelStatsList[modelIndex].successfulAttempts);
            }
        }
        else
        {
            Debug.Log("ON EPISODE BEGIN!");
        }
        //
        episodeStartTime = Time.time;
        m_Configuration = modernRoomGenerator.maximumRoomCount;

        // Reset all doors in the scene
        DoorController[] allDoors = area.GetComponentsInChildren<DoorController>();
        foreach (DoorController door in allDoors)
        {
            door.Reset();
        }

        if (symbolOGoal)
        {
            // Vector3 randomFirePosition = roomManager.GetRandomGoalPosition();
            // symbolOGoal.transform.position = randomFirePosition;
            m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
            m_GoalMaterial = m_GoalRenderer.material;

            Transform fireTransform = symbolOGoal.transform.Find("CFX4 Fire");
            if (fireTransform != null)
            {
                fireParticleSystem = fireTransform.GetComponent<ParticleSystem>();

                if (fireParticleSystem != null)
                {
                    fireParticleSystem.Clear();
                    fireParticleSystem.Play();
                }
                else
                {
                    Debug.LogWarning("CFX4 Fire component does not have a ParticleSystem attached.");
                }
            }
            else
            {
                Debug.LogWarning("CFX4 Fire object could not be found.");
            }
        }
        else
        {
            Debug.LogWarning("symbolOGoal is not set.");
        }

        transform.position = roomManager.GetStartPoint() + new Vector3(0f, 0.5f, 0f);
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;
    }
    protected virtual void CheckCurrentEvaluationModels()
    {
        if (isEvaluation)
        {
            // If all models have been tested in the current layout
            if (modelIndex >= totalModelSets - 1)
            {
                modelIndex = -1;  // Reset the index to -1
                ResetEnvironment(); // Generate a new environment layout here
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
    public virtual void ResetEnvironment()
    {
        modernRoomGenerator.ClearOldDungeon();
        modernRoomGenerator.Generate();
    }
    public virtual void PlayWaterAndStopFire()
    {
        Debug.Log("Play Water");
        if (symbolOGoal)
        {
            fireParticleSystem = symbolOGoal.transform.Find("CFX4 Fire").GetComponent<ParticleSystem>();
            waterParticleSystem = symbolOGoal.transform.Find("CFX2_Big_Splash (No Collision)").GetComponent<ParticleSystem>();
            if (!waterParticleSystem.gameObject.activeInHierarchy)
            {
                waterParticleSystem.gameObject.SetActive(true);
            }

            if (!waterParticleSystem.isPlaying)
            {
                waterParticleSystem.Play();
            }

            if (fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Stop();
            }
        }

    }
    protected virtual void FixedUpdate()
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
    protected virtual void ConfigureAgent(int config)
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
            // Debug.Log("Config: " + config);

            // if (modelIndex >= 0)
            // {
            //     if (evaluationModelSets.Count == 0 || evaluationModelSets[modelIndex].Models.Count == 0)
            //     {
            //         Debug.LogError("CUSTOM ERROR: The evaluation model set is empty. Please assign model sets in the inspector.");
            //         return;
            //     }
            //     // Debug.Log("modelIndex: " + modelIndex);
            //     // Fetch the appropriate NNModelSet based on the current modelIndex
            //     NNModelSet currentModelSet = evaluationModelSets[modelIndex];
            //     // Fetch the appropriate NNModel based on the config value
            //     string behaviorName = config switch
            //     {
            //         2 => "RoomTwo",
            //         3 => "RoomThree",
            //         4 => "RoomFour",
            //         5 => "RoomFive",
            //         _ => "RoomFive",
            //     };

            //     int modelIndexInSet = config - 2;
            //     if (modelIndexInSet >= 0 && modelIndexInSet < currentModelSet.Models.Count)
            //     {
            //         SetModel(behaviorName, currentModelSet.Models[modelIndexInSet]);
            //     }
            //     else
            //     {
            //         SetModel(behaviorName, currentModelSet.Models[currentModelSet.Models.Count - 1]);
            //     }
            // }


        }
    }
}
