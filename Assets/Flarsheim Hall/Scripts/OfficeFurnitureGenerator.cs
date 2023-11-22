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
    public List<GameObject> laptopPrefabs;
    public GameObject tabletPrefab;
    GameObject chair;
    GameObject laptop;
    GameObject tablet;
    public Transform parentObject;
    int randomIndex = 0;
    public List<DataObject> chairPositions = new List<DataObject>();
    public List<DataObject> laptopPositions = new List<DataObject>();
    public List<DataObject> tabletPositions = new List<DataObject>();

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
        generateChairs();

        generateLaptop();

        generateTablet();
        // foreach (DataObject dataObject in chairPositions)
        // {
        //     // Create a Vector3 representing the desired rotation (in degrees)
        //     rotationVector = new Vector3(0f, dataObject.ry, 0f);

        //     // Convert the Vector3 to a Quaternion using Quaternion.Euler
        //     rotationQuaternion = Quaternion.Euler(rotationVector);
        //     //Debug.Log($"{{x: {dataObject.x}, y: {dataObject.y}, z: {dataObject.z}}}");
        //     randomIndex = Random.Range(0, chairPrefabs.Count);
        //     chair = Instantiate(chairPrefabs[randomIndex], parentObject) as GameObject;
        //     chair.transform.parent = parentObject;
        //     chair.transform.localPosition = new Vector3(dataObject.px, 0f, dataObject.pz);
        //     chair.transform.localRotation = rotationQuaternion;
        // }
    }


    private void generateChairs()
    {
        foreach (DataObject dataObject in chairPositions)
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

    private void generateLaptop()
    {
        foreach (DataObject dataObject in laptopPositions)
        {
            // Create a Vector3 representing the desired rotation (in degrees)
            rotationVector = new Vector3(0f, dataObject.ry, 0f);

            // Convert the Vector3 to a Quaternion using Quaternion.Euler
            rotationQuaternion = Quaternion.Euler(rotationVector);
            //Debug.Log($"{{x: {dataObject.x}, y: {dataObject.y}, z: {dataObject.z}}}");
            randomIndex = Random.Range(0, laptopPrefabs.Count);
            laptop = Instantiate(laptopPrefabs[randomIndex], parentObject) as GameObject;
            laptop.transform.parent = parentObject;
            laptop.transform.localPosition = new Vector3(dataObject.px, 20f, dataObject.pz);
            laptop.transform.localRotation = rotationQuaternion;
        }
    }


    private void generateTablet()
    {
        foreach (DataObject dataObject in tabletPositions)
        {
            // Create a Vector3 representing the desired rotation (in degrees)
            rotationVector = new Vector3(-90f, dataObject.ry, -80f);

            // Convert the Vector3 to a Quaternion using Quaternion.Euler
            rotationQuaternion = Quaternion.Euler(rotationVector);
            //Debug.Log($"{{x: {dataObject.x}, y: {dataObject.y}, z: {dataObject.z}}}");
            randomIndex = Random.Range(0, chairPrefabs.Count);
            tablet = Instantiate(tabletPrefab, parentObject) as GameObject;
            tablet.transform.parent = parentObject;
            tablet.transform.localPosition = new Vector3(dataObject.px, 20f, dataObject.pz);
            tablet.transform.localRotation = rotationQuaternion;
        }
    }
    private void DestroyAllChairsWithTag()
    {
        GameObject[] chairsToDestroy = GameObject.FindGameObjectsWithTag("chair");
        foreach (GameObject chair in chairsToDestroy)
        {
            DestroyImmediate(chair);
        }

        GameObject[] laptopsToDestroy = GameObject.FindGameObjectsWithTag("laptops");
        foreach (GameObject laptop in laptopsToDestroy)
        {
            DestroyImmediate(laptop);
        }

        GameObject[] tabletsToDestroy = GameObject.FindGameObjectsWithTag("laptops");
        foreach (GameObject tablet in tabletsToDestroy)
        {
            DestroyImmediate(tablet);
        }
    }
}
