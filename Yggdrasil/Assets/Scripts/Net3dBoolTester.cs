using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;
using System.Linq;

public class Net3dBoolTester : MonoBehaviour {
    public Material ObjMaterial;
    public Solid mesh;

    public Color[] getColorArray(int length, Color c) {
        var ar = new Color[length];
        for (var i = 0; i < length; i++)
            ar[i] = new Color(c.r, c.g, c.b);
        return ar;
    }

    public void Start() {

        var box = new Solid(DefaultCoordinates.DEFAULT_BOX_VERTICES,
        DefaultCoordinates.DEFAULT_BOX_COORDINATES);

        var sphere = new Solid(DefaultCoordinates.DEFAULT_SPHERE_VERTICES,
        DefaultCoordinates.DEFAULT_SPHERE_COORDINATES);
        sphere.Scale(0.68f, 0.68f, 0.68f);

        var cylinder1 = new Solid(DefaultCoordinates.DEFAULT_CYLINDER_VERTICES,
        DefaultCoordinates.DEFAULT_CYLINDER_COORDINATES);
        cylinder1.Scale(0.38f, 1f, 0.38f);

        var cylinder2 = new Solid(DefaultCoordinates.DEFAULT_CYLINDER_VERTICES,
        DefaultCoordinates.DEFAULT_CYLINDER_COORDINATES);
        cylinder2.Scale(0.38f, 1, 0.38f);
        cylinder2.Rotate(Mathf.PI / 2f, 0);

        var cylinder3 = new Solid(DefaultCoordinates.DEFAULT_CYLINDER_VERTICES,
        DefaultCoordinates.DEFAULT_CYLINDER_COORDINATES);
        cylinder3.Scale(0.38f, 1f, 0.38f);
        cylinder3.Rotate(Mathf.PI / 2f, 0f);
        cylinder3.Rotate(0f, Mathf.PI / 2f);

        //--

        //mesh = s;

        //--

        // var modeller = new BooleanModeller(b, c1);
        // mesh = modeller.getDifference();

        //--

        var modeller = new BooleanModeller(box, sphere);
        var tmp = modeller.GetIntersection();

        modeller = new BooleanModeller(tmp, cylinder1);
        tmp = modeller.GetDifference();

        modeller = new BooleanModeller(tmp, cylinder2);
        tmp = modeller.GetDifference();

        modeller = new BooleanModeller(tmp, cylinder3);
        tmp = modeller.GetDifference();

        mesh = tmp;

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        Mesh tmesh = new Mesh();
        Vector3[] vertices = mesh.GetVertices().Select(ToUnityVector).ToArray();
        tmesh.vertices = vertices;

        tmesh.triangles = mesh.GetIndices();

        tmesh.RecalculateNormals();
        mf.mesh = tmesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[1];
        mr.materials[0] = ObjMaterial;
        mr.material = ObjMaterial;
    }

    Vector3 ToUnityVector(Vector3d vec) {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }
}
