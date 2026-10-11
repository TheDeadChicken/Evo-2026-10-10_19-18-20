using UnityEngine;

public class HexScript : MonoBehaviour {

    // Pos stands for Posititon

    [SerializeField] private GameObject wall;

    private void Start() {
        Main.instance.OnExitedEditMode += SetUpWalls;
    }

    private void OnDestroy() {
        Main.instance.OnExitedEditMode -= SetUpWalls;
    }

    private void SetUpWalls() {
        float distanceBetweenTiles = Mathf.Sqrt(3) * transform.localScale.x/100; // Looks at the flat side to flat side only
        for (int i=0; i<6; i++) {

            Vector3 offsetPos = (new Vector3(Mathf.Cos(((60)*i) * Mathf.Deg2Rad) * distanceBetweenTiles, 0, -Mathf.Sin(((60)*i) * Mathf.Deg2Rad) * distanceBetweenTiles));
            if (!ContainsTileAtPosition(RoundVector3(transform.position + offsetPos, 2))) {
                Quaternion rotation = Quaternion.Euler(0, (i*60) + 180, 0);
                Instantiate(wall, transform.position + offsetPos, rotation);
            }
        }
    }


    private Vector3 RoundVector3(Vector3 myVector, int decimals){
        float multiplyer = Mathf.Pow(10, decimals);
        return new Vector3((Mathf.Round(myVector.x * multiplyer)/multiplyer), (Mathf.Round(myVector.y * multiplyer)/multiplyer), (Mathf.Round(myVector.z * multiplyer)/multiplyer));
    }

    private bool ContainsTileAtPosition(Vector3 position) {
        foreach (Vector3 key in HexPlacer.instance.tileLocations.Keys) {
            if (RoundVector3(key, 2) == position) {
                return true; 
            }
        }
        return false; 
    }

}
