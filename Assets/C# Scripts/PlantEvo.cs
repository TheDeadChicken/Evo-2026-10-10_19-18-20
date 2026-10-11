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
    [SerializeField] private float mergeScale = 0.2f;

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

    private void MergeClosePoints(float range, List<Vector3> points, List<int> triangles) {
        List<int> closePoints = new List<int>();
        for (int i=0; i<points.Count; i++) {
            for (int j=0; j<points.Count; j++) {
                if(phs.DistanceBetweenPoints(points[i], points[j]) < range) {
                    closePoints.Add(j);
                }
            }

            for (int j=0; j<triangles.Count; j++) {
                for (int q=0; q<closePoints.Count; q++) {
                    if(triangles[j] == closePoints[q]) {
                        triangles[j] = i;
                    }
                }
            }

            closePoints.Clear();
        }
    }

    private bool Evolve(List<int> triangles, List<Vector3> points, float centerOffset) {
        if (Random.Range(0, 50) == 0) {
            int action = Random.Range(0, 50);
            phs.SplitRandomTriangle(triangles, points, 1f);
            return true;
        }
        return false;
    }

    
    
}
