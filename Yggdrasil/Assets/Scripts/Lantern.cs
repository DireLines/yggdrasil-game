using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lantern : MonoBehaviour {
    public float activeRadius = 200f;
    Dictionary<int, (GameObject, bool)> currentColliders;
    List<int> ids;
    List<int> keysToRemove;

    void Start() {
        currentColliders = new Dictionary<int, (GameObject, bool)>();
        ids = new List<int>();
        keysToRemove = new List<int>();
    }
    void Update() {
        ids.Clear();
        foreach (int id in currentColliders.Keys) {
            ids.Add(id);
        }
        foreach (int id in ids) {
            currentColliders[id] = (currentColliders[id].Item1, false);
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, activeRadius, 1 << LayerMask.NameToLayer("Tree"));
        foreach (Collider collider in colliders) {
            int id = collider.gameObject.GetInstanceID();
            if (!currentColliders.ContainsKey(id)) {
                foreach(ActivatedObject activatedObject in collider.gameObject.GetComponents<ActivatedObject>()) {
                    activatedObject.SetActivated(true);
                }
            }
            currentColliders[id] = (collider.gameObject, true);
        }
        keysToRemove.Clear();
        foreach (int id in currentColliders.Keys) {
            var v = currentColliders[id];
            if (v.Item2 == false) {
                keysToRemove.Add(id);
            }
        }
        foreach (int id in keysToRemove) {
            var v = currentColliders[id];
            foreach(ActivatedObject activatedObject in v.Item1.GetComponents<ActivatedObject>()) {
                activatedObject.SetActivated(false);
            }
            currentColliders.Remove(id);
        }
    }
}
