using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(CarveShape))]
public class LeafPlatform : ActivatedObject {
    public Vector3 sourcePos;
    public float sourceRadius;
    CarveShape carveShape;
    bool everActivated;
    // Start is called before the first frame update
    void Start() {
        carveShape = GetComponent<CarveShape>();
    }

    public override void SetVisible(bool visible) {
        base.SetVisible(visible);
    }
    public override void SetActivated(bool activated) {
        base.SetActivated(activated);
        if (everActivated) {
            foreach(Transform t in transform) {
                SetComponentsActive(t.gameObject,activated);
            }
            return;
        }
        if(activated) {
            everActivated = true;
            MakePlatformAsync();
        }
    }

    async void MakePlatformAsync() {
        List<GameObject> platformObjects = await carveShape.MakePlatform(sourcePos, sourceRadius, transform.position);
        foreach (GameObject obj in platformObjects) {
            obj.transform.SetParent(transform);
        }
    }

    void SetComponentsActive(GameObject obj, bool active) {
        foreach (MonoBehaviour component in obj.GetComponents<MonoBehaviour>()) {
            component.enabled = active;
        }
        foreach (MeshRenderer mr in obj.GetComponents<MeshRenderer>()) {
            mr.enabled = active;
        }
        foreach (Collider c in obj.GetComponents<Collider>()) {
            c.enabled = active;
        }
    }

}
