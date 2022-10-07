using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCubes : MonoBehaviour {
    public GameObject Platform;
    public GameObject Empty;
    public List<Color> colors = new List<Color>(9);

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < 9; i++) { colors.Add(Random.ColorHSV()); }
        spawnTree(transform.position, Quaternion.identity, 0);
        // spawnTree(transform.position + transform.up * 1000, Quaternion.identity, 0);
        // spawnTree(transform.position + transform.up * 2000, Quaternion.identity, 0);
        print(transform.childCount + " leaves");
    }

    GameObject spawnPlatform(Vector3 position, Quaternion orientation) {
        GameObject platform = Instantiate(Platform, position, orientation, transform);
        Color color = colors[Random.Range(0, colors.Count)];
        foreach (MeshRenderer meshRenderer in platform.GetComponentsInChildren<MeshRenderer>()) {
            meshRenderer.material.color = color;
        }
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
            if (!branch.GetComponent<TestScript>()) {
                Destroy(branch);
            }
        }
        return trunk;
    }


}
