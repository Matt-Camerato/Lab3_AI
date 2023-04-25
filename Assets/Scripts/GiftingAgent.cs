using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GiftingAgent : MonoBehaviour
{
    public enum BehaviorState { Leave, Search, Grab, Steal, Gift, Return };

    public BehaviorState State = BehaviorState.Leave;

    public HouseSpawner houseSpawner;

    [Header("Steal Settings")]
    public List<HouseSpawner> enemyHouses = new List<HouseSpawner>();
    [SerializeField] private GameObject blobPrefab;
    private HouseSpawner targetHouse = null;
    private HouseSpawner giftingHouse = null;

    [Header("Search Settings")]
    public Transform home;
    [SerializeField] private Vector2 searchRangeMinMax;
    [SerializeField] private float searchRate;

    [Header("State Settings")]
    [SerializeField] private MeshRenderer stateIndicator;
    [SerializeField] private Material leaveMaterial;
    [SerializeField] private Material searchMaterial;
    [SerializeField] private Material grabMaterial;
    [SerializeField] private Material stealMaterial;
    [SerializeField] private Material giftMaterial;
    [SerializeField] private Material returnMaterial;

    private NavMeshAgent agent;
    private float searchCooldown = 0f;
    private Transform blobToGrab = null;
    private bool holdingBlob = false;
    private int searchCount = 0;
    
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
            case BehaviorState.Steal:
                //if house has no more blobs to steal, go back to searching
                if(targetHouse.score <= 0)
                {
                    //update state to search for more blobs
                    agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
                    stateIndicator.material = leaveMaterial;
                    State = BehaviorState.Leave;
                }

                //steal a blob from the target house upon reaching it
                if(Vector3.Distance(agent.nextPosition, agent.destination) <= 2f)
                {
                    targetHouse.UpdateScore(-1); //decrement house score

                    //spawn blob and parent to agent
                    GameObject blob = Instantiate(blobPrefab, transform);
                    blob.GetComponent<Rigidbody>().isKinematic = true;
                    blob.transform.localPosition = Vector3.up;
                    holdingBlob = true;
                    blob.GetComponent<Blob>().GrabBlob(gameObject);
                    blobToGrab = blob.transform;

                    //determine whether to gift the blob or return it
                    GiftOrReturn();
                }
                break;
            case BehaviorState.Gift:
                if(Vector3.Distance(agent.nextPosition, agent.destination) <= 2f)
                {
                    if(blobToGrab != null)
                    {
                        Destroy(blobToGrab.gameObject);
                        blobToGrab = null;
                        holdingBlob = false;
                        giftingHouse.UpdateScore(1);
                    }

                    //update state to search for more blobs
                    agent.destination = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
                    stateIndicator.material = leaveMaterial;
                    State = BehaviorState.Leave;
                }
                break;
            case BehaviorState.Return:
                if(Vector3.Distance(agent.nextPosition, home.position) <= 2f)
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
        if(searchCount > 5)
        {
            //set this agents house as the best house
            HouseSpawner bestHouse = houseSpawner;
            int bestScore = houseSpawner.score;
            
            //determine if any other house has a better score
            foreach(HouseSpawner house in enemyHouses)
            {
                if(house.score <= 0) continue;
                if(house.score < bestScore) continue;

                bestHouse = house;
                bestScore = house.score;
            }

            //if other house has better score, steal from them
            if(bestHouse != houseSpawner)
            {
                targetHouse = bestHouse;
                agent.destination = bestHouse.homePos.position;
                stateIndicator.material = stealMaterial;
                State = BehaviorState.Steal;
            }
        }

        if(searchCooldown <= 0f)
        {
            //reset search cooldown
            searchCooldown = searchRate;

            //increment search count
            searchCount++;

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

            //determine whether to gift the blob or return it
            GiftOrReturn();
        }
    }

    private void GiftOrReturn()
    {
        //set this agents house as the worst house
        HouseSpawner worstHouse = houseSpawner;
        int worstScore = houseSpawner.score;
        
        //determine if any other house has a worse score
        foreach(HouseSpawner house in enemyHouses)
        {
            if(house == targetHouse) continue; //don't gift back to the house that was just stolen from

            if(house.score >= worstScore) continue;

            worstHouse = house;
            worstScore = house.score;
        }

        //if other house has worse score, gift to them
        if(worstHouse != houseSpawner)
        {
            giftingHouse = worstHouse;
            agent.destination = worstHouse.homePos.position;
            stateIndicator.material = giftMaterial;
            State = BehaviorState.Gift;
        }
        else
        {
            //otherwise return with the blob
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

            searchCount = 0; //reset search count
            blobToGrab = other.transform;
            stateIndicator.material = grabMaterial;
            State = BehaviorState.Grab;
        }
    }
}
