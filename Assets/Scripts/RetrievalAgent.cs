using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RetrievalAgent : MonoBehaviour
{
    public enum BehaviorState { Leave, Search, Grab, Return };

    public BehaviorState State = BehaviorState.Leave;

    public HouseSpawner houseSpawner;

    [Header("Search Settings")]
    public Transform home;
    [SerializeField] private float searchRange;
    [SerializeField] private float searchRate;
    [SerializeField] private float searchRadius;

    private NavMeshAgent agent;
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
                if(Vector3.Distance(agent.nextPosition, agent.destination) <= 2f) State = BehaviorState.Search;
                break;
            case BehaviorState.Search: 
                Search();
                break;
            case BehaviorState.Grab:
                Grab();
                break;
            case BehaviorState.Return:
                if(Vector3.Distance(transform.position, home.position) <= 1f)
                {
                    if(blobToGrab != null)
                    {
                        Destroy(blobToGrab.gameObject);
                        houseSpawner.UpdateScore(1);
                    }

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
        agent.destination = blobToGrab.position;
        if(Vector3.Distance(transform.position, blobToGrab.position) <= 1f)
        {
            //grab blob and place on head
            blobToGrab.parent = transform;
            blobToGrab.GetComponent<Rigidbody>().isKinematic = true;
            blobToGrab.localPosition = Vector3.up;
            holdingBlob = true;
            blobToGrab.GetComponent<Blob>().GrabBlob(gameObject);

            //update destination and state to return with blob
            agent.destination = home.position;
            State = BehaviorState.Return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Blob") && !holdingBlob)
        {
            //ignore blobs already dropped off at home
            if(Vector3.Distance(other.transform.position, home.position) < 5f) return;

            //ignore blobs being held by someone else
            if(other.GetComponent<Blob>().agentHolding != null) return;

            blobToGrab = other.transform;
            State = BehaviorState.Grab;
        }
    }
}
