using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evo : MonoBehaviour {

    [SerializeField] private Color lightBlue = new Color(0.3f, 0.7f, 0.2f);
    [SerializeField] private float energyLostPerSecond;
    [SerializeField] private float energyForCreation;
    [SerializeField] private float energyGainedFromEatingPlants;
    [SerializeField] private LayerMask sightMask;

    //[SerializeField] private Color red = Color.Red;

    private Dictionary<string, List<int>> traits = new Dictionary<string, List<int>>();
    private static Collider[] overlapBuffer = new Collider[50]; // reused every call, no per-tick allocation

    
                
    public float energy;
    public float time;
    public float mass;
    public int gen = 0;
    public int randNum;
    public GameObject evoParent;

    private float metabolismSpeed;
    private Rigidbody rb;
    private Renderer evoRenderer;

    private List<GameObject> family = new List<GameObject>();

    
    private List<int> brain;
    // private List<string> key = new List<string>{"Movement","Mass","Consumption","Brain","Energy","Sight"};
    
    //{0, 0, 0, 0, 0, 0, 0, 0, 0};//

    private void OnEnable() {
        SimulationStepper.RegisterEvo(this);
    }       

    private void OnDisable() {
        SimulationStepper.UnregisterEvo(this);
    }

    public void Awake() {
        traits["Movement"] = new List<int> {};
        traits["MovementIndexing"] = new List<int> {5};

        traits["Brain"] = new List<int> {}; 

        traits["Consumption"] = new List<int> {0, 2};
        traits["Energy"] = new List<int> {100, 85, 70};    

        traits["Mass"] = new List<int> {5};           
        traits["Sight"] = new List<int> {10}; 
        traits["Metabolism"] = new List<int> {10}; 

        
        //LogTraits();
        for(int i = 0; i < 20; i++) {
            traits["Movement"].Add(RandomMoveTrait());
        }
        for(int i = 0; i < 80; i++) {
            traits["Brain"].Add(Random.Range(-9, 10));
        }
    }  

    private void Start() {
        ApplyTraits();
    } 

    public void ApplyTraits() {
        brain = traits["Brain"];
        energy = traits["Energy"][0] - 30;

        if ( 1 > traits["Mass"][0] || traits["Mass"][0] > 10) { 
            Destroy(this);
        }
        mass = traits["Mass"][0];
        


        transform.localScale = new Vector3(mass, mass, mass);

        rb = GetComponent<Rigidbody>();
        evoRenderer = GetComponent<Renderer>();

        if(traits["Consumption"][0] == 1) {
            evoRenderer.material.color = Color.red;
        } else {
            if(traits["Consumption"][0] == 0) {
                evoRenderer.material.color = lightBlue;
            }
        }

        traits["Energy"][1] = Mathf.Clamp(traits["Energy"][1], 0, 99);

        if(Spawner.instance.gen < gen) {
            Spawner.instance.gen = gen;
        }
    }
    // private void Update() {
    //     for (int i=0; i<Main.instance.speedFactor; i++) {

            
    //     }
        
    // }

    public void SimTick() {
        if (traits["Metabolism"][0] <= 0) {
            Destroy(this.transform.gameObject);
        } 
        metabolismSpeed = (traits["Metabolism"][0] / 10f);

        randNum = Random.Range(0, 125000);

        Move(Brain(See()));  
        time += Time.fixedDeltaTime;

        Live();
        Reproduce();
        Evolve();
        
        Color color = this.GetComponent<Renderer>().material.color;
        color.a= traits["Energy"][0] / energy;
        this.GetComponent<Renderer>().material.color = color;

    }



    private void OnCollisionEnter(Collision collision) {
        Eat(collision);
    }

    private void Eat(Collision collision) {
        Evo collisionEvo = collision.gameObject.GetComponent<Evo>();
        // check if hit animal
        if (collisionEvo != null) {
            // check if eats animals
            if (traits["Consumption"][0] == 1) {
                // check if can larger then animal
                if(collisionEvo.mass <= mass) {
                    // check if famaily of the animal
                    if(!family.Contains(collision.gameObject)) {
                        
                        if(collisionEvo.mass == mass) {
                            if (collisionEvo.randNum > randNum) {
                                return;
                            }
                        }

                        float tempEnergyChange = collision.gameObject.GetComponent<Evo>().energy;
                        Destroy(collision.gameObject);

                        energy += (tempEnergyChange * (traits["Energy"][1]/100));

                    }
                }
            }
        } else {
            if (traits["Consumption"][0] == 0) {
                if (collision.gameObject.GetComponent<Food>() != null) {
                    energy += energyGainedFromEatingPlants;
                    Destroy(collision.gameObject);
                }
            }
        }
    }

    private void Move(int input) {
        int start = Wrap(input, 0, traits["Movement"].Count);
        for (int i = start; i < start + traits["MovementIndexing"][0]; i++) {

            //Break if out of range 
            if (i > traits["Movement"].Count - 1) {
                break;
            }



            if (traits["Movement"][i] > 0) {
                rb.MovePosition(rb.position + (transform.forward * (10 - mass) * Time.fixedDeltaTime * traits["Movement"][i] * 0.5f) * metabolismSpeed);
            } else {
                if (traits["Movement"][i] < 0) {
                    float turn;

                    if (traits["Movement"][i] < -9) {
                        turn = (traits["Movement"][i] + 10) * 5f * metabolismSpeed;
                        
                    } else {
                        turn = -1 * traits["Movement"][i] * 5f * metabolismSpeed;
                    }

                    Quaternion rotation = Quaternion.Euler(0, turn, 0);
                    rb.MoveRotation(rb.rotation * rotation);

                } else if (traits["Movement"][i] == 0) {
                    // I should do somthing cool here!
                }
            }
        }
    }
    
    private int Brain(List<List<int>> input) {
        int output = 0;
        foreach(var list in input) {
            int inputIndex = 0;
            int brainIndex = 0;
            int safty = 0;
            while (inputIndex != list.Count - 1) {
                safty ++;
                
                brainIndex += list[inputIndex];
                brainIndex = Wrap(brainIndex, 0, traits["Brain"].Count);


                output += traits["Brain"][brainIndex];
                
                inputIndex += (int)Mathf.Sign(traits["Brain"][brainIndex]);                
                inputIndex = Wrap(inputIndex, 0, list.Count);
                

                if (safty == 20) {
                    break;
                }

                
            }
        }
        return Mathf.Abs(output);

    }

    private void Live() {
        energy -= Time.fixedDeltaTime * energyLostPerSecond * metabolismSpeed;
        if (energy < 0.01) {
            Destroy(this.gameObject);
        }
        if (energy>traits["Energy"][0]) {
            energy = traits["Energy"][0];
        }
    }



    private List<List<int>> See() {

        List<List<int>> seeing = new List<List<int>>();

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, traits["Sight"][0], overlapBuffer, sightMask);

        for (int i = -2; i < 3; i++) {
            float angle = 15 * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            Collider best = null;
            float bestAngleDiff = 7.5f; // half the 15-degree spacing between rays — mimics each ray's "lane"
            float bestDist = 0f;

            for (int j = 0; j < hitCount; j++) {
                Collider col = overlapBuffer[j];
                if (col == null || col.gameObject == this.gameObject) continue;

                Vector3 closestPoint = col.ClosestPoint(transform.position);
                Vector3 toTarget = closestPoint - transform.position;
                float distToTarget = toTarget.magnitude;
                if (distToTarget < 0.0001f) continue;

                float angleDiff = Vector3.Angle(dir, toTarget);
                if (angleDiff < bestAngleDiff) {
                    bestAngleDiff = angleDiff;
                    best = col;
                    bestDist = distToTarget;
                }
            }   

            if (best != null) {
                Evo hitEvo = best.GetComponent<Evo>();
                if (hitEvo != null) {
                    seeing.Add(new List<int> {hitEvo.traits["Consumption"][0], i, (int)bestDist, traits["Consumption"][0], (int)energy});
                } else if (best.GetComponent<Food>() != null) {
                    seeing.Add(new List<int> {2, i, (int)bestDist, traits["Consumption"][0], (int)energy});
                } else {
                    seeing.Add(new List<int> {3, i, (int)bestDist, traits["Consumption"][0], (int)energy});
                }
            } else {
                seeing.Add(new List<int> {4, i, 0, traits["Consumption"][0], (int)energy});
            }
        }

        return seeing;
    }

    private void Reproduce() {
        GameObject copy;
        Evo copyEvo;
        if (energy > traits["Energy"][2]) {

            copy = Instantiate(this.gameObject, evoParent.transform);
            copyEvo = copy.GetComponent<Evo>();

            copyEvo.traits = new Dictionary<string, List<int>>();


            foreach (var trait in traits) {
                copyEvo.traits[trait.Key] = new List<int>(trait.Value);
            }
            copyEvo.ApplyTraits();

            copyEvo.family = new List<GameObject>(); 

            if (traits["Consumption"][0] == 1) {
                for (int i = 0; i<family.Count; i++) {
                    copyEvo.family.Add(family[i]);
                }

                copyEvo.family.Insert(0, this.gameObject);
                while(copyEvo.family.Count > copyEvo.traits["Consumption"][1]) {
                    copyEvo.family.RemoveAt(copyEvo.family.Count - 1);
                }

                family.Insert(0, copy);
                while(family.Count > traits["Consumption"][1]) {
                    family.RemoveAt(family.Count - 1);
                }


            }

            copyEvo.energy = traits["Energy"][2] - energyForCreation; 

            copyEvo.gen = gen + 1;

            energy -= traits["Energy"][2];
        }
    }

    private void Evolve() {
        if (Random.Range(0, 125000) == 0) {
            string trait = null;
            int key = Random.Range(0,13);

            if(key == 0) {
                trait = "MovementIndexing";
            } else if (key < 4) {
                    trait = "Movement";
                } else {    
                    if (key < 8) {
                        trait = "Brain";
                    } else {
                        if (key == 8) {
                            trait = "Energy";
                        }
                        if (key == 9) {
                            trait = "Mass";
                        }
                        if (key == 10) {
                            trait = "Sight";
                        }
                        if (key == 11) {
                            trait = "Consumption";
                        }
                        if (key == 12) {
                            trait = "Metabolism";
                        }
                    }
                }
            
            
            

            if ((trait == "Mass") || (trait == "Energy") || (trait == "Sight") || (trait == "MovementIndexing") || (trait == "Metabolism")) {
                traits[trait][Random.Range(0, traits[trait].Count)] += Random.Range(-1,2);
            }
            if (trait == "Consumption") {
                if (traits[trait][0] == 1) {
                    if (Random.Range(0, 4) == 0) {                        
                        traits[trait][0] = 0;
                        evoRenderer.material.color = Color.lightBlue;
                    } else {
                        traits[trait][1] += Random.Range(-1, 2);
                    }

                } else {
                    traits[trait][0] = 1;
                    evoRenderer.material.color = Color.red;

                }
            }
            if (trait == "Movement") {
                if(Random.Range(0, 6) < 2) {
                    if(Random.Range(0,10) != 0) {
                        traits[trait].Add(RandomMoveTrait());
                    } else {
                        traits[trait].RemoveAt(Random.Range(0, traits[trait].Count));
                    }                           
                } else {
                    traits[trait][Random.Range(0, traits[trait].Count)] = RandomMoveTrait();
                }
            }
            if (trait == "Brain") {
                if(Random.Range(0, 6) < 2) {
                    traits[trait].Add(Random.Range(-9, 10));
                } else {
                    traits[trait][Random.Range(0, traits[trait].Count)] = Random.Range(-9, 10);
                }
            }
        }
    }

    private int RandomMoveTrait() {
        if(Random.Range(0, 10) == 0) {
            if (Random.Range(0, 2) == 0) {
                return Random.Range(-9, 1);
                ;
            } else {
                return Random.Range(-9, 1) - 10;
            }
        } else {
            if(Random.Range(0,2) == 0) {
                return Random.Range(1,3);
            } else if(Random.Range(0, 2) == 0) {
                return Random.Range(3,6);
            } else if(Random.Range(0, 2) == 0) {
                return Random.Range(6,10);
            }

            return 0;
            
        }
    }

    private int Wrap(int value, int min, int max) {
        int output = value;
        while (output < min) {
            output += max;
        }
        output = output % max;
        return output;
    }

    public void LogTraits() {
        foreach (var pair in traits) {
            Debug.Log(pair.Key + " : " + string.Join(",", pair.Value));
        }
    }

    public Dictionary<string, List<int>> GetTraits() {
        return traits;
    }




        // private List<List<int>> See() {
    //     List<List<int>> seeing = new List<List<int>>();

    //     for (int i = -2; i < 3; i++) {
            
    //         float angle = 15 * i;
    //         Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

    //         if (Physics.Raycast(transform.position, dir, out RaycastHit hit, traits["Sight"][0])) {
    //             Evo hitEvo = hit.collider.GetComponent<Evo>();
    //             if (hitEvo != null) {
    //                 seeing.Add(new List<int> {hitEvo.traits["Consumption"][0], i, (int)hit.distance, traits["Consumption"][0], (int)energy});
    //             } else if(hit.collider.GetComponent<Food>() != null) {
    //                 seeing.Add(new List<int> {2, i, (int)hit.distance, traits["Consumption"][0], (int)energy});
    //             } else {
    //                 seeing.Add(new List<int> {3, i, (int)hit.distance, traits["Consumption"][0], (int)energy});
    //             }

    //         } else {
    //             seeing.Add(new List<int> {4, i, 0, traits["Consumption"][0], (int)energy});
    //         }
    //     }

    //     return seeing;

    // }


    // Add this as a class field, near your other private fields:
}