using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobSpawner : MonoBehaviour
{
    public GameObject blobPrefab;

    private void Update()
    {
        //check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            //cast a ray out from mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //if ray hit an agent, destroy it
                if(hit.transform.CompareTag("RetrievalAgent"))
                {
                    hit.transform.GetComponent<RetrievalAgent>().houseSpawner.agents.Remove(hit.transform.gameObject);
                    Destroy(hit.transform.gameObject);
                }
                else if(hit.transform.CompareTag("StealingAgent"))
                {
                    hit.transform.GetComponent<StealingAgent>().houseSpawner.agents.Remove(hit.transform.gameObject);
                    Destroy(hit.transform.gameObject);
                }
                else if(hit.transform.CompareTag("GiftingAgent"))
                {
                    hit.transform.GetComponent<GiftingAgent>().houseSpawner.agents.Remove(hit.transform.gameObject);
                    Destroy(hit.transform.gameObject);
                }
                else
                {
                    //otherwise spawn a blob at mouse position
                    Vector3 spawnPosition = hit.point + (Vector3.up * 0.7f);
                    Instantiate(blobPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }
    }
}
