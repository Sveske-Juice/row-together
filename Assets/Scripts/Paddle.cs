using UnityEngine;

public class Paddle : MonoBehaviour
{
    public Animator animator;

    [SerializeField, Range(0f, 50f)]
    private float paddleForce = 15f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Activate(Transform boatTransform, Rigidbody rb)
    {
        Debug.Log("Activar");
        animator.Play("Row");
        Vector3 force = (transform.parent.position - transform.position).normalized * paddleForce;
        force.y = 0f;
        //Vector2 force = boatTransform.forward * paddleForce;
        rb.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
    }
}
