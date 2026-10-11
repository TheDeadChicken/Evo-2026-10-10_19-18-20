    using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class PlantEvo : MonoBehaviour{

    private Mesh mesh;
    private static PlantHelperScripts phs = new PlantHelperScripts();

    [SerializeField] private List<Vector3> points = new List<Vector3>();
    [SerializeField] private List<int> triangles = new List<int>();
    [SerializeField] private float scale;
    [SerializeField] private float mergeScale;
 
    private void Awake() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Start() {
        points.Clear();
        triangles.Clear();

        int layers = Random.Range(7, 15);
        int pointsPerLayer = Random.Range(6, 15);

        points.AddRange(CreateStalk(layers, Random.Range(0.1f, 1f) * scale, pointsPerLayer, Random.Range(0.5f, 3f) * scale, Vector3.zero, 0f));
        triangles.AddRange(phs.ConnectLayers(layers, pointsPerLayer));

        UpdateMesh();
    }

    private void Update() {
        if (Evolve(triangles, points, scale)) {
            MergeClosePoints(triangles, points, mergeScale);
            UpdateMesh();
        }
    }

    private void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private List<Vector3> CreatePlantLayer(int amountOfPoints, float radius, Vector3 startingPosition, float rotationalOffset, bool endLayer) {
        List<Vector3> layerPoints = new List<Vector3>();

        for (int i = 0; i < amountOfPoints; i++) {
            float angle = ((float)i / amountOfPoints) * 2f * Mathf.PI + rotationalOffset;
            Vector3 ringPoint = new Vector3(
                startingPosition.x + Mathf.Cos(angle) * radius,
                startingPosition.y,
                startingPosition.z + Mathf.Sin(angle) * radius
            );

            if (endLayer) {
                layerPoints.Add(startingPosition); 
            }
            layerPoints.Add(ringPoint);            
        }
        return layerPoints;
    }

    private List<Vector3> CreateStalk(int amountOfLayers, float spaceBetweenLayers, int pointsPerLayer, float layerRadius, Vector3 startingPosition, float rotationalOffset) {
        List<Vector3> stalkPoints = new List<Vector3>();

        for (int i = 0; i < amountOfLayers; i++) {
            float offset = rotationalOffset + (Mathf.PI / pointsPerLayer) * (i % 2);
            Vector3 layerPos = startingPosition + Vector3.up * (i * spaceBetweenLayers);
            stalkPoints.AddRange(CreatePlantLayer(pointsPerLayer, layerRadius, layerPos, offset, (i == 0 || i == (amountOfLayers - 1))));
        }
        return stalkPoints;
    }

    private void MergeClosePoints(List<int> triangles, List<Vector3> points, float range) {
        int count = points.Count;
        float sqrRange = range * range;

        int[] remap = new int[count];
        for (int i = 0; i < count; i++) remap[i] = -1;

        for (int i = 0; i < count; i++) {
            if (remap[i] != -1) continue;   
            remap[i] = i;                   
            for (int j = i + 1; j < count; j++) {
                if (remap[j] == -1 && (points[i] - points[j]).sqrMagnitude < sqrRange) {
                    remap[j] = i;
                }
            }
        }

        int[] newIndex = new int[count];
        List<Vector3> newPoints = new List<Vector3>();
        for (int i = 0; i < count; i++) {
            if (remap[i] == i) {
                newIndex[i] = newPoints.Count;
                newPoints.Add(points[i]);
            }
        }

        List<int> newTris = new List<int>(triangles.Count);
        for (int t = 0; t < triangles.Count; t += 3) {
            int a = newIndex[remap[triangles[t]]];
            int b = newIndex[remap[triangles[t + 1]]];
            int c = newIndex[remap[triangles[t + 2]]];
            if (a == b || b == c || a == c) continue;
            newTris.Add(a);
            newTris.Add(b);
            newTris.Add(c);
        }

        points.Clear();
        points.AddRange(newPoints);
        triangles.Clear();
        triangles.AddRange(newTris);
    }

    private bool Evolve(List<int> triangles, List<Vector3> points, float centerOffset) {
        if (Random.Range(0, 5) == 0) {
            int action = Random.Range(0, 50);
            phs.SplitRandomTriangle(triangles, points, centerOffset);
            return true;
        }
        return false;
    }

    
    
}
