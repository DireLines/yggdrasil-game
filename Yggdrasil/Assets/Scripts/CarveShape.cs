using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;
using System.Linq;

public class CarveShape : MonoBehaviour {
    public GameObject carvingObject;
    public GameObject carvedObject;
    public GameObject sphereObject;
    public GameObject platformTemplate;
    Solid mesh;

    Solid ToSolid(GameObject obj) {
        Mesh m;
        Solid solid;
        Transform t = obj.transform;

        if (obj.GetComponent<MeshFilter>())
        {
            m = obj.GetComponent<MeshFilter>().mesh;
            solid = new Solid(m.vertices.Select(t.TransformVector).Select(ToVector3d).ToArray(), m.GetIndices(0));
        }
        else if (obj.GetComponent<Terrain>())
        {
            var terrain = obj.GetComponent<Terrain>();
            var terrainData = terrain.terrainData;
            var resolution = terrainData.heightmapResolution;
            var heightMap = terrainData.GetHeights(0, 0, resolution, resolution);
            
            // Todo: Get these actual values
            var xStep = 1000f / resolution;
            var yStep = 1000f / resolution;
            var terrainHeight = 600f; 
            
            List<Vector3> vertices = new List<Vector3>();
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    var xPos = x * xStep;
                    var yPos = y * yStep;
                    var zPos = heightMap[x, y] * terrainHeight;
                    vertices.Add(new Vector3(xPos, yPos, zPos));
                }
            }

            List<int> triangles = new List<int>();
            for (int i = 0; i < vertices.Count - resolution; i++)
            {
                if (i % resolution == resolution - 1) continue; // Skip over last row and column

                // Must be clockwise for normals to be correctly oriented
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + resolution);
                triangles.Add(i + 1);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution);
            }

            solid = new Solid(vertices.Select(t.TransformVector).Select(ToVector3d).ToArray(), triangles.ToArray());
        } 
        else
        {
            return null;
        }

        solid.Translate(ToVector3d(t.position));
        return solid;
    }
    public GameObject IntersectedWithSphere(GameObject sourceObject, Vector3 sourceCenter, float radius, Vector3 targetCenter) {
        GameObject carvingObject = Instantiate(sphereObject, sourceCenter, Quaternion.identity);
        carvingObject.transform.localScale *= radius * 2f;
        Solid carvingShape = ToSolid(carvingObject);
        Solid carvedShape = ToSolid(sourceObject);
        Destroy(carvingObject);

        var modeller = new BooleanModeller(carvingShape, carvedShape);
        Solid intersection = modeller.GetIntersection();
        intersection.Translate(ToVector3d(-carvingObject.transform.position));
        Vector3[] vertices = intersection.GetVertices().Select(ToUnityVector).ToArray();
        int[] indices = intersection.GetIndices();

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
    public List<GameObject> MakePlatform(Vector3 sourceCenter, float radius, Vector3 targetCenter) {
        Collider[] colliders = Physics.OverlapSphere(sourceCenter, radius, 1 << LayerMask.NameToLayer("Default"));
        GameObject[] objects = colliders.Select(c => c.gameObject).Distinct().ToArray();
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject obj in objects) {
            result.Add(IntersectedWithSphere(obj, sourceCenter, radius, targetCenter));
        }
        return result;
    }
    public void Start() {
        MakePlatform(carvingObject.transform.position, carvingObject.transform.localScale.x / 2f, transform.position);
    }

    Vector3 ToUnityVector(Vector3d vec) {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }
    Vector3d ToVector3d(Vector3 vec) {
        return new Vector3d(vec.x, vec.y, vec.z);
    }
}
