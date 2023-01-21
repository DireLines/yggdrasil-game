using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;
using System.Linq;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using System.Threading.Tasks;

public class CarveShape : MonoBehaviour {
    public GameObject sphereObject;
    public GameObject platformTemplate;

    Solid ToSolid(GameObject obj) {
        Transform t = obj.transform;
        Mesh m = obj.GetComponent<MeshFilter>().mesh;
        Solid solid = new Solid(m.vertices.Select(t.TransformVector).Select(ToVector3d).ToArray(), m.GetIndices(0));
        solid.Translate(ToVector3d(t.position));
        return solid;
    }
    public async Task<GameObject> IntersectedWithSphere(GameObject sourceObject, Vector3 sourceCenter, float radius, Vector3 targetCenter) {
        GameObject tempCarvingObject = Instantiate(sphereObject, sourceCenter, Quaternion.identity);
        tempCarvingObject.transform.localScale *= radius * 2f;
        Solid carvingShape = ToSolid(tempCarvingObject);
        Solid carvedShape = ToSolid(sourceObject);

        var modeller = await Task.Run<BooleanModeller>(()=>new BooleanModeller(carvingShape, carvedShape));
        Solid intersection = modeller.GetIntersection();
        intersection.Translate(ToVector3d(-tempCarvingObject.transform.position));
        Vector3[] vertices = intersection.GetVertices().Select(ToUnityVector).ToArray();
        int[] indices = intersection.GetIndices();
        DestroyImmediate(tempCarvingObject);

        Mesh tmesh = new Mesh();
        tmesh.vertices = vertices;
        tmesh.triangles = indices;
        tmesh.name = "Intersection";
        tmesh.RecalculateBounds();
        tmesh.RecalculateNormals();
        tmesh.Optimize();


        GameObject result = Instantiate(platformTemplate, targetCenter, Quaternion.identity);

        MeshFilter mf = result.AddComponent<MeshFilter>();
        mf.mesh = tmesh;

        Material mat = sourceObject.GetComponent<MeshRenderer>().material;
        MeshRenderer mr = result.AddComponent<MeshRenderer>();
        mr.materials = new Material[1];
        mr.materials[0] = mat;
        mr.material = mat;

        result.AddComponent<MeshCollider>();
        return result;
    }
    public async Task<List<GameObject>> MakePlatform(Vector3 sourceCenter, float radius, Vector3 targetCenter) {
        Collider[] colliders = Physics.OverlapSphere(sourceCenter, radius, 1 << LayerMask.NameToLayer("Default"));
        GameObject[] objects = colliders.Select(c => c.gameObject).Distinct().ToArray();
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject obj in objects) {
            result.Add(await IntersectedWithSphere(obj, sourceCenter, radius, targetCenter));
        }
        return result;
    }

    // public IEnumerator<GameObject> MakePlatformAsync(Vector3 sourceCenter, float radius, Vector3 targetCenter) {
    //     Collider[] colliders = Physics.OverlapSphere(sourceCenter, radius, 1 << LayerMask.NameToLayer("Default"));
    //     GameObject[] objects = colliders.Select(c => c.gameObject).Distinct().ToArray();
    //     foreach (GameObject obj in objects) {
    //         yield return IntersectedWithSphere(obj, sourceCenter, radius, targetCenter);
    //     }
    // }

    Vector3 ToUnityVector(Vector3d vec) {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }
    Vector3d ToVector3d(Vector3 vec) {
        return new Vector3d(vec.x, vec.y, vec.z);
    }
}
