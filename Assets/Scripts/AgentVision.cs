using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AgentVision : MonoBehaviour
{
    [SerializeField] private int numEnemiesToFlee = 1;

    private RetrievalAgent agent;
    private List<Collider> enemies = new List<Collider>();

    private void Start()
    {
        agent = GetComponent<RetrievalAgent>();
    }
}
