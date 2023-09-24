using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonizerCreator : MonoBehaviour
{
    public GameObject dungeonizer;
    // Start is called before the first frame update
    void Start()
    {
        // StartCoroutine(dungeonTimer());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerator dungeonTimer()
    {
        // This means you can create a new dungeon whenever you want.
        Dungeonizer d = dungeonizer.GetComponent("Dungeonizer") as Dungeonizer;

        d.ClearOldDungeon();
        d.Generate();

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(dungeonTimer());
    }
}
