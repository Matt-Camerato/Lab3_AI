using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RetrievalAgent : MonoBehaviour
{
    public enum AgentType { Yellow, Red, Blue };
    public enum BehaviorState { Leave, Search, Grab, Chase, Flee, Return };

    public AgentType agentType; //change this to set upon spawning
    public BehaviorState State = BehaviorState.Leave;

    [Header("Search Settings")]
    [SerializeField] private Vector2 searchRangeMinMax = new Vector2(10f, 20f);
    [SerializeField] private float searchRate;
    [SerializeField] private float searchRadius;
    [SerializeField] private Transform home;

    [Header("Flee Settings")]
    [SerializeField] private int numEnemiesToFlee = 1; //0, 1
    private Vector3 fleePos;

    private NavMeshAgent agent;
    private List<Collider> enemies = new List<Collider>();
    private Transform target = null;

    private float searchCooldown = 0f;
    private Transform blobToGrab = null;
    private bool holdingBlob = false;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //get random destination outside of home area
        agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
    }

    private void Update()
    {
        switch(State)
        {
            case BehaviorState.Leave:
                if(Vector3.Distance(transform.position, agent.destination) <= 1f) 
                    State = BehaviorState.Search;
                break;
            case BehaviorState.Search: 
                Search();
                break;
            case BehaviorState.Grab:
                Grab();
                break;
            case BehaviorState.Chase:
                agent.destination = target.position;
                if(Vector3.Distance(transform.position, agent.destination) <= 1f)
                {
                    RetrievalAgent otherAgent = target.GetComponent<RetrievalAgent>();
                    if(otherAgent.holdingBlob)
                    {
                        Transform blob = otherAgent.DropBlob();
                        blob.parent = transform;
                        blob.localPosition = Vector3.up;
                        holdingBlob = true;
                    }
                    else
                    {
                        agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
                        State = BehaviorState.Leave;
                    }
                }
                if(Vector3.Distance(agent.destination, target.GetComponent<RetrievalAgent>().home.position) < 5f)
                {
                    agent.destination = home.position;
                    State = BehaviorState.Return;
                }
                break;
            case BehaviorState.Flee:
                if(enemies.Count < numEnemiesToFlee)
                {
                    agent.speed /= 2;
                    agent.destination = home.position;
                    State = BehaviorState.Return;
                }
                else agent.destination = -10 * (enemies[1].transform.position - agent.nextPosition);
                break;
            case BehaviorState.Return:
                if(Vector3.Distance(transform.position, home.position) <= 1f)
                {
                    //drop off blob if holding one
                    DropBlob();

                    //update state to search for more blobs
                    agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
                    State = BehaviorState.Leave;
                }
                break;
        }
    }

    private void Search()
    {
        if(searchCooldown <= 0f)
        {
            //reset search cooldown
            searchCooldown = searchRate;

            //set new search pos
            Vector2 dir = Random.insideUnitCircle * Random.Range(searchRangeMinMax.x, searchRangeMinMax.y);
            agent.destination = transform.position + new Vector3(dir.x, 0, dir.y);
        }
        else searchCooldown -= Time.deltaTime;
    }

    private void Grab()
    {
        if(Vector3.Distance(transform.position, blobToGrab.position) <= 1f)
        {
            //grab blob and place on head
            blobToGrab.parent = transform;
            blobToGrab.GetComponent<Rigidbody>().isKinematic = true;
            blobToGrab.localPosition = Vector3.up;
            holdingBlob = true;

            //update destination and state to return with blob
            agent.destination = home.position;
            State = BehaviorState.Return;
        }
    }

    private Transform DropBlob()
    {
        if(blobToGrab != null)
        {
            blobToGrab.GetComponent<Rigidbody>().isKinematic = false;
            blobToGrab.SetParent(null);
            blobToGrab = null;
            holdingBlob = false;
            
        }
        return blobToGrab;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if another agent, check if enemy
        if(other.GetComponent<RetrievalAgent>())
        {
            //if enemy, add them to collider list
            if(agentType != other.GetComponent<RetrievalAgent>().agentType) enemies.Add(other);

            //check if this agent can chase
            if(numEnemiesToFlee == -1)
            {
                target = other.transform;
                State = BehaviorState.Chase;
            }
            else if(enemies.Count >= numEnemiesToFlee && State != BehaviorState.Flee)
            {
                agent.speed = 2 * agent.speed;
                State = BehaviorState.Flee; //check if agent flees
            }
        }
        
        if(other.CompareTag("Blob") && State != BehaviorState.Grab)
        {
            blobToGrab = other.transform;

            //ignore blob if already dropped off at home
            if(Vector3.Distance(blobToGrab.position, home.position) < 3f)
            {
                blobToGrab = null;
                return;
            }
            agent.destination = blobToGrab.position;
            State = BehaviorState.Grab;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //remove enemy from list
        if(enemies.Contains(other)) enemies.Remove(other);

        //make enemy return after chasing
        if(numEnemiesToFlee == -1 && other.transform == target)
        {
            target = null;
            agent.destination = home.position;
            State = BehaviorState.Return;
        }
    }
}
