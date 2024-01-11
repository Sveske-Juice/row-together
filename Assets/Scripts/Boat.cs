using UnityEngine;

public class Boat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [SerializeField] private Paddle nwPaddle;
    [SerializeField] private Paddle nePaddle;
    [SerializeField] private Paddle sePaddle;
    [SerializeField] private Paddle swPaddle;

    // Since unity doesn't support enums in UnityEvents
    public void NWActivate() => ActivatePaddle(Rotation.NW);
    public void NEActivate() => ActivatePaddle(Rotation.NE);
    public void SEActivate() => ActivatePaddle(Rotation.SE);
    public void SWActivate() => ActivatePaddle(Rotation.SW);

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
