// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.InputSystem;
// using TMPro;

// public class Selection : MonoBehaviour {
//     [SerializeField] private Camera camera;

//     [SerializeField] private GameObject dropdown;
//     [SerializeField] private GameObject parent;

//     private void Update() {
//         if (Mouse.current.position.ReadValue() != null) {
//             Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
//             if (Physics.Raycast(ray, out RaycastHit hit)) {
//                 Evo hitEvo = hit.collider.GetComponent<Evo>();
//                 if (hitEvo != null && Input.GetMouseButtonDown(0)) {
//                     Debug.Log("Selected!");
//                     CreateUI(hitEvo.GetTraits());
//                 }
//             }
//         }
//     }

//     private void CreateUI(Dictionary<string, List<int>> traits) {
//         foreach (var pair in traits) {
//             GameObject newDropDown = Instantiate(dropdown, Vector3.zero, Quaternion.identity, parent.transform);
//             Transform button = newDropDown.transform.Find("Button");
            
//             button.GetComponentInChildren<TMP_Text>().text = pair.Key;
//         }
//     }
// }
