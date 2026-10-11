using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    //Constants
        public int speedFactor = 1;


    //Others
    
    public static Main instance;

    public bool inEditMode = true;

    

    //public UnityEvent OnExitedEditMode;

    public event System.Action OnExitedEditMode;
    public event System.Action OnSetupStart;


    private List<Vector3> tiles;

    private void OnEnable() {
        if (instance == null) {
            instance = this;
        }
    }

    private void Start() {
        OnExitedEditMode += ExitedEditMode;
    }

    private void Update() {
        if(Keyboard.current.eKey.wasPressedThisFrame && (inEditMode == true)) {
            OnExitedEditMode?.Invoke();
            inEditMode = false;
        }
    }

    public void ExitedEditMode() {
        tiles = new List<Vector3>(HexPlacer.instance.tileLocations.Keys);
        OnSetupStart?.Invoke();
    }

    public Vector3 GetRandomMapLocation() {
        if(tiles != null) {
            Vector3 spawnLocation = tiles[Random.Range(0, HexPlacer.instance.tileLocations.Count)];

            float sizeFactor = HexPlacer.instance.tileLocations[spawnLocation].transform.localScale.x/100f;
            spawnLocation.x += Random.Range(-0.5f, 0.6f) * sizeFactor;
            spawnLocation.z -= Random.Range(-0.5f, 0.6f) * sizeFactor;

            return new Vector3(spawnLocation.x, 2, spawnLocation.z);
        } else {
            Debug.LogError("oopsy, looking for a map that doesnt exist");
            return new Vector3(100, 100, 100);
        }
    }

}
