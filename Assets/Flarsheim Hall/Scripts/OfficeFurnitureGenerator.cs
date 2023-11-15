using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeFurnitureGenerator : MonoBehaviour
{
    // // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {

    // }

    public GameObject chair;
    //public GameObject targetTable;
    public Transform parentObject;

    public void generate()
    {
        Debug.Log("Button is clicked");
        //Debug.Log(targetTable.transform.localPosition);
        //targetTable.Instantiate(chair, (0,0,0), targetTable.transform.rotation, this.targetTable.transform.localPosition);
        //(Instantiate (chair, this.targetTable.transform.localPosition) as GameObject).transform.parent = chair.transform;
        chair = Instantiate(chair, parentObject) as GameObject;
        chair.transform.parent = transform;
        //targetTable.transform.SetParent(chair.transform);
    }
}
