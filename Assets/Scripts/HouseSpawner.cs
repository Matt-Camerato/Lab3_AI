using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HouseSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private int population = 3;
    [SerializeField] private float spawnRate = 5f;
    [SerializeField] private bool canSteal = false;

    [Header("References")]
    public Transform homePos;
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Material agentMaterial;
    [SerializeField] private Transform spawnPos;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private List<HouseSpawner> enemyHouses = new List<HouseSpawner>();

    private float cooldown = 0f;
    private List<GameObject> agents = new List<GameObject>();
    public int score = 0;

    private void Update()
    {
        if(agents.Count < population) SpawnAgent();
    }

    private void SpawnAgent()
    {
        //cooldown between agent spawns
        if(cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        //reset cooldown and spawn new agent
        cooldown = spawnRate;
        GameObject agentObj = Instantiate(agentPrefab, spawnPos.position, Quaternion.identity);
        foreach(MeshRenderer renderer in agentObj.GetComponentsInChildren<MeshRenderer>()) renderer.material = agentMaterial; //set material of agent
        agents.Add(agentObj);

        if(canSteal)
        {
            StealingAgent agent = agentObj.GetComponent<StealingAgent>();
            agent.houseSpawner = this;
            agent.home = homePos;
            agent.enemyHouses = enemyHouses;
        }
        else
        {
            RetrievalAgent agent = agentObj.GetComponent<RetrievalAgent>();
            agent.houseSpawner = this;
            agent.home = homePos;
        }
        
    }

    public void UpdateScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }
}
