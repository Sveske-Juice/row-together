using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField, Range(0f, 50f)]
    private float paddleForce = 15f;

    public void Activate(Transform boatTransform, Rigidbody rb)
    {
        Vector3 force = boatTransform.forward * paddleForce * Time.fixedDeltaTime;

        rb.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
    }
}
