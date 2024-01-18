using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGeneration : MonoBehaviour
{
    public GameObject groundSection;
    RiverGen rg;
    float timer = 0;
    float cooldown = 100;
    GameObject prevSection;

    void Start()
    {
        rg = GameObject.Find("RiverGenerator").GetComponent<RiverGen>();
        Vector3 startSpawnPos = new Vector3(0, -0.08f, 40);
        GameObject nextSection = Instantiate(groundSection, startSpawnPos, Quaternion.identity);
        prevSection = nextSection;
    }

    void Update()
    {
        if (timer < cooldown)
            timer++;
        else
        {
            Vector3 nextSpawnPos = new Vector3(rg.prevKnot.Position.x, prevSection.transform.position.y, prevSection.transform.position.z + 40);
            GameObject nextSection = Instantiate(groundSection, nextSpawnPos, Quaternion.identity);
            prevSection = nextSection;
            timer = 0; //Reset timer
        }
    }
}
