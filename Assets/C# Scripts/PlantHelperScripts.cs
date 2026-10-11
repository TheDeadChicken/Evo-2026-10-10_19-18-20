using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlantHelperScripts {
    
    private bool IsEndLayer(int layer, int amountOfLayers) {
        return layer == 0 || layer == amountOfLayers - 1;
    }

    private int GetLayerStart(int layer, int pointsPerLayer) {
        return (layer == 0) ? 0 : (layer + 1) * pointsPerLayer;
    }   

    private int GetRingIndex(int layer, int j, int pointsPerLayer, int amountOfLayers) {
        int start = GetLayerStart(layer, pointsPerLayer);
        if (IsEndLayer(layer, amountOfLayers)) {
            return start + (j * 2) + 1;
        }
        return start + j;
    }

    private int GetCapRingIndex(int layer, int j, int pointsPerLayer) {
        return GetLayerStart(layer, pointsPerLayer) + (j * 2) + 1;
    }

    private int GetCenterIndex(int layer, int j, int pointsPerLayer) {
        return GetLayerStart(layer, pointsPerLayer) + (j * 2);
    }

    public List<int> ConnectLayers(int amoutOfLayers, int pointsPerLayer) {
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

    public void SplitRandomTriangle(List<int> triangles, List<Vector3> points, float centerOffset) {

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

    public float DistanceBetweenPoints(Vector3 pos1, Vector3 pos2) {
        return (Mathf.Sqrt(
            (pos2.x-pos1.x)*(pos2.x-pos1.x) +
            (pos2.y-pos1.y)*(pos2.y-pos1.y) +
            (pos2.z-pos1.z)*(pos2.z-pos1.z)
        ));
    }
}