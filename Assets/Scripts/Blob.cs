using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{
    public GameObject agentHolding = null;

    public void GrabBlob(GameObject agent) => agentHolding = agent;

    public void DropBlob() => agentHolding = null;
}
