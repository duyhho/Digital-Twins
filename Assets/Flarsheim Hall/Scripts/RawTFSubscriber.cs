using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

public class RawTFSubscriber : MonoBehaviour
{
    void Start()
    {
        // Subscribe to the /tf topic as a generic Message
        ROSConnection.GetOrCreateInstance().Subscribe<Message>("tf", PrintRawTFMessage);
    }

    void PrintRawTFMessage(Message rawMessage)
    {
        // Print the raw message directly
        Debug.Log($"Raw /tf data: {rawMessage}");
    }
}
