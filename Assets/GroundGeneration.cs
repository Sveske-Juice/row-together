using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityUtils;

public class GroundGeneration : MonoBehaviour
{
    public GameObject groundSection;
    RiverGen rg;
    float timer = 0;
    float cooldown = 100;
    GameObject prevSection;

    void Start()
    {
        RiverGen.NewPieceSpawned += SpawnGround;
        Vector3 startSpawnPos = new Vector3(0, -0.08f, 40);
        Instantiate(groundSection, startSpawnPos, Quaternion.identity);
    }
    void OnDisable()
    {

        RiverGen.NewPieceSpawned -= SpawnGround;
    }

    void SpawnGround(BezierKnot knot)
    {
        Vector3 nextSpawnPos = knot.Position;
        GameObject nextSection = Instantiate(groundSection, nextSpawnPos.With(y: -0.08f), Quaternion.identity);
        nextSection.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, 50f);
    }

}
