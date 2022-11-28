using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;

public class SpawnCubes : MonoBehaviour {
    public GameObject Platform;
    public GameObject Branch;
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
    float[] realmPlatformFrequencies = {
        8f,//asgard
        9f,//hel
        9f,//niflheim
        15f,//jotunheim
        13f,//svartalfheim
        6f,//vanaheim
        5f,//alfheim
        6f,//muspelheim
        15f,//midgard
    };
    List<Vector3> realmPositions;
    Transform branchesContainer;
    Transform leavesContainer;
    // Start is called before the first frame update
    void Start() {
        branchesContainer = transform.Find("Branches");
        leavesContainer = transform.Find("Leaves");
        realmPositions = new List<Vector3>();
        // spawnTree(transform.position + transform.up * 1000, Quaternion.identity, 0);
        // spawnTree(transform.position + transform.up * 2000, Quaternion.identity, 0);

        for (int i = 0; i < realmPositionsSpherical.Length - 1; i++) {
            Vector3 v = realmPositionsSpherical[i];
            Vector3 vi = sphericalToCartesian(5000f, Mathf.Deg2Rad * v.y, Mathf.Deg2Rad * v.z);
            realmPositions.Add(vi);
            GameObject realm = spawnPlatform(vi, Quaternion.identity, i);
            realm.transform.localScale *= 500f;
            realm.transform.up = -vi;
            foreach (MeshRenderer meshRenderer in realm.GetComponentsInChildren<MeshRenderer>()) {
                meshRenderer.material.color = colors[i];
            }
        }
        spawnTree(transform.position, Quaternion.identity, 0);
        print(leavesContainer.childCount + " leaves");
        // Destroy(branchesContainer.gameObject);
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

    GameObject spawnPlatform(Vector3 position, Quaternion orientation, int realmId) {
        GameObject platform = Instantiate(Platform, position, orientation, leavesContainer);
        return platform;
    }

    float avgNumBranches = 4f;
    GameObject spawnTree(Vector3 position, Quaternion orientation, int depth) {
        float trunkLength = 1400f * Mathf.Pow(1.85f, -depth) * Random.Range(0.95f, 1.05f);
        if (depth > 5) {
            trunkLength *= 0.5f;
        }
        GameObject trunk = Instantiate(Branch, position, orientation, branchesContainer);
        Vector3 trunkStart = trunk.transform.position;
        Vector3 trunkEnd = trunkStart + trunk.transform.up * trunkLength;
        trunk.transform.localScale = new Vector3(trunkLength / 40f, trunkLength / 2, trunkLength / 40f);
        trunk.transform.position += trunk.transform.up * trunkLength / 2;
        if (depth > 5) {
            int realmId = RandomIndexWithWeights(realmPlatformFrequencies);
            GameObject leaf = spawnPlatform(position, orientation, realmId);
            leaf.transform.position += leaf.transform.up * (trunkLength + leaf.transform.localScale.x);
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
        if (depth == 0) {
            numBranches = 9;
        }
        for (int i = 0; i < numBranches; i++) {
            float lerpVal = Random.Range(0.45f, 1);
            if (depth == 0) {
                lerpVal = Random.Range(0.2f, 1);
            }
            Vector3 newBranchPos = Vector3.Lerp(trunkStart, trunkEnd, lerpVal);
            float branchingAngle = (1 - lerpVal) * 80 + Random.Range(10, 30);
            Quaternion newBranchOrientation = orientation * Quaternion.AngleAxis(branchingAngle, Vector3.ProjectOnPlane(Random.onUnitSphere, trunk.transform.up));
            int newDepth = depth + 1;
            if (Random.Range(0f, 1f) < 0.025f && depth > 1) {
                newDepth = depth;
            }
            GameObject branch = spawnTree(newBranchPos, newBranchOrientation, newDepth);
        }
        return trunk;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            branchesContainer.gameObject.SetActive(!branchesContainer.gameObject.activeSelf);
        }
    }

    int RandomIndexWithWeights(float[] weights) {
        float total = 0f;
        foreach (float w in weights) {
            total += w;
        }
        float random = Random.Range(0f, total);
        int result = 0;
        float threshold = weights[0];
        while (random > threshold) {
            result++;
            threshold += weights[result];
        }
        return result;
    }
}
