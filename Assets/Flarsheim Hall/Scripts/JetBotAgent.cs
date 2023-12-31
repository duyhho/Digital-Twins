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
using SimpleJSON; // Assuming you are using SimpleJSON for JSON parsing
public class JetBotAgent : Agent
{
    protected Rigidbody m_AgentRb;  // Changed to protected
    public bool useVectorObs = true;
    public Transform leftWheel;
    public Transform rightWheel;
    public float wheelRotationSpeed = 500f; // Speed at which wheels rotate, adjust as needed
    public float moveSpeed = 2f; // You can adjust the speed as necessary
    public float turnSpeed = 200f; // Adjust turning speed as necessary
    protected int lastAction = -1; // Initialize with a value that doesn't correspond to any valid action
    public bool connectToJetbotFlaskApi = false;
    public bool connectToIMUFlaskAPI = false;

    public const string RobotBaseUrl = "http://192.168.0.129:5000";
    public const string IMUBaseUrl = "http://192.168.0.129:5000";
    public float IMUoffsetToUnity = 14.5f;


    private Quaternion targetRotation;
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        targetRotation = transform.rotation;
        if (connectToIMUFlaskAPI)
        {
            StartCoroutine(FetchSensorData());
        }
    }
    private void Update()
    {
        if (connectToIMUFlaskAPI)
        {
            // Smoothly interpolate to the target rotation
            float rotationSpeed = 2.0f; // Adjust rotation speed as needed
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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

    protected IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.001f); // Wait for .001 second
    }

    public override void OnEpisodeBegin()
    {
        lastAction = -1;
    }
    public virtual void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        Debug.Log("action: " + action);
        // /* Use Force */
        Vector3 forceVector = Vector3.zero;
        Quaternion rotateQuaternion = Quaternion.identity;
        switch (action)
        {
            case 1:
                forceVector = transform.forward * moveSpeed;
                break;
            case 2:
                forceVector = -transform.forward * moveSpeed;
                break;
            case 3:
            // rotateQuaternion = Quaternion.Euler(0f, turnSpeed * Time.fixedDeltaTime, 0f);
            // m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
            // break;
            case 4:
            // rotateQuaternion = Quaternion.Euler(0f, -turnSpeed * Time.fixedDeltaTime, 0f);
            // m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
            // break;
            default:
                //     // Stop the Rigidbody's velocity when no action (or action 0) is selected
                //     m_AgentRb.velocity = Vector3.zero;
                //     m_AgentRb.angularVelocity = Vector3.zero;
                break;
        }
        if (forceVector != Vector3.zero)
        {
            m_AgentRb.AddForce(forceVector, ForceMode.VelocityChange);
        }
        RotateWheels(forceVector.magnitude);

        // Check if the current action is different from the last action
        if (action != lastAction)
        {
            string command = "";

            switch (action)
            {
                case 1:
                    command = "forward";
                    break;
                case 2:
                    command = "backward";
                    break;
                case 3:
                    command = "right";
                    break;
                case 4:
                    command = "left";
                    break;
                default:
                    command = "stop";
                    break;
            }

            // Send the command to the robot
            if (connectToJetbotFlaskApi)
                StartCoroutine(SendCommandToRobot(command));

            // Update the last action
            lastAction = action;
        }
    }
    // This function will rotate the wheels
    protected virtual void RotateWheels(float movementSpeed)
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
    protected virtual IEnumerator SendCommandToRobot(string command)
    {
        string url = command == "stop" ? $"{RobotBaseUrl}/control?command=stop" : $"{RobotBaseUrl}/control?command={command}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(": Received: " + webRequest.downloadHandler.text);
            }
        }
    }
    private IEnumerator FetchSensorData()
    {
        while (true)
        {
            Debug.Log("Calling Fetch Sensor Data");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(IMUBaseUrl + "/get_imu"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError("Sensor Fetch Error: " + webRequest.error);
                }
                else
                {
                    ProcessSensorData(webRequest.downloadHandler.text);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // private void ProcessSensorData(string jsonData)
    // {
    //     Debug.Log("jsonData: " + jsonData);
    //     var N = JSON.Parse(jsonData);
    //     float rotationX = N["X"].AsFloat;
    //     float rotationY = N["Y"].AsFloat;
    //     float rotationZ = N["Z"].AsFloat;


    //     // Calculate the new target rotation from the IMU sensor data
    //     targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotationX + IMUoffsetToUnity, transform.rotation.eulerAngles.z);

    //     Debug.Log("IMU Sensor Data: X: " + rotationX + " Y: " + rotationY + " Z: " + rotationZ);
    // }
    private void ProcessSensorData(string jsonData)
    {
        Debug.Log("jsonData: " + jsonData);
        var N = JSON.Parse(jsonData);

        // Extract Euler angles
        float pitch = N["euler"]["pitch"].AsFloat;
        float roll = N["euler"]["roll"].AsFloat;
        float yaw = N["euler"]["yaw"].AsFloat; //yaw = y axis in unity

        // Calculate the new target rotation from the IMU sensor data
        // Assuming you want to use the 'yaw' value for Unity object's y-axis rotation
        targetRotation = Quaternion.Euler(roll, -yaw + IMUoffsetToUnity, pitch);

        Debug.Log("IMU Sensor Data: Roll: " + roll + " Pitch: " + pitch + " Yaw: " + yaw);
    }

}
