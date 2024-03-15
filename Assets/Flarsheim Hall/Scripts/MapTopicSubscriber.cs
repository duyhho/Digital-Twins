using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using System.Collections.Generic;

public class MapTopicSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/map";
    public int occupancyThreshold = 50; // Threshold for considering a cell as an obstacle
    public GameObject obstaclePrefab; // Assign a prefab for the obstacles in the Unity Inspector

    private Vector3 offset = Vector3.zero; // Offset for the position of instantiated obstacles
    private float scale = 1f; // Scaling factor for the size of instantiated obstacles
    private List<GameObject> obstacles = new List<GameObject>(); // List to store instantiated obstacles

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<OccupancyGridMsg>(topicName, MapCallback);

        JetBotAgent jetBotAgent = FindObjectOfType<JetBotAgent>();
        if (jetBotAgent != null)
        {
            offset = jetBotAgent.jetbotOrigin;
            offset.y = 1;
            scale = jetBotAgent.translationScaleFactor;
        }

    }

    void MapCallback(OccupancyGridMsg mapMessage)
    {
        // Process the map data here
        // Debug.Log("Received map with resolution: " + mapMessage.info.resolution);

        // Clear existing obstacles
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        obstacles.Clear();

        // Instantiate new obstacles based on the map data
        for (int y = 0; y < mapMessage.info.height; y++)
        {
            for (int x = 0; x < mapMessage.info.width; x++)
            {
                int index = (int)(y * mapMessage.info.width + x);
                if (mapMessage.data[index] >= occupancyThreshold)
                {
                    float posX = (float)(mapMessage.info.origin.position.x + x * mapMessage.info.resolution);
                    float posY = (float)(mapMessage.info.origin.position.y + y * mapMessage.info.resolution);

                    Vector3 position = new Vector3(posY, 0, posX) * scale + offset;
                    GameObject newObstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
                    obstacles.Add(newObstacle);

                    // Debug.Log($"Obstacle at ({posX}, {posY})");
                }
            }
        }
    }
}
