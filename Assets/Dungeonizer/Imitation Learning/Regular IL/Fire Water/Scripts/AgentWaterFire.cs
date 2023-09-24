using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
public class AgentWaterFire : DungeonAgentFire
{
    [Header("Agent State Visualization")]
    public Material normalMaterial;
    public Material waterMaterial;

    [SerializeField]
    bool m_CarryWater = false;
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

        RaycastUpdateGrid(); //this doesn't collect any observations ,just updating grid cell status;
        if (useVectorObs)
        {
            sensor.AddObservation(m_CarryWater ? 1 : 0);
            sensor.AddObservation(StepCount / (float)MaxStep);
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
        yield return new WaitForSeconds(0.01f); // Wait for 1 second
        if (!isEvaluation) //in training mode
        {
            if (m_CarryWater)
                ResetEnvironment();
        }

        EndEpisode();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // base.OnTriggerEnter(other);
        if (other.gameObject.CompareTag("door_switch"))
        {
            /*Reward is set in Door Button.cs script */
            // AddReward(1f);
            gridManager.SetDoor(transform.position);
            gridManager.SetVisited(transform.position);

        }
        if (other.gameObject.CompareTag("water"))
        {
            CarryWater();
        }
        // Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("fire"))
        {
            if (isEvaluation)
            {
                UpdateModelStats();

            }
            if (m_CarryWater)
            {
                Debug.Log("Carrying Water! Reward!");
                SetReward(2f);
            }
            else
            {
                Debug.Log("Not Carrying Water! Punish!");

                SetReward(-2f);
            }
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            // StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));
            StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

            gridManager.SetFire(transform.position);
            gridManager.SetVisited(transform.position);

        }


    }
    void CarryWater()
    {
        m_CarryWater = true;
        gameObject.GetComponentInChildren<Renderer>().material = waterMaterial;
    }
    protected override void UpdateModelStats()
    {
        // base.UpdateModelStats();
        if (modelIndex >= 0 && m_CarryWater == true)
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
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
        m_CarryWater = false;
    }
    protected override void CheckCurrentEvaluationModels()
    {
        base.CheckCurrentEvaluationModels();
    }
    public override void ResetEnvironment()
    {
        base.ResetEnvironment();
    }
    public override void PlayWaterAndStopFire()
    {
        base.PlayWaterAndStopFire();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // /// <summary>
    // /// Configures the agent's neural network model based on the specified room configuration.
    // /// </summary>
    // /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    // protected override void ConfigureAgent(int config)
    // {
    //     base.ConfigureAgent(config);
    // }
}
