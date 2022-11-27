using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;
using System.Linq;

public class CarveShape : MonoBehaviour {
    public GameObject carvingObject;
    public GameObject carvedObject;
    Solid mesh;
    public Material ObjMaterial;

    Solid ToSolid(GameObject obj) {
        Mesh m = obj.GetComponent<MeshFilter>().mesh;
        Transform t = obj.transform;
        var solid = new Solid(m.vertices.Select(t.TransformVector).Select(ToVector3d).ToArray(), m.GetIndices(0));
        solid.Translate(ToVector3d(t.position));
        return solid;
    }

    public void Start() {
        var carvingShape = ToSolid(carvingObject);
        var carvedShape = ToSolid(carvedObject);
        foreach (Vector3d v in carvedShape.GetVertices()) {
            print(v);
        }

        var modeller = new BooleanModeller(carvingShape, carvedShape);
        var intersection = modeller.GetIntersection();
        intersection.Translate(ToVector3d(-carvingObject.transform.position));

        Vector3[] vertices = intersection.GetVertices().Select(ToUnityVector).ToArray();
        int[] indices = intersection.GetIndices();
        Mesh tmesh = new Mesh();
        tmesh.vertices = vertices;
        tmesh.triangles = indices;
        tmesh.RecalculateBounds();
        tmesh.RecalculateNormals();
        tmesh.Optimize();

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = tmesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[1];
        mr.materials[0] = ObjMaterial;
        mr.material = ObjMaterial;
    }

    Vector3 ToUnityVector(Vector3d vec) {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }
    Vector3d ToVector3d(Vector3 vec) {
        return new Vector3d(vec.x, vec.y, vec.z);
    }
}
