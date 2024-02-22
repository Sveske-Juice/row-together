using UnityEngine;

public class Paddle : MonoBehaviour
{
    public Animator animator;

    [SerializeField, Range(0f, 50f)]
    private float paddleForce = 15f;

    [SerializeField, Range(0f, 50f)]
    private float forwardBoost = 0.5f;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Activate(Transform boatTransform, Rigidbody rb)
    {
        animator.Play("Row");
        Vector3 force = (transform.parent.position - transform.position).normalized * paddleForce;
        force.y = 0f;
        //Vector2 force = boatTransform.forward * paddleForce;
        rb.AddForceAtPosition(force, transform.position, ForceMode.Impulse);

        rb.AddForce(boatTransform.forward * forwardBoost);
    }
}
