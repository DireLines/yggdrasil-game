using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCubes : MonoBehaviour {
    public GameObject Platform;
    public GameObject Empty;
    public List<Color> colors = new List<Color>(9);
    string[] realmNames = {
        "asgard",
        "hel",
        "niflheim",
        "jotunheim",
        "svartalfheim",
        "vanaheim",
        "alfheim",
        "muspelheim",
        "midgard",
    };
    Vector3[] realmPositionsSpherical = {
        new Vector3(1, 0, 90),//asgard
        new Vector3(1, 0, -90),//hel
        new Vector3(1, 0, -85),//niflheim
        new Vector3(1, 20, 0),//jotunheim
        new Vector3(1, -10, -30),//svartalfheim
        new Vector3(1, -180, 60),//vanaheim
        new Vector3(1, -110, 45),//alfheim
        new Vector3(1, 125, -30),//muspelheim
        new Vector3(0, 0, 0),//midgard
    };
    List<Vector3> realmPositions;
    // Start is called before the first frame update
    void Start() {
        realmPositions = new List<Vector3>();
        // spawnTree(transform.position + transform.up * 1000, Quaternion.identity, 0);
        // spawnTree(transform.position + transform.up * 2000, Quaternion.identity, 0);

        for (int i = 0; i < realmPositionsSpherical.Length; i++) {
            Vector3 v = realmPositionsSpherical[i];
            Vector3 vi = sphericalToCartesian(5000f, Mathf.Deg2Rad * v.y, Mathf.Deg2Rad * v.z);
            realmPositions.Add(vi);
            GameObject realm = spawnPlatform(vi, Quaternion.identity);
            realm.transform.localScale *= 500f;
            realm.transform.up = -vi;
            foreach (MeshRenderer meshRenderer in realm.GetComponentsInChildren<MeshRenderer>()) {
                meshRenderer.material.color = colors[i];
            }
        }
        spawnTree(transform.position, Quaternion.identity, 0);
        print(transform.childCount + " leaves");
    }

    Vector3 sphericalToCartesian(float radius, float polar, float elevation) {
        float a = radius * Mathf.Cos(elevation);
        return new Vector3(a * Mathf.Cos(polar), radius * Mathf.Sin(elevation), a * Mathf.Sin(polar));
    }
    Vector3 getDownDirectionForRealm(int realmId, Vector3 position) {
        if (realmNames[realmId] == "midgard") {
            return -position;
        }
        return realmPositions[realmId];
    }

    GameObject spawnPlatform(Vector3 position, Quaternion orientation) {
        GameObject platform = Instantiate(Platform, position, orientation, transform);
        return platform;
    }

    float avgNumBranches = 4f;
    GameObject spawnTree(Vector3 position, Quaternion orientation, int depth) {
        float trunkLength = 1400f * Mathf.Pow(2f, -depth) * Random.Range(0.95f, 1.05f);
        GameObject trunk = Instantiate(Empty, position, orientation, transform);
        Vector3 trunkStart = trunk.transform.position;
        Vector3 trunkEnd = trunkStart + trunk.transform.up * trunkLength;
        trunk.transform.localScale = new Vector3(trunkLength / 50f, trunkLength / 2, trunkLength / 50f);
        trunk.transform.position += trunk.transform.up * trunkLength / 2;
        if (depth > 5) {
            GameObject leaf = spawnPlatform(position, orientation);
            leaf.transform.position += leaf.transform.up * trunkLength;
            int realmId = Random.Range(0, realmPositionsSpherical.Length);
            Color color = colors[Random.Range(0, colors.Count)];
            if (realmId >= 0) {
                color = colors[realmId];
                leaf.transform.up = -getDownDirectionForRealm(realmId, leaf.transform.position);
            }
            foreach (MeshRenderer meshRenderer in leaf.GetComponentsInChildren<MeshRenderer>()) {
                meshRenderer.material.color = color;
            }
            return leaf;
        }
        int numBranches = Mathf.RoundToInt(Random.Range(avgNumBranches - 2, avgNumBranches + 2));
        for (int i = 0; i < numBranches; i++) {
            Vector3 newBranchPos = Vector3.Lerp(trunkStart, trunkEnd, Random.Range(0.7f, 1));
            Quaternion newBranchOrientation = orientation * Quaternion.AngleAxis(Random.Range(15, 60), Vector3.ProjectOnPlane(Random.onUnitSphere, trunk.transform.up));
            int newDepth = depth + 1;
            if (Random.Range(0f, 1f) < 0.025f) {
                newDepth = depth;
            }
            GameObject branch = spawnTree(newBranchPos, newBranchOrientation, newDepth);
            // if (!branch.GetComponent<TestScript>()) {
            //     Destroy(branch);
            // }
        }
        return trunk;
    }


}
