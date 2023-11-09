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
    // public int agentCount = 1;
    // public bool shouldRandomize = false;
    public Transform leftWheel;
    public Transform rightWheel;
    public float wheelRotationSpeed = 500f; // Speed at which wheels rotate, adjust as needed
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
        RotateWheels(moveVector.magnitude);
    }
    // This function will rotate the wheels
    private void RotateWheels(float movementSpeed)
    {
        // Calculate the rotation amount. If moveVector.magnitude is too small, you might want to use a fixed value for visual effect.
        float rotationAmount = -wheelRotationSpeed * movementSpeed * Time.fixedDeltaTime;

        // Rotate the wheels around their local X-axis
        leftWheel.Rotate(rotationAmount, 0, 0);
        rightWheel.Rotate(rotationAmount, 0, 0);
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
}
