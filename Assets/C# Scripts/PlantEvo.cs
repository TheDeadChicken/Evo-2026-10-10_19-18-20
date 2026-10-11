using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class PlantEvo : MonoBehaviour{

    private Mesh mesh;

    [SerializeField] private List<Vector3> points = new List<Vector3>();
    [SerializeField] private List<int> triangles = new List<int>();
    [SerializeField] private float scale = 8f;
    [SerializeField] private float mergeScale = 0.2f;

    private void Awake() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Start() {
        points.Clear();
        triangles.Clear();

        int layers = Random.Range(7, 15);
        int pointsPerLayer = 6;

        points.AddRange(CreateStalk(layers, Random.Range(0.1f, 1f) * scale, pointsPerLayer, Random.Range(0.5f, 3f) * scale, Vector3.zero, 0f));
        triangles.AddRange(ConnectLayers(layers, pointsPerLayer));

        UpdateMesh();
    }

    private void Update() {
        if (Evolve(triangles, points, scale)) {
            //MergeClosePoints(mergeScale);
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
                layerPoints.Add(startingPosition); // cap center
            }
            layerPoints.Add(ringPoint);            // ring point used by side walls
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

    private bool Evolve(List<int> triangles, List<Vector3> points, float centerOffset) {
        if (Random.Range(0, 5) == 0) {
            int action = Random.Range(0, 50);
            return true;
        }
        return false;
    }

    private int GetLayerStart(int layer, int pointsPerLayer) {
        return (layer == 0) ? 0 : (layer + 2) * pointsPerLayer;
    }

    private bool IsEndLayer(int layer, int amountOfLayers) {
        return (layer == 0 || layer == amountOfLayers - 1);
    }

    private int GetRingIndex(int layer, int j, int pointsPerLayer, int amountOfLayers) {
        int start = GetLayerStart(layer, pointsPerLayer);
        if(IsEndLayer(layer, amountOfLayers)) {
            return start + (j * 3) + 2;
        }
        return start + j;
    }
    
    private int GetCapRingIndex(int layer, int j, int pointsPerLayer) {
        return GetLayerStart(layer, pointsPerLayer) + (j * 3) + 1;
    }

    private int GetCenterIndex(int layer, int j, int pointsPerLayer) {
        return GetLayerStart(layer, pointsPerLayer) + (j * 3);
    }

    private List<int> ConnectLayers(int amoutOfLayers, int pointsPerLayer) {
        List<int> tris = new List<int>();

        for (int layer = 0; layer < amoutOfLayers - 1; layer++) {
            bool lowerIsOffset = (layer % 2) == 1;
            for (int j = 0; j < pointsPerLayer; j++) {
                int next = (j + 1) % pointsPerLayer;

                int lowerJ = GetRingIndex(layer, j, pointsPerLayer, amoutOfLayers);
                int lowerNext = GetRingIndex(layer, next, pointsPerLayer, amoutOfLayers);
                int upperJ = GetRingIndex(layer + 1, j, pointsPerLayer, amoutOfLayers);
                int upperNext = GetRingIndex(layer + 1, next, pointsPerLayer, amoutOfLayers);

                if (!lowerIsOffset) {
                    tris.Add(lowerJ);
                    tris.Add(upperJ);
                    tris.Add(lowerNext);

                    tris.Add(lowerNext);
                    tris.Add(upperJ);
                    tris.Add(upperNext);
                }
                else {
                    tris.Add(lowerJ);
                    tris.Add(upperNext);
                    tris.Add(lowerNext);

                    tris.Add(lowerJ);
                    tris.Add(upperJ);
                    tris.Add(upperNext);
                }
            }
        }

        int bottomLayer = 0;
        int topLayer = amoutOfLayers - 1;

        for (int j = 0; j < pointsPerLayer; j++) {
            int next = (j + 1) % pointsPerLayer;

            int bottomCenter = GetCenterIndex(bottomLayer, j, pointsPerLayer);
            int bottomJ = GetCapRingIndex(bottomLayer, j, pointsPerLayer);
            int bottomNext = GetCapRingIndex(bottomLayer, next, pointsPerLayer);

            tris.Add(bottomCenter);
            tris.Add(bottomJ);
            tris.Add(bottomNext);

            int topCenter = GetCenterIndex(topLayer, j, pointsPerLayer);
            int topJ = GetCapRingIndex(topLayer, j, pointsPerLayer);
            int topNext = GetCapRingIndex(topLayer, next, pointsPerLayer);

            tris.Add(topCenter);
            tris.Add(topNext);
            tris.Add(topJ);
        }

        return tris;
    }

    private Vector3 GetCenterPoint(float centerOffset, Vector3 p0, Vector3 p1, Vector3 p2) {
        Vector3 centroid = (p0 + p1 + p2) / 3f;
        Vector3 faceNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
        
        return centroid + (faceNormal * Random.Range(-0.5f,  0.5f) * centerOffset);
    }

    private void SplitRandomTriangle(List<int> triangles, List<Vector3> points, float centerOffset) {

        int randomPoint = Random.Range(0, triangles.Count);
        int randomStartPoint = randomPoint - (randomPoint % 3);

        int indexZero = triangles[randomStartPoint];
        int indexOne = triangles[randomStartPoint + 1];
        int indexTwo = triangles[randomStartPoint + 2];

        Vector3 p0 = points[indexZero];
        Vector3 p1 = points[indexOne];
        Vector3 p2 = points[indexTwo];

        Vector3 pointThree = GetCenterPoint(centerOffset, p0, p1, p2);

        int indexThree = points.Count;
        points.Add(pointThree);

        triangles[randomStartPoint] = indexZero;
        triangles[randomStartPoint + 1] = indexOne;
        triangles[randomStartPoint + 2] = indexThree;

        triangles.Add(indexOne);
        triangles.Add(indexTwo);
        triangles.Add(indexThree);

        triangles.Add(indexTwo);
        triangles.Add(indexZero);
        triangles.Add(indexThree);
    }
    
}
