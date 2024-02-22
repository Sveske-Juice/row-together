using UnityEngine.Splines;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;

// WARNING: this code is shit idgaf about this project
public class WaterSurfaceGenerator : MonoBehaviour
{
    // NOTE: since spline instantiate component is retarded we have to this ourselves
    [Header("Tree spawning")]
    public GameObject[] treePrefabs;
    public float spawnChance = 0.4f;
    public float distFromSplineCenter = 5f;

    [Header("Side collisions")]
    public bool debug = false;
    public Material colMat;
    public float colHeight = 10f;
    public PhysicMaterial sideWallMat;

    public float destroyDist = 20f;

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
        Spline.Changed += BuildWaterSurfaces;
    }

    private void OnDisable()
    {
        Spline.Changed -= BuildWaterSurfaces;
    }

    private (List<Vector3>, List<Vector3>) GetVerts(int knotIdx, bool spawnTrees = true)
    {
        List<Vector3> v1s = new(resolution);
        List<Vector3> v2s = new(resolution);

        float start = ((float) knotIdx) / ((float) splineContainer.Spline.Knots.ToList().Count);

        float step = (1f-start) / (float) resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = start + step * i;

            // Debug.Log($"t: {t}, step: {step}, start: {start}");
            Vector3 v1, v2;
            SampleSplineWidth(surfaceWidth, t, out v1, out v2);
            v1s.Add(v1);
            v2s.Add(v2);

            if (spawnTrees && knotIdx > 1)
            {
                if (UnityEngine.Random.Range(0f, 1f) < spawnChance) continue;
                SampleSplineWidth(distFromSplineCenter, t, out v1, out v2);
                var tree1 = Instantiate(treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length - 1)], v1, Quaternion.identity, waterParent);
                var tree2 = Instantiate(treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length - 1)], v2, Quaternion.identity, waterParent);

                tree1.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, 40f);
                tree2.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, 40f);
            }
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

    private void BuildWaterSurfaces(Spline splineChanged, int knotIdx, SplineModification modification)
    {
        if (knotIdx == -1) return;
        if (splineChanged != splineContainer.Spline) return;
        if (modification != SplineModification.KnotInserted) return;

        GameObject go = new GameObject($"Water Surface {knotIdx}");
        go.transform.SetParent(waterParent);

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = waterSurfaceMat;
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

        // Generate junction between two surfaces
        if (prevWaterSurface != null)
        {
            Mesh prevWaterMesh = prevWaterSurface.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = prevWaterMesh.vertices;

            Vector3 p1 = vertices[vertices.Length - 2];
            Vector3 p2 = vertices[vertices.Length - 1];
            Vector3 p3 = surfaceVertsP1[0];
            Vector3 p4 = surfaceVertsP2[0];
            int t1 = 0;
            int t2 = 2;
            int t3 = 3;

            int t4 = 3;
            int t5 = 1;
            int t6 = 0;

            GameObject juncGo = new GameObject($"Water junction {knotIdx}");
            juncGo.transform.SetParent(waterParent);
            Mesh junction = new Mesh();
            junction.name = $"Water junction {knotIdx}";

            MeshFilter mf = juncGo.AddComponent<MeshFilter>();
            MeshRenderer juncmr = juncGo.AddComponent<MeshRenderer>();
            juncmr.material = waterSurfaceMat;

            junction.SetVertices(new List<Vector3> { p1, p2, p3, p4 });
            junction.SetUVs(channel: 0, new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) });
            junction.SetTriangles(new List<int> { t1, t2, t3, t4, t5, t6 }, submesh: 0);

            mf.mesh = junction;

            juncGo.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);


            if (knotIdx > 1)
            {
                Vector3 flowDirection = splineChanged.Knots.ElementAt(knotIdx).Position - splineChanged.Knots.ElementAt(knotIdx - 1).Position;
                flowDirection.Normalize();
                Vector3 knotPos = splineChanged.ElementAt(knotIdx).Position;
                // Debug.Log($"knotpos: {knotPos}, dir: {flowDirection}");
                Debug.DrawRay(knotPos, flowDirection, Color.red, 10f);

                // I have no idea why i need to use vec2(-z, -x) components like that but that how it works ig
                // times 0.1f because it makes it look weird otherwise
                // i hate this code...
                juncmr.material.SetVector("_WaveDirection", new Vector2(-flowDirection.z, -flowDirection.x * 0.1f));

                Vector3 meshBounds = mf.mesh.bounds.size;
                float surfaceSize = Mathf.Max(meshBounds.x, meshBounds.z);

                juncmr.material.SetFloat("_SurfaceSize", surfaceSize);

                // Collider bounderies
                GameObject juncCol1 = new GameObject($"Water junction collider {knotIdx} (1)");
                juncCol1.transform.SetParent(waterParent);
                juncCol1.tag = "Obstacle";
                GameObject juncCol2 = new GameObject($"Water junction collider {knotIdx} (2)");
                juncCol2.transform.SetParent(waterParent);
                juncCol2.tag = "Obstacle";

                MeshFilter juncCol1Mf = juncCol1.AddComponent<MeshFilter>();
                MeshFilter juncCol2Mf = juncCol2.AddComponent<MeshFilter>();

                Mesh juncCol1Mesh = new();
                juncCol1Mesh.name = "Water junction col 1";
                Mesh juncCol2Mesh = new();
                juncCol2Mesh.name = "Water junction col 2";

                juncCol1Mesh.SetVertices(new List<Vector3> { p1, new Vector3(p1.x, p1.y + colHeight, p1.z), p3, new Vector3(p3.x, p3.y + colHeight, p3.z)});
                juncCol1Mesh.SetTriangles(new List<int> { t1, t2, t3, t4, t5, t6 }, submesh: 0);
                juncCol1Mf.mesh = juncCol1Mesh;

                juncCol1.AddComponent<MeshCollider>();

                juncCol2Mesh.SetVertices(new List<Vector3> { p2, new Vector3(p2.x, p2.y + colHeight, p2.z), p4, new Vector3(p4.x, p4.y + colHeight, p4.z) });
                juncCol2Mesh.SetTriangles(new List<int> { t1, t2, t3, t4, t5, t6 }, submesh: 0);
                juncCol2Mf.mesh = juncCol2Mesh;

                juncCol2.AddComponent<MeshCollider>();

                GameObject juncCol11 = new GameObject($"Water junction collider {knotIdx} (1), redouble");
                juncCol11.transform.SetParent(waterParent);
                juncCol11.tag = "Obstacle";
                GameObject juncCol22 = new GameObject($"Water junction collider {knotIdx} (2), redouble");
                juncCol22.transform.SetParent(waterParent);
                juncCol22.tag = "Obstacle";

                MeshFilter juncCol1Mf1 = juncCol11.AddComponent<MeshFilter>();
                MeshFilter juncCol2Mf2 = juncCol22.AddComponent<MeshFilter>();

                Mesh juncCol1Mesh1 = new();
                juncCol1Mesh.name = "Water junction col 1, redouble";
                Mesh juncCol2Mesh2 = new();
                juncCol2Mesh.name = "Water junction col 2, redouble";

                juncCol1Mesh1.SetVertices(juncCol1Mf.mesh.vertices);
                juncCol2Mesh2.SetVertices(juncCol2Mf.mesh.vertices);
                juncCol1Mf1.mesh = juncCol1Mesh1;
                juncCol2Mf2.mesh = juncCol2Mesh2;

                juncCol1Mf1.mesh.SetIndices(juncCol1Mf.mesh.GetIndices(0).Reverse().ToArray(), MeshTopology.Triangles, submesh: 0);
                juncCol2Mf2.mesh.SetIndices(juncCol2Mf.mesh.GetIndices(0).Reverse().ToArray(), MeshTopology.Triangles, submesh: 0);

                var jucCol11Col = juncCol11.AddComponent<MeshCollider>();
                var juncCol22Col = juncCol22.AddComponent<MeshCollider>();

                jucCol11Col.material = sideWallMat;
                juncCol22Col.material = sideWallMat;

                juncCol1.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);
                juncCol11.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);
                juncCol2.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);
                juncCol22.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);

                // debug
                if (debug)
                {
                    var jjj = juncCol1.AddComponent<MeshRenderer>();
                    var jjj1 = juncCol2.AddComponent<MeshRenderer>();
                    jjj.material = colMat;
                    jjj1.material = colMat;
                }
            }
        }


        GameObject col1 = new GameObject($"Water collider {knotIdx} (1)");
        col1.transform.SetParent(waterParent);
        col1.tag = "Obstacle";
        GameObject col2 = new GameObject($"Water collider {knotIdx} (2)");
        col2.transform.SetParent(waterParent);
        col2.tag = "Obstacle";

        MeshFilter col1Mf = col1.AddComponent<MeshFilter>();
        MeshFilter col2Mf = col2.AddComponent<MeshFilter>();

        Mesh col1Mesh = new();
        col1Mesh.name = "Water col 1";
        Mesh col2Mesh = new();
        col2Mesh.name = "Water col 2";

        List<Vector3> col1Verts = new();
        List<int> col1Tris = new();

        List<Vector3> col2Verts = new();
        List<int> col2Tris = new();

        // Generate water surface mesh
        float uvOffset = 0f;
        for (int i = 1; i < length; i++)
        {
            Vector3 p1 = surfaceVertsP1[i - 1];
            Vector3 p2 = surfaceVertsP2[i - 1];
            Vector3 p3, p4;

            p3 = surfaceVertsP1[i];
            p4 = surfaceVertsP2[i];

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

            // Collider bounderies
            if (knotIdx < 1) continue;
            col1Verts.AddRange(new List<Vector3> { p1, new Vector3(p1.x, p1.y + colHeight, p1.z), p3, new Vector3(p3.x, p3.y + colHeight, p3.z)});
            col1Mf.mesh.SetIndices(col1Mf.mesh.GetIndices(0).Concat(col1Mf.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, submesh: 0);
            col2Mf.mesh.SetIndices(col2Mf.mesh.GetIndices(0).Concat(col2Mf.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, submesh: 0);
            col1Tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });

            col2Verts.AddRange(new List<Vector3> { p2, new Vector3(p2.x, p2.y + colHeight, p2.z), p4, new Vector3(p4.x, p4.y + colHeight, p4.z) });
            col2Tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });

        }

        prevWaterSurface = go;

        go.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);
        col1.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);
        col2.AddComponent<DestroyOnDist>().Init(Boat.Instance.transform, destroyDist, checkMesh: true);

        waterSurface.SetVertices(verts);
        waterSurface.SetUVs(channel: 0, uvs);
        waterSurface.SetTriangles(tris, submesh: 0);
        outputMeshFilter.mesh = waterSurface;

        if (knotIdx > 1)
        {
            Vector3 flowDirection = splineChanged.Knots.ElementAt(knotIdx).Position - splineChanged.Knots.ElementAt(knotIdx - 1).Position;
            flowDirection.Normalize();
            Vector3 knotPos = splineChanged.ElementAt(knotIdx).Position;
            // Debug.Log($"knotpos: {knotPos}, dir: {flowDirection}");
            Debug.DrawRay(knotPos, flowDirection, Color.red, 10f);

            // I have no idea why i need to use vec2(-z, -x) components like that but that how it works ig
            // times 0.1f because it makes it look weird otherwise
            // i hate this code...
            mr.material.SetVector("_WaveDirection", new Vector2(-flowDirection.z, -flowDirection.x * 0.1f));

            Vector3 meshBounds = outputMeshFilter.mesh.bounds.size;
            float surfaceSize = Mathf.Max(meshBounds.x, meshBounds.z);

            mr.material.SetFloat("_SurfaceSize", surfaceSize);

            // collider shit
            col1Mesh.SetVertices(col1Verts);
            col2Mesh.SetVertices(col2Verts);

            col1Mesh.SetTriangles(col1Tris, 0);
            col2Mesh.SetTriangles(col2Tris, 0);

            col1Mf.mesh = col1Mesh;
            col2Mf.mesh = col2Mesh;

            col1Mf.mesh.SetIndices(col1Mf.mesh.GetIndices(0).Concat(col1Mf.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, submesh: 0);
            col2Mf.mesh.SetIndices(col2Mf.mesh.GetIndices(0).Concat(col2Mf.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, submesh: 0);

            var col1Col = col1.AddComponent<MeshCollider>();
            var col2Col = col2.AddComponent<MeshCollider>();

            col1Col.material = sideWallMat;
            col2Col.material = sideWallMat;

            // debug
            if (debug)
            {
                var col1Mr = col1.AddComponent<MeshRenderer>();
                var col2Mr = col2.AddComponent<MeshRenderer>();
                col1Mr.material = colMat;
                col2Mr.material = colMat;
            }
        }
    }
}
