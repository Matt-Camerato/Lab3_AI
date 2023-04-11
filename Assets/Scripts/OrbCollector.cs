using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OrbCollector : MonoBehaviour
{
    [SerializeField] private GameObject orbIndicator;
    [SerializeField] private Transform homePosition; //position the agent will return to after collecting orbs
    [SerializeField] private float searchRadius = 10f; //radius around the agent where orbs will be searched for
    [SerializeField] private LayerMask orbLayer; //layer containing the orbs
    [SerializeField] private float wanderRadius = 5f; //radius around the agent where it will randomly wander
    [SerializeField] private float wanderTimer = 5f; //time between each wander

    private NavMeshAgent agent;
    private Collider[] nearbyOrbs;
    private int currentOrbIndex = 0;
    private float timer;
    private Vector3 wanderPosition;
    private Vector3 leavePosition;

    private enum AgentState
    {
        Leave,
        Search,
        Grab,
        Return
    }

    private AgentState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindNearbyOrbs();
        leavePosition = new Vector3(Random.Range(-15f, 15f), 1, Random.Range(-15f, 15f));
        currentState = AgentState.Leave;
        timer = wanderTimer;
    }

    private void Update()
    {
        switch (currentState)
        {
            case AgentState.Leave:
                agent.SetDestination(leavePosition);
                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = AgentState.Search;
                }
                break;
            
            case AgentState.Search:
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    timer += Time.deltaTime;
                    if (timer >= wanderTimer)
                    {
                        wanderPosition = RandomNavSphere(transform.position, wanderRadius, -1);
                        agent.SetDestination(wanderPosition);
                        timer = 0f;
                    }
                    FindNearbyOrbs();
                }
                if (nearbyOrbs.Length > 0) currentState = AgentState.Grab;
                break;

            case AgentState.Grab:
                agent.SetDestination(nearbyOrbs[0].transform.position);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Destroy(nearbyOrbs[0].gameObject);
                    orbIndicator.SetActive(true);
                    currentState = AgentState.Return;
                }
                break;

            case AgentState.Return:
                agent.SetDestination(homePosition.position);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if(orbIndicator.activeSelf)
                    {
                        //spawn orb at home
                        orbIndicator.SetActive(false);
                    }

                    leavePosition = new Vector3(Random.Range(-15f, 15f), 1, Random.Range(-15f, 15f));
                    currentState = AgentState.Leave;
                }
                break;
        }
    }

    private void FindNearbyOrbs()
    {
        nearbyOrbs = Physics.OverlapSphere(transform.position, searchRadius, orbLayer);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}
