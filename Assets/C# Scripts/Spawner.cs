using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public static Spawner instance;

    [SerializeField] private GameObject foodParent;
    [SerializeField] private GameObject food;

    [SerializeField] private GameObject evoParent;
    [SerializeField] private GameObject evo;


    [SerializeField] private float spawnTime = 0;
    [SerializeField] private float time = 0;


    

    public int gen;
    
    private void Awake(){
        if (instance == null) {
            instance = this;
        }
    }

    private void Start() {
        Main.instance.OnSetupStart += SetupFood;
        Main.instance.OnSetupStart += SetupEvo;
    }

    private void OnDestroy() {
        Main.instance.OnSetupStart -= SetupFood;
        Main.instance.OnSetupStart -= SetupEvo;
    }
    
    // private void Update() {
    //     if(!Main.instance.inEditMode) {
    //         // for (int i=0; i<Main.instance.speedFactor; i++) {
    //         //     time += Time.deltaTime; 

    //         //     spawnTime += Time.deltaTime;
    //         //     if ((Mathf.Round(spawnTime % 2)) == 0) {
    //         //         SpawnFood();
    //         //         spawnTime += 0.5f;
    //         //     }
    //         // }

    //         // for (int i=0; i<HexPlacer.instance.tileLocations.Count/3; i++) {
    //         //     if (Random.Range(0, 5000) == 0) {
    //         //         SpawnFood();
    //         //         Debug.Log("spawned");
    //         //     }
    //         // }
    //     }
    // }

    private void SpawnFood() {
        Instantiate(food, Main.instance.GetRandomMapLocation(), Quaternion.identity, foodParent.transform);   
    }

    private void SetupFood() {
        for (int i=0; i<HexPlacer.instance.tileLocations.Count/10; i++) {
            SpawnFood();
        }
    }

    private void SetupEvo() {

        for (int i=0; i<HexPlacer.instance.tileLocations.Count/11; i++) {
            SpawnEvo();
        }
    }

    private void SpawnEvo() {
        Evo newEvo = Instantiate(evo, Main.instance.GetRandomMapLocation(), Quaternion.identity, evoParent.transform).GetComponent<Evo>();
        newEvo.evoParent = evoParent;
        
    }

    public void SimTick() {
        // for (int i=0; i<HexPlacer.instance.tileLocations.Count/3; i++) {
        //     if (Random.Range(0, 5000) == 0) {
        //         SpawnFood();
        //         Debug.Log("spawned");
        //     }
        // }
    }


}
