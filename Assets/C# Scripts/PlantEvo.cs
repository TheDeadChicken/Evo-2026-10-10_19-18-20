    using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class PlantEvo : MonoBehaviour{

    private Mesh mesh;
    private MeshCollider meshCollider;

    private static PlantHelperScripts phs = new PlantHelperScripts();

    [SerializeField] private GameObject energyUnit;

    [SerializeField] private List<Vector3> points = new List<Vector3>();
    [SerializeField] private List<int> triangles = new List<int>();
    [SerializeField] private float scale;
    [SerializeField] private float mergeFraction;
    [SerializeField] private float splitLength;
    [SerializeField] private float extruedLength;
    [SerializeField] private float moveLength; 
    private void Awake() {
        mesh = new Mesh();
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

    }

    private void Start() {
        points.Clear();
        triangles.Clear();

        int layers = Random.Range(5, 10);
        int pointsPerLayer = Random.Range(5, 8);

        points.AddRange(phs.CreateStalk(layers, Random.Range(0.1f, 1f) * scale, pointsPerLayer, Random.Range(0.1f, 0.5f) * scale, Vector3.zero, 0f));
        triangles.AddRange(phs.ConnectLayers(layers, pointsPerLayer));

        UpdateMesh();
    }

    private void Update() {
        if (Evolve(triangles, points, splitLength, extruedLength, moveLength)) {
            phs.MergeClosePoints(triangles, points, scale * mergeFraction);
            UpdateMesh();
        }
    }

    private void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    public void GetEaten() {
        for (int i = 0; i<points.Count; i++) {
            Instantiate(energyUnit, points[i] += transform.position, Quaternion.identity);
        }
        Destroy(this.gameObject);   
    }

   
    private bool Evolve(List<int> triangles, List<Vector3> points, float splitLength, float extruedLength, float moveLength) {
        if (Random.Range(0, 25000) == 0) {
            int action = Random.Range(0, 500);
            if(action < 1) {
                phs.SplitRandomTriangle(triangles, points, Random.Range(0.1f, 1f) * splitLength);
            } else if (action < 3) {
                phs.ExtrudeRandomTriangle(triangles, points, Random.Range(0.1f, 1f) * extruedLength);
            } else {
                phs.MoveRandomPoint(points, moveLength);
            }
            return true;
        } 
        return false;
    }

    
    
}
