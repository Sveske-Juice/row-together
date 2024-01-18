using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RiverGen : MonoBehaviour
{
    public SplineContainer sc;
    public BezierKnot prevKnot;
    float timer = 0;
    float cooldown = 30;
    SplineExtrude se;
    float curveAmount = 8;

    // Start is called before the first frame update
    void Awake()
    {
        sc = GetComponent<SplineContainer>();
        se = gameObject.GetComponent<SplineExtrude>();
    }

    private void Start()
    {
        BezierKnot startPoint = new BezierKnot(new Vector3(1f, 1f, 1f));
        sc.Spline.Add(startPoint);
        prevKnot = startPoint;

        
        se.Radius = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < cooldown)
            timer++;
        else {
            BezierKnot nextKnot = new BezierKnot(new Vector3(prevKnot.Position.x + Random.Range(-curveAmount, curveAmount), 0, prevKnot.Position.z + 10));
            sc.Spline.Add(nextKnot);
            se.Radius = 3;
            prevKnot = nextKnot;
            timer = 0; //Reset time
        }
    }
}
