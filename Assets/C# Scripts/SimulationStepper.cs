using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStepper : MonoBehaviour {
    private static List<Evo> evos = new List<Evo>();

    public static void RegisterEvo(Evo e) => evos.Add(e);
    public static void UnregisterEvo(Evo e) => evos.Remove(e);

    public static Spawner instance;

    private void Awake() {
        Physics.simulationMode = SimulationMode.Script;
        // if (instance == null) {
        //     instance = this;
        // }
    }



    private float accumulator = 0f;

    private void Update() {
        if (Main.instance.inEditMode) return;

        float dt = Time.fixedDeltaTime;
        accumulator += Time.deltaTime * Main.instance.speedFactor;

        while (accumulator >= dt) {
            var currentEvos = evos.ToArray();
            foreach (var e in currentEvos) {
                if (e != null) e.SimTick();
            }
            Spawner.instance.SimTick();
            Physics.Simulate(dt);
            accumulator -= dt;
        }
    }
}
