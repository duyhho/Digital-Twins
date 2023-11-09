using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using System;
public class JetBotAgent : Agent
{
    protected Rigidbody m_AgentRb;  // Changed to protected
    public bool useVectorObs = true;
    // protected EnvironmentParameters m_ResetParams;

    // public FireLifeScript fireLifeScript;
    // float parentOffsetHeight;
    // public int agentCount = 1;
    // public bool shouldRandomize = false;
    public float moveSpeed = 2f; // You can adjust the speed as necessary
    public float turnSpeed = 200f; // Adjust turning speed as necessary
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();

        // m_ResetParams = Academy.Instance.EnvironmentParameters;
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
        }

        MoveAgent(actionBuffers.DiscreteActions);
    }

    protected void OnCollisionEnter(Collision col)
    {

    }
    protected void OnCollisionStay(Collision col)
    {

    }
    protected IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.001f); // Wait for .001 second
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("fire"))
        {
            EndEpisode();
        }

    }

    public override void OnEpisodeBegin()
    {

    }
    // public void MoveAgent(ActionSegment<int> act)
    // {
    //     var dirToGo = Vector3.zero;
    //     var rotateDir = Vector3.zero;

    //     var action = act[0];
    //     switch (action)
    //     {
    //         case 1:
    //             dirToGo = transform.forward * 1f;
    //             break;
    //         case 2:
    //             dirToGo = transform.forward * -1f;
    //             break;
    //         case 3:
    //             rotateDir = transform.up * 1f;
    //             break;
    //         case 4:
    //             rotateDir = transform.up * -1f;
    //             break;
    //     }
    //     transform.Rotate(rotateDir, Time.deltaTime * 200f);
    //     m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
    // }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        Vector3 moveVector = Vector3.zero;
        Quaternion rotateQuaternion = Quaternion.identity;

        switch (action)
        {
            case 1:
                moveVector = transform.forward * moveSpeed * Time.fixedDeltaTime;
                break;
            case 2:
                moveVector = -transform.forward * moveSpeed * Time.fixedDeltaTime;
                break;
            case 3:
                rotateQuaternion = Quaternion.Euler(0f, turnSpeed * Time.fixedDeltaTime, 0f);
                break;
            case 4:
                rotateQuaternion = Quaternion.Euler(0f, -turnSpeed * Time.fixedDeltaTime, 0f);
                break;
        }

        // Apply the movements
        m_AgentRb.MovePosition(m_AgentRb.position + moveVector);
        m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
    }
    // public void MoveAgent(ActionSegment<int> act)
    // {
    //     var dirToGo = Vector3.zero;
    //     var rotateDir = Vector3.zero;

    //     var action = act[0];
    //     switch (action)
    //     {
    //         case 1:
    //             dirToGo = transform.forward;
    //             break;
    //         case 2:
    //             dirToGo = -transform.forward;
    //             break;
    //         case 3:
    //             rotateDir = Vector3.up;
    //             break;
    //         case 4:
    //             rotateDir = -Vector3.up;
    //             break;
    //     }

    //     // Set the velocity directly for movement
    //     m_AgentRb.velocity = dirToGo * moveSpeed;

    //     // Apply a rotation speed limit
    //     if (m_AgentRb.angularVelocity.magnitude < turnSpeed)
    //     {
    //         m_AgentRb.AddTorque(rotateDir * turnSpeed);
    //     }
    // }

    // // Call MoveAgent from FixedUpdate instead of OnActionReceived to align with the physics update
    // void FixedUpdate()
    // {
    //     // Assuming OnActionReceived is setting some class variable for the action to take, which is then used here.
    //     // You need to ensure that the actionBuffers are being stored appropriately for this FixedUpdate call.
    //     MoveAgent(storedActionBuffers.DiscreteActions);
    // }

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
}
