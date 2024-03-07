using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf;

public class TFSubscriber : MonoBehaviour
{
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<TFMessage>("/tf", PrintTFMessage);
    }

    void PrintTFMessage(TFMessage message)
    {
        foreach (var transform in message.transforms)
        {
            Debug.Log($"Transform: {transform}");
        }
    }
}
