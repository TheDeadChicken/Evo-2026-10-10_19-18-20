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

    public void ExtrudeRandomTriangle(List<int> triangles, List<Vector3> points, float length) {
        int randomPoint = Random.Range(0, triangles.Count);
        int randomStartPoint = randomPoint - (randomPoint % 3);

        int indexZero = triangles[randomStartPoint];
        int indexOne = triangles[randomStartPoint + 1];
        int indexTwo = triangles[randomStartPoint + 2];

        Vector3 p0 = points[indexZero];
        Vector3 p1 = points[indexOne];
        Vector3 p2 = points[indexTwo];

        Vector3 normal = Vector3.Cross(p1-p0, p2-p0).normalized;

        Vector3 p3 = p0 + normal * length;
        Vector3 p4 = p1 + normal * length;
        Vector3 p5 = p2 + normal * length;

        points.Add(p3);
        points.Add(p4);
        points.Add(p5);

        int indexThree = points.Count - 3;
        int indexFour = points.Count - 2;
        int indexFive = points.Count - 1;

        triangles.Add(indexZero);
        triangles.Add(indexOne);
        triangles.Add(indexThree);

        triangles.Add(indexOne);
        triangles.Add(indexFour);
        triangles.Add(indexThree);

        triangles.Add(indexOne);
        triangles.Add(indexTwo);
        triangles.Add(indexFour);

        triangles.Add(indexTwo);
        triangles.Add(indexFive);
        triangles.Add(indexFour);

        triangles.Add(indexTwo);
        triangles.Add(indexZero);
        triangles.Add(indexThree);

        triangles.Add(indexTwo);
        triangles.Add(indexThree);
        triangles.Add(indexFive);

        triangles[randomStartPoint] = indexThree;
        triangles[randomStartPoint + 1] = indexFour;
        triangles[randomStartPoint + 2] = indexFive;
        
        Debug.Log("Extruded triangle at indices: " + indexZero + ", " + indexOne + ", " + indexTwo);
    }

    public void MoveRandomPoint(List<Vector3> points, float offset ) {
        points[Random.Range(0, points.Count)] += new Vector3(Random.Range(-offset, offset), Random.Range(-offset, offset), Random.Range(-offset, offset));
    }


    public List<Vector3> CreatePlantLayer(int amountOfPoints, float radius, Vector3 startingPosition, float rotationalOffset, bool endLayer) {
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

    public List<Vector3> CreateStalk(int amountOfLayers, float spaceBetweenLayers, int pointsPerLayer, float layerRadius, Vector3 startingPosition, float rotationalOffset) {
        List<Vector3> stalkPoints = new List<Vector3>();

        for (int i = 0; i < amountOfLayers; i++) {
            float offset = rotationalOffset + (Mathf.PI / pointsPerLayer) * (i % 2);
            Vector3 layerPos = startingPosition + Vector3.up * (i * spaceBetweenLayers);
            stalkPoints.AddRange(CreatePlantLayer(pointsPerLayer, layerRadius, layerPos, offset, (i == 0 || i == (amountOfLayers - 1))));
        }
        return stalkPoints;
    }

    public void MergeClosePoints(List<int> triangles, List<Vector3> points, float range) {
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

    // public float DistanceBetweenPoints(Vector3 pos1, Vector3 pos2) {
    //     return (Mathf.Sqrt(
    //         (pos2.x-pos1.x)*(pos2.x-pos1.x) +
    //         (pos2.y-pos1.y)*(pos2.y-pos1.y) +
    //         (pos2.z-pos1.z)*(pos2.z-pos1.z)
    //     ));
    // }
}