using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;
using System.Linq;

public class CarveShape : MonoBehaviour {
    public GameObject carvingObject;
    public GameObject carvedObject;
    public Solid mesh;
    public Material ObjMaterial;

    Solid ToSolid(GameObject obj) {
        Mesh m = obj.GetComponent<MeshFilter>().mesh;
        Transform t = obj.transform;
        var solid = new Solid(m.vertices.Select(ToVector3d).ToArray(), m.GetIndices(0));
        solid.Scale(ToVector3d(t.localScale));
        solid.Translate(ToVector3d(t.position));
        return solid;
    }

    public void Start() {
        var carvingShape = ToSolid(carvingObject);
        var carvedShape = ToSolid(carvedObject);

        //--

        //mesh = s;

        //--

        // var modeller = new BooleanModeller(b, c1);
        // mesh = modeller.getDifference();

        //--

        var modeller = new BooleanModeller(carvingShape, carvedShape);
        var tmp = modeller.GetIntersection();

        mesh = tmp;

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        Mesh tmesh = new Mesh();
        Vector3[] vertices = mesh.GetVertices().Select(ToUnityVector).ToArray();
        tmesh.vertices = vertices;

        tmesh.triangles = mesh.GetIndices();

        tmesh.RecalculateBounds();
        tmesh.RecalculateNormals();
        tmesh.Optimize();
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
