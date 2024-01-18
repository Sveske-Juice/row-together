using UnityEngine.Splines;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;

public class WaterSurfaceGenerator : MonoBehaviour
{
    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField]
    private Transform waterParent;

    [SerializeField, Range(2, 100)]
    private int resolution = 10;

    [SerializeField]
    private float surfaceWidth = 8f;

    [SerializeField]
    private Material waterSurfaceMat;

    private GameObject prevWaterSurface = null;

    private void OnEnable()
    {
        Spline.Changed += BuildWaterSurface;
    }

    private void OnDisable()
    {
        Spline.Changed -= BuildWaterSurface;
    }

    private (List<Vector3>, List<Vector3>) GetVerts(int knotIdx)
    {
        List<Vector3> v1s = new(resolution);
        List<Vector3> v2s = new(resolution);

        float start = ((float) knotIdx) / ((float) splineContainer.Spline.Knots.ToList().Count);

        float step = (1f-start) / (float) resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = start + step * i;

            Debug.Log($"t: {t}, step: {step}, start: {start}");
            Vector3 v1, v2;
            SampleSplineWidth(surfaceWidth, t, out v1, out v2);
            v1s.Add(v1);
            v2s.Add(v2);
        }

        return (v1s, v2s);
    }

    private void SampleSplineWidth(float width, float t, out Vector3 v1, out Vector3 v2)
    {
        float3 position;
        float3 tangent;
        float3 upVec;
        splineContainer.Evaluate(t, out position, out tangent, out upVec);

        float3 right = Vector3.Cross(tangent, upVec).normalized;
        v1 = position + (right * width);
        v2 = position + (-right * width);
    }

    private void BuildWaterSurface(Spline splineChanged, int knotIdx, SplineModification modification)
    {
        if (splineChanged != splineContainer.Spline) return;

        GameObject go = new GameObject($"Water Surface {knotIdx}");
        go.transform.SetParent(waterParent);

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = waterSurfaceMat;
        MeshFilter outputMeshFilter = go.AddComponent<MeshFilter>();

        Mesh waterSurface = new();
        waterSurface.name = "surface water";
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        var waterSurfaceVerts = GetVerts(knotIdx);
        List<Vector3> surfaceVertsP1 = waterSurfaceVerts.Item1;
        List<Vector3> surfaceVertsP2 = waterSurfaceVerts.Item2;

        foreach (var vert in surfaceVertsP1)
        {
            Debug.DrawRay(vert, Vector3.up, Color.magenta, 5f);
        }

        foreach (var vert in surfaceVertsP2)
        {
            Debug.DrawRay(vert, Vector3.up, Color.magenta, 5f);
        }

        Assert.IsTrue(surfaceVertsP1.Count == surfaceVertsP2.Count);

        int length = surfaceVertsP1.Count;
        Debug.Log($"knotIdx: {knotIdx}, res: {resolution}, v len: {surfaceVertsP1.Count}");

        // Connect previous water surface to this
        if (prevWaterSurface != null)
        {
            Mesh prevWaterMesh = prevWaterSurface.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = prevWaterMesh.vertices;

            vertices[vertices.Length - 1] = surfaceVertsP2[0];
            vertices[vertices.Length - 2] = surfaceVertsP1[0];

            prevWaterMesh.SetVertices(vertices);
            prevWaterSurface.GetComponent<MeshFilter>().mesh = prevWaterMesh;
        }

        Debug.Log(knotIdx);
        float uvOffset = 0f;
        for (int i = 1; i <= length; i++)
        {
            Vector3 p1 = surfaceVertsP1[i - 1];
            Vector3 p2 = surfaceVertsP2[i - 1];
            Vector3 p3, p4;

            if (i == length)
            {
                p3 = surfaceVertsP1[0];
                p4 = surfaceVertsP2[0];

            }
            else
            {
                p3 = surfaceVertsP1[i];
                p4 = surfaceVertsP2[i];
            }

            int offset = 4 * (i - 1);

            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;

            verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
            tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });

            float distance = Vector3.Distance(p1, p3) / 4f;
            float uvDist = uvOffset + distance;
            uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDist, 0), new Vector2(uvDist,1) });

            uvOffset += distance;
        }

        prevWaterSurface = go;

        waterSurface.SetVertices(verts);
        waterSurface.SetUVs(channel: 0, uvs);
        waterSurface.SetTriangles(tris, submesh: 0);

        outputMeshFilter.mesh = waterSurface;
    }
}
