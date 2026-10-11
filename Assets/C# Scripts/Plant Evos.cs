using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PlantEvos : MonoBehaviour {
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
                layerPoints.Add(ringPoint);        // ring point used only by cap
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

    /// <summary>
    /// Calculates an outward target point using face centroid and outward normal.
    /// Prevents crisscrossing triangles by extruding away from the surface.
    /// </summary>
    private Vector3 GetOutwardPoint(float centerOffset, Vector3 p0, Vector3 p1, Vector3 p2) {
        Vector3 centroid = (p0 + p1 + p2) / 3f;
        Vector3 faceNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
        
        return centroid + (faceNormal * Random.Range(centerOffset * 0.1f, centerOffset * 0.5f));
    }

    private void SplitRandomTriangle(List<int> triangles, List<Vector3> points, float centerOffset) {
        if (triangles.Count < 3) return;

        int randomPoint = Random.Range(0, triangles.Count);
        int randomStartPoint = randomPoint - (randomPoint % 3);

        int indexZero = triangles[randomStartPoint];
        int indexOne = triangles[randomStartPoint + 1];
        int indexTwo = triangles[randomStartPoint + 2];

        Vector3 p0 = points[indexZero];
        Vector3 p1 = points[indexOne];
        Vector3 p2 = points[indexTwo];

        Vector3 pointThree = GetOutwardPoint(centerOffset, p0, p1, p2);

        int indexThree = points.Count;
        points.Add(pointThree);

        // Replace original triangle with 3 sub-triangles sharing indexThree
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

    private bool Evolve(List<int> triangles, List<Vector3> points, float centerOffset) {
        if (Random.Range(0, 5) == 0) {
            int action = Random.Range(0, 50);
            if (action == 0) {
                CollapseRandomPoint(triangles, points);
            } else if (action < 6) {
                SplitRandomTriangle(triangles, points, centerOffset);
            } else {
                // Thicken structure by pushing a random vertex outward relative to mesh center
                int targetIndex = Random.Range(0, points.Count);
                Vector3 point = points[targetIndex];
                
                // Direction from origin or center Y axis outward
                Vector3 outwardDir = new Vector3(point.x, 0f, point.z).normalized;
                if (outwardDir == Vector3.zero) outwardDir = Vector3.up;

                point += outwardDir * (centerOffset * 0.05f) * Random.Range(-1, 2);
                points[targetIndex] = point; // Assign back to List<Vector3>
            }
            return true;
        }
        return false;
    }

    private int GetLayerStart(int layer, int pointsPerLayer) {
        return (layer == 0) ? 0 : (layer + 2) * pointsPerLayer;
    }

    private bool IsEndLayer(int layer, int amountOfLayers) {
        return (layer == 0 || layer == (amountOfLayers - 1));
    }

    private int GetRingIndex(int layer, int j, int pointsPerLayer, int amountOfLayers) {
        int start = GetLayerStart(layer, pointsPerLayer);
        if (IsEndLayer(layer, amountOfLayers)) {
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

    private List<int> ConnectLayers(int amountOfLayers, int pointsPerLayer) {
        List<int> tris = new List<int>();

        for (int layer = 0; layer < amountOfLayers - 1; layer++) {
            bool lowerIsOffset = (layer % 2) == 1;

            for (int j = 0; j < pointsPerLayer; j++) {
                int next = (j + 1) % pointsPerLayer;

                int lowerJ = GetRingIndex(layer, j, pointsPerLayer, amountOfLayers);
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

    private void MergeClosePoints(float mergeDistance) {
        float sqrThreshold = mergeDistance * mergeDistance;
        int pointCount = points.Count;

        int[] remap = new int[pointCount];
        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < pointCount; i++) {
            Vector3 current = points[i];
            int existingIndex = -1;

            for (int j = 0; j < newPoints.Count; j++) {
                if ((newPoints[j] - current).sqrMagnitude <= sqrThreshold) {
                    existingIndex = j;
                    break;
                }
            }

            if (existingIndex != -1) {
                remap[i] = existingIndex;
            }
            else {
                remap[i] = newPoints.Count;
                newPoints.Add(current);
            }
        }

        List<int> newTriangles = new List<int>();
        for (int i = 0; i < triangles.Count; i += 3) {
            int t0 = remap[triangles[i]];
            int t1 = remap[triangles[i + 1]];
            int t2 = remap[triangles[i + 2]];

            if (t0 != t1 && t1 != t2 && t0 != t2) {
                newTriangles.Add(t0);
                newTriangles.Add(t1);
                newTriangles.Add(t2);
            }
        }

        points = newPoints;
        triangles = newTriangles;
    }

    private void CollapseRandomPoint(List<int> triangles, List<Vector3> points) {
        // Don't collapse if the mesh is getting too small
        if (points.Count <= 10) return;

        int targetIndex = Random.Range(0, points.Count);
        int neighborIndex = -1;

        // 1. Look through the triangles to find a vertex that shares an edge with our target
        for (int i = 0; i < triangles.Count; i += 3)
        {
            if (triangles[i] == targetIndex) neighborIndex = triangles[i + 1];
            else if (triangles[i + 1] == targetIndex) neighborIndex = triangles[i + 2];
            else if (triangles[i + 2] == targetIndex) neighborIndex = triangles[i];

            if (neighborIndex != -1) break; // Found a neighbor, stop looking
        }

        // 2. Snap the target point to the neighbor's exact position
        if (neighborIndex != -1)
        {
            points[targetIndex] = points[neighborIndex];
            
            // Note: We don't even need to delete the triangles here! 
            // Because the two points are now at the exact same position, 
            // your existing MergeClosePoints() method will see they are touching, 
            // merge them into one point, and delete the flattened triangles for you.
        }
    }
}