using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class PlantEvo {

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
            MergeClosePoints(mergeScale);
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
        return (layer == 0 || layer = amountOfLayers - 1);
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

    private list<int> ConnectLayers(int amoutOfLayers, int pointsPerLayer) {
        List<int> tris = new List<int>();

        for (int layer = 0; layer < amoutOfLayers - 1; layer++) {
            bool lowerIsOffset = (layer % 2) == 1;
            for (int j = 0; j < pointsPerLayer; j++) {
                int next = (j + 1) % pointsPerLayer;

                int lowerJ = GetRingIndex(layer, j, pointsPerLayer, amoutOfLayers);
                int lowerNext = GetRingIndex(layer, next, pointsPerLayer, amountOfLayers);
                int upperJ = GetRingIndex(layer + 1, j, pointsPerLayer, amountOfLayers);
                int upperNext = GetRingIndex(layer + 1, next, pointsPerLayer, amountOfLayers);

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
        int topLayer = amountOfLayers - 1;

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
    
}
