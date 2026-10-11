using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HexPlacer : MonoBehaviour {

    public static HexPlacer instance;

    [SerializeField] private Main main;
    [SerializeField] private Tiles placedObject;

    private float yHeight = 0; 

    public Dictionary<Vector3, GameObject> tileLocations {get; private set;} = new Dictionary<Vector3, GameObject>(); 

    private GameObject newObject = null;
    

    private bool isErase = true;


    private void OnEnable() {
        if (instance == null) {
            instance = this;
        }
    }

    void Update() {
        if (newObject != null) {
            Destroy(newObject);
        }
        
        if(main.inEditMode) {
            Destroy(newObject);

            Vector3 objectPosition = new Vector3();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, yHeight, 0));
            float distance;
            float interTileXDistence = (placedObject.tileHologram.transform.localScale.x/100);

            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);


                

                objectPosition.z = NearestPoint(worldPosition.z, 1.5f * interTileXDistence, 0f);
                objectPosition.x = NearestPoint(worldPosition.x, Mathf.Sqrt(3) * interTileXDistence, Mathf.Sqrt(3)/2 * (Mathf.Round(worldPosition.z/(interTileXDistence*1.5f)) % 2) * interTileXDistence);
                objectPosition.y = yHeight - 0.3f*interTileXDistence;

            }

            newObject = Instantiate(placedObject.tileHologram, objectPosition, Quaternion.identity); 



            //this.gameObject.SetActive(main.inEditMode);
            if (main.inEditMode) {
                
                if(Mouse.current.leftButton.wasPressedThisFrame) {
                    isErase = tileLocations.ContainsKey(objectPosition);
                    Debug.Log(isErase);
                    
                }
                //if(isErase == null) {
                    //Debug.LogError("Oh No, the erase mode is not set");
                //}
                if(Mouse.current.leftButton.isPressed) {
                        if(!isErase) {
                            if (!tileLocations.ContainsKey(objectPosition)) {
                                if (!tileLocations.ContainsKey(objectPosition)) {
                                    AddTile(objectPosition);
                                };
                            }
                        } else {
                            if(tileLocations.ContainsKey(objectPosition)) {

                                Destroy(tileLocations[objectPosition]);
                                tileLocations.Remove(objectPosition);
                            }
                        } 
                
                    }

                //Debug.Log(Mouse.current.leftButton.wasPressedThisFrame);
    }}}

    public float NearestPoint(float currentPos, float distanceBetweenPoints, float offset) {
        return (Mathf.Round(
            (currentPos - offset) /
            (distanceBetweenPoints)) * 
            (distanceBetweenPoints)) +
            (offset); 
    }

    public void ClearTiles() {
    foreach (var tile in tileLocations.Values) {
        if (tile != null) Destroy(tile);
    }
    tileLocations.Clear();
}

    public GameObject AddTile(Vector3 position) {
        if (tileLocations.TryGetValue(position, out GameObject existing)) return existing;
        GameObject newTile = Instantiate(placedObject.tile, position, Quaternion.identity);
        tileLocations.Add(newTile.transform.position, newTile);
        return newTile;
    }
}
