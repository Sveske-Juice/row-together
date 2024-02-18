using System.Linq;
using UnityEngine;

public class DestroyOnDist : MonoBehaviour
{
    Transform trackingObj;
    float distThreshold;
    bool checkMesh = false;


    public void Init(Transform trackingObj, float distThreshold, bool checkMesh = false)
    {
        this.trackingObj = trackingObj;
        this.distThreshold = distThreshold;
        this.checkMesh = checkMesh;
    }

    void Update()
    {
        if (trackingObj == null) return;

        Vector3 origin;
        if (checkMesh)
        {
            origin = GetComponent<MeshFilter>().mesh.vertices.FirstOrDefault();
        }
        else
        {
            origin = transform.position;
        }

        if (Vector3.Distance(origin, trackingObj.position) > distThreshold)
            Destroy(gameObject);
    }
}
