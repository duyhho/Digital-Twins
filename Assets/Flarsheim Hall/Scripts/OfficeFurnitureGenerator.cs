using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class DataObject
{
    public int px;
    public int ry;
    public int pz;

    // Constructor to initialize the object
    public DataObject(int px, int ry, int pz)
    {
        this.px = px;
        this.ry = ry;
        this.pz = pz;
    }
}

public class OfficeFurnitureGenerator : MonoBehaviour
{
    // Define a class for your data objects




    public List<GameObject> chairPrefabs;
    GameObject chair;
    public Transform parentObject;
    int randomIndex = 0;
    public List<DataObject> dataObjectList = new List<DataObject>();

    Vector3 rotationVector;
    Quaternion rotationQuaternion;



    void Start()
    {

    }

    public void generate()
    {
        // First, destroy all existing chairs
        DestroyAllChairsWithTag();

        /*Generate new chairs*/
        Debug.Log("Button is clicked");

        // randomIndex = Random.Range(0, chairPrefabs.Count);
        // chair = Instantiate(chairPrefabs[randomIndex], parentObject) as GameObject;
        // chair.transform.parent = parentObject;
        // chair.transform.localPosition = new Vector3(0f, 0f, 0f);

        // for (int i = 0; i <= 8; i++)
        // {
        //     randomIndex = Random.Range(0, chairPrefabs.Count);
        //     chair = Instantiate(chairPrefabs[randomIndex], parentObject) as GameObject;
        //     chair.transform.parent = parentObject;
        //     chair.transform.localPosition = new Vector3(i * 1.5f, 0f, i * 1.5f);
        // }

        foreach (DataObject dataObject in dataObjectList)
        {
            // Create a Vector3 representing the desired rotation (in degrees)
            rotationVector = new Vector3(0f, dataObject.ry, 0f);

            // Convert the Vector3 to a Quaternion using Quaternion.Euler
            rotationQuaternion = Quaternion.Euler(rotationVector);
            //Debug.Log($"{{x: {dataObject.x}, y: {dataObject.y}, z: {dataObject.z}}}");
            randomIndex = Random.Range(0, chairPrefabs.Count);
            chair = Instantiate(chairPrefabs[randomIndex], parentObject) as GameObject;
            chair.transform.parent = parentObject;
            chair.transform.localPosition = new Vector3(dataObject.px, 0f, dataObject.pz);
            chair.transform.localRotation = rotationQuaternion;
        }
    }
    private void DestroyAllChairsWithTag()
    {
        GameObject[] chairsToDestroy = GameObject.FindGameObjectsWithTag("chair");
        foreach (GameObject chair in chairsToDestroy)
        {
            DestroyImmediate(chair);
        }
    }
}
