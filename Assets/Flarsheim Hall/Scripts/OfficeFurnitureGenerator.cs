using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeFurnitureGenerator : MonoBehaviour
{

    public List<GameObject> chairPrefabs;
    GameObject chair;
    public Transform parentObject;
    int randomIndex = 0;
    void Start()
    {

    }

    public void generate()
    {
        // First, destroy all existing chairs
        DestroyAllChairsWithTag();

        /*Generate new chairs*/
        Debug.Log("Button is clicked");

        randomIndex = Random.Range(0, chairPrefabs.Count);
        chair = Instantiate(chairPrefabs[randomIndex], parentObject) as GameObject;
        chair.transform.parent = parentObject;
        chair.transform.localPosition = new Vector3(0f, 0f, 0f);
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
