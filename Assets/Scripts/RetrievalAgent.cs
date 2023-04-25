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
    [SerializeField] private Vector2 searchRangeMinMax;
    [SerializeField] private float searchRate;

    [Header("State Settings")]
    [SerializeField] private MeshRenderer stateIndicator;
    [SerializeField] private Material leaveMaterial;
    [SerializeField] private Material searchMaterial;
    [SerializeField] private Material grabMaterial;
    [SerializeField] private Material returnMaterial;

    private NavMeshAgent agent;
    private float searchCooldown = 0f;
    private Transform blobToGrab = null;
    private bool holdingBlob = false;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //get random destination outside of home area
        agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));

        //set initial indicator material
        stateIndicator.material = leaveMaterial;
    }

    private void Update()
    {
        switch(State)
        {
            case BehaviorState.Leave:
                if(Vector3.Distance(agent.nextPosition, agent.destination) <= 2f)
                {
                    stateIndicator.material = searchMaterial;
                    State = BehaviorState.Search;
                }
                break;
            case BehaviorState.Search: 
                Search();
                break;
            case BehaviorState.Grab:
                Grab();
                break;
            case BehaviorState.Return:
                if(Vector3.Distance(transform.position, home.position) <= 2f)
                {
                    if(blobToGrab != null)
                    {
                        Destroy(blobToGrab.gameObject);
                        blobToGrab = null;
                        holdingBlob = false;
                        houseSpawner.UpdateScore(1);
                    }

                    //update state to search for more blobs
                    agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
                    stateIndicator.material = leaveMaterial;
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

        //make sure blob isn't held by another agent
        if(blobToGrab.GetComponent<Blob>().agentHolding != null)
        {
            //update state to search for more blobs
            agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
            stateIndicator.material = leaveMaterial;
            State = BehaviorState.Leave;
        }

        //check if close enough to grab blob and return with it
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
            stateIndicator.material = returnMaterial;
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
            stateIndicator.material = grabMaterial;
            State = BehaviorState.Grab;
        }
    }
}
