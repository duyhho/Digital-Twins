using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

public class ScanTopicSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/scan";
    public GameObject obstaclePrefab; // Assign a prefab for the obstacles in the Unity Inspector
    public float distanceThreshold = 1f; // Threshold for considering a point as an obstacle

    private List<GameObject> obstacles = new List<GameObject>(); // List to store instantiated obstacles
    private Vector3 offset = Vector3.zero; // Offset for the position of instantiated obstacles
    private float scale = 1f; // Scaling factor for the size of instantiated obstacles

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(topicName, ScanCallback);

        JetBotAgent jetBotAgent = FindObjectOfType<JetBotAgent>();
        if (jetBotAgent != null)
        {
            offset = jetBotAgent.jetbotOriginGlobal;
            offset.y = obstaclePrefab.transform.localScale.y / 2;
            scale = jetBotAgent.translationScaleFactor;
            // scale = 1;
        }
    }

    void ScanCallback(LaserScanMsg scan)
    {
        // Clear existing obstacles
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        obstacles.Clear();

        GameObject jetbotBase = GameObject.Find("map/odom/base_footprint");
        if (jetbotBase != null)
        {
            Vector3 jetbotPosition = jetbotBase.transform.position;
            Quaternion jetbotRotation = jetbotBase.transform.rotation;

            // Instantiate new obstacles based on the scan data
            for (int i = 0; i < scan.ranges.Length; i++)
            {
                if (scan.ranges[i] < distanceThreshold)
                {
                    // Adjust the angle by -90 degrees (or π/2 radians)
                    float adjustedAngle = scan.angle_min + i * scan.angle_increment - Mathf.PI / 2;

                    // Now calculate localPosX and localPosY using the adjusted angle
                    float localPosX = Mathf.Cos(adjustedAngle) * scan.ranges[i];
                    float localPosY = Mathf.Sin(adjustedAngle) * scan.ranges[i];

                    Vector3 localPosition = new Vector3(localPosX, 0, localPosY);
                    // Vector3 worldPosition = jetbotPosition + jetbotRotation * localPosition + offset;
                    Vector3 worldPosition = (jetbotPosition + jetbotRotation * localPosition) * scale + offset;


                    GameObject newObstacle = Instantiate(obstaclePrefab, worldPosition, Quaternion.identity);
                    obstacles.Add(newObstacle);
                }
            }
        }
        else
        {
            Debug.LogError("map/odom/base_footprint object not found in the scene.");
        }
    }

}
