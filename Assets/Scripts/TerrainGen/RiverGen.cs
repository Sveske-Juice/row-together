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
    float curveAmount = 8;
    int idx = 0;

    // Start is called before the first frame update
    void Awake()
    {
        sc = GetComponent<SplineContainer>();
    }

    private void Start()
    {
        BezierKnot startPoint = new BezierKnot(new Vector3(1f, 1f, 1f));
        sc.Spline.Add(startPoint);
        prevKnot = startPoint;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < cooldown)
            timer++;
        else {
            BezierKnot nextKnot;
            if (idx == 0)
            {
                nextKnot = new BezierKnot(new Vector3(prevKnot.Position.x + 1f, prevKnot.Position.y, prevKnot.Position.z + 1f));
            }
            else if (idx < 4)
            {
                nextKnot = new BezierKnot(new Vector3(prevKnot.Position.x + 5, prevKnot.Position.y, prevKnot.Position.z + 5));
            }
            else
            {
                nextKnot = new BezierKnot(new Vector3(prevKnot.Position.x + Random.Range(-curveAmount, curveAmount), prevKnot.Position.y, prevKnot.Position.z + 10));
            }
            // sc.AddSpline(new Spline());

            sc.Splines[sc.Splines.Count - 1].Add(nextKnot);

            // Connect the knots of each spline
            // sc.KnotLinkCollection.Link(new SplineKnotIndex(sc.Splines.Count - 1, 1), new SplineKnotIndex(sc.Splines.Count, 0));

            prevKnot = nextKnot;
            timer = 0; //Reset time
            idx++;
        }
    }
}
