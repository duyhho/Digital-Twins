using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMesh : MonoBehaviour
{
    [SerializeField]
    private Transform movePositionTransform;
    private ObstacleAgent navMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<ObstacleAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        navMeshAgent.SetDestination(movePositionTransform.position);
    }
}
