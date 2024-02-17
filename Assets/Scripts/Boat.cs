using UnityEngine;
using System.Linq;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Boat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField] float streamDrag = 1f;
    [SerializeField] float keepOnPathDrag = 1f;
    [SerializeField] float maxSpeed = 8f;
    [SerializeField] float rotateSpeed = 1f;

    [SerializeField] private Paddle nwPaddle;
    [SerializeField] private Paddle nePaddle;
    [SerializeField] private Paddle sePaddle;
    [SerializeField] private Paddle swPaddle;

    Quaternion targetRotation;

    // Since unity doesn't support enums in UnityEvents
    public void NWActivate() => ActivatePaddle(Rotation.NW);
    public void NEActivate() => ActivatePaddle(Rotation.NE);
    public void SEActivate() => ActivatePaddle(Rotation.SE);
    public void SWActivate() => ActivatePaddle(Rotation.SW);

    void Update()
    {
        // FIXME: this is so bad but somehow rb rotation constraints don't want
        // to do their fu**ing job so idgaf
        transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
    }

    void FixedUpdate()
    {
        // Push boat in river direction
        int closestSlineIdxToBoat = IndexOfClosestPoint(splineContainer, transform.position);

        Vector3 flowDirection;
        BezierKnot closestKnot = splineContainer.Spline.Knots.ElementAt(closestSlineIdxToBoat);
        if (splineContainer.Spline.Count() > 1 && closestSlineIdxToBoat != 0)
        {
            flowDirection = splineContainer.Spline.Knots.ElementAt(closestSlineIdxToBoat+1).Position - closestKnot.Position;
        }
        else
        {
            if (splineContainer.Spline.Knots.Count() <= 1) return;

            flowDirection = -splineContainer.Spline.Knots.ElementAt(closestSlineIdxToBoat+1).Position - new float3(transform.position);
        }
        flowDirection.y = 0f;
        flowDirection.Normalize();


        Vector3 keepOnPathForce = splineContainer.Spline.Knots.ElementAt(closestSlineIdxToBoat+1).Position - new float3(transform.position);
        keepOnPathForce.y = 0f;
        keepOnPathForce.Normalize();

        Vector3 netForce = (flowDirection * streamDrag) + (keepOnPathForce * keepOnPathDrag);

        rb.AddForce(netForce);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        // Lerp towards target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.forward, flowDirection), rotateSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }

    int IndexOfClosestPoint(SplineContainer sc, Vector3 wsPos)
    {
        int idx = 0;
        if (sc.Spline.Knots.Count() < 1) return idx;

        float closest = Vector3.Distance(sc.Spline.Knots.First().Position, transform.position);
        for (int i = 0; i < sc.Spline.Knots.Count(); i++)
        {
            var knot = sc.Spline.Knots.ElementAt(i);
            float dist = Vector3.Distance(knot.Position, transform.position);
            if (dist <= closest)
            {
                closest = dist;
                idx = i;
            }
        }

        return idx;
    }

    public void ActivatePaddle(Rotation rotation)
    {
        switch (rotation)
        {
            case Rotation.NW:
                nwPaddle.Activate(transform, rb);
                break;

            case Rotation.NE:
                nePaddle.Activate(transform, rb);
                break;

            case Rotation.SE:
                sePaddle.Activate(transform, rb);
                break;

            case Rotation.SW:
                swPaddle.Activate(transform, rb);
                break;

            default:
                throw new System.Exception("Invalid paddle");
        }
    }
}
