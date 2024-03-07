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
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
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

    public const string RobotBaseUrl = "http://192.168.0.142:5000";
    public const string IMUBaseUrl = "http://192.168.0.142:5000";
    public float IMUoffsetToUnity = 14.5f;
    public float translationScaleFactor = 5f;

    private Quaternion targetRotation;
    private Vector3 jetbotOrigin;
    private Vector3 lastUpdatedPosition;
    private Transform baseFootprintTransform;
    ROSConnection ros;
    private float linearSpeed = 0.25f;
    private float angularSpeed = 1.0f;
    public string topicName = "cmd_vel";
    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        targetRotation = transform.rotation;
        jetbotOrigin = transform.localPosition;
        if (connectToIMUFlaskAPI)
        {
            // StartCoroutine(FetchSensorData());
            FetchSensorDataROSTCP();
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<TwistMsg>(topicName);
        }
        else
        {
            turnSpeed = 70f;
            moveSpeed = 1f;
        }
    }
    private void Update()
    {
        // if (connectToIMUFlaskAPI)
        // {
        //     // Smoothly interpolate to the target rotation
        //     // float rotationSpeed = 0.5f; // Adjust rotation speed as needed
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        //     // StartCoroutine(RotateOverTime(transform.rotation, targetRotation, 1f / turnSpeed));
        //     // Smoothly interpolate to the target position
        //     transform.localPosition = Vector3.Lerp(transform.localPosition, lastUpdatedPosition, moveSpeed * Time.deltaTime);
        // }
        if (connectToIMUFlaskAPI)
        {
            FetchSensorDataROSTCP();

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
        Debug.Log("action: " + action + ", moveSpeed: " + moveSpeed + ", turnSpeed: " + turnSpeed);
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
                rotateQuaternion = Quaternion.Euler(0f, turnSpeed * Time.fixedDeltaTime, 0f);
                // m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
                break;
            case 4:
                rotateQuaternion = Quaternion.Euler(0f, -turnSpeed * Time.fixedDeltaTime, 0f);
                // m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
                break;
            default:
                // Stop the Rigidbody's velocity when no action (or action 0) is selected
                m_AgentRb.velocity = Vector3.zero;
                m_AgentRb.angularVelocity = Vector3.zero;
                break;
        }

        RotateWheels(forceVector.magnitude);

        // Check if the current action is different from the last action
        if (connectToJetbotFlaskApi)
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


            // SendCommandToRobot(command);
            SendCommandToRobotROSTCP(command);


            // Update the last action
            lastAction = action;

        }

        else
        {

            if (forceVector != Vector3.zero)
            {
                Debug.Log("forceVector: " + forceVector.ToString());
                m_AgentRb.AddForce(forceVector, ForceMode.VelocityChange);

            }
            if (rotateQuaternion != Quaternion.identity)
            {
                m_AgentRb.MoveRotation(m_AgentRb.rotation * rotateQuaternion);
            }
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
    // protected virtual IEnumerator SendCommandToRobot(string command)
    // {
    //     string url = command == "stop" ? $"{RobotBaseUrl}/control?command=stop" : $"{RobotBaseUrl}/control?command={command}";

    //     using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
    //     {
    //         // Request and wait for the desired page.
    //         yield return webRequest.SendWebRequest();

    //         if (webRequest.isNetworkError || webRequest.isHttpError)
    //         {
    //             Debug.Log(": Error: " + webRequest.error);
    //         }
    //         else
    //         {
    //             Debug.Log(": Received: " + webRequest.downloadHandler.text);
    //         }
    //     }
    // }
    protected void SendCommandToRobot(string command)
    {
        string url = command == "stop" ? $"{RobotBaseUrl}/control?command=stop" : $"{RobotBaseUrl}/control?command={command}";

        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        var asyncOperation = webRequest.SendWebRequest();

        asyncOperation.completed += (operation) =>
        {
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(": Received: " + webRequest.downloadHandler.text);
            }

            // Dispose of the web request to free resources
            webRequest.Dispose();
        };
    }
    private IEnumerator FetchSensorData()
    {
        while (true)
        {
            Debug.Log("Calling Fetch Sensor Data");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(IMUBaseUrl + "/get_tf"))
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
    private void FetchSensorDataROSTCP()
    {
        Debug.Log("Calling Fetch Sensor Data through ROS-TCP");
        GameObject baseFootprint = GameObject.Find("map/odom/base_footprint");
        if (baseFootprint != null)
        {
            transform.rotation = baseFootprint.transform.rotation;

            // Calculate the change in position of the base_footprint relative to the map
            Vector3 positionChange = baseFootprint.transform.position;

            // Apply the position change to the transform.localPosition relative to its original position
            transform.localPosition = jetbotOrigin + positionChange * translationScaleFactor;
        }
    }
    protected void SendCommandToRobotROSTCP(string command)
    {
        TwistMsg twist = new TwistMsg();

        switch (command)
        {
            case "forward":
                twist.linear.x = linearSpeed;
                break;
            case "backward":
                twist.linear.x = -linearSpeed;
                break;
            case "right":
                twist.angular.z = -angularSpeed;
                break;
            case "left":
                twist.angular.z = angularSpeed;
                break;
            case "stop":
                // No need to set anything, default values are 0
                break;
        }

        ros.Publish(topicName, twist);
    }

    private void ProcessSensorData(string jsonData)
    {
        Debug.Log("jsonData: " + jsonData);
        var N = JSON.Parse(jsonData);

        // Extract Euler angles
        float pitch = N["rotation_euler_degrees"]["pitch"].AsFloat;
        float roll = N["rotation_euler_degrees"]["roll"].AsFloat;
        float yaw = N["rotation_euler_degrees"]["yaw"].AsFloat;

        // Extract translation and apply axis mapping
        float jetbotX = N["translation"]["x"].AsFloat * translationScaleFactor;
        float jetbotY = N["translation"]["y"].AsFloat * translationScaleFactor;
        float jetbotZ = N["translation"]["z"].AsFloat * translationScaleFactor;

        // Map JetBot's coordinate system to Unity's coordinate system
        Vector3 mappedPosition = new Vector3(-jetbotY, jetbotZ, jetbotX);

        // Calculate the new target rotation from the IMU sensor data
        targetRotation = Quaternion.Euler(roll, -yaw + IMUoffsetToUnity, pitch);

        lastUpdatedPosition = mappedPosition + jetbotOrigin;
        Debug.Log("IMU Sensor Data: Roll: " + roll + " Pitch: " + pitch + " Yaw: " + yaw);
        Debug.Log("Mapped Translation Data: X: " + mappedPosition.x + " Y: " + mappedPosition.y + " Z: " + mappedPosition.z);
    }
    IEnumerator RotateOverTime(Quaternion originalRotation, Quaternion finalRotation, float duration)
    {
        if (duration > 0f)
        {
            float startTime = Time.time;
            float endTime = startTime + duration;
            transform.rotation = originalRotation;
            yield return null;
            while (Time.time < endTime)
            {
                float progress = (Time.time - startTime) / duration;
                // progress will equal 0 at startTime, 1 at endTime.
                transform.rotation = Quaternion.Slerp(originalRotation, finalRotation, progress);
                yield return null;
            }
        }
        transform.rotation = finalRotation;
    }


    private void OnDestroy()
    {
        if (connectToJetbotFlaskApi)
        {
            // StartCoroutine(SendCommandToRobot("stop"));
            // SendCommandToRobot("stop");
            SendCommandToRobotROSTCP("stop");

        }
    }
}
