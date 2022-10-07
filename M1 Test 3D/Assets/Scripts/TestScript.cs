using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
    Vector3 rotationAxis;
    public GameObject PointLight;
    private void Start() {
        rotationAxis = Random.onUnitSphere;
        // transform.localScale *= Random.Range(0.5f, 2f);
        // if (Random.Range(0f, 1f) < 0.001f) {
        // Instantiate(PointLight, transform.position + Random.insideUnitSphere * 5f, Quaternion.identity, transform);
        // }
        // transform.rotation = Random.rotation;
        // transform.rotation = Quaternion.identity;
    }

    void Update() {
        // transform.rotation *= Quaternion.AngleAxis(20f * Time.deltaTime, rotationAxis);
        // transform.position += Random.insideUnitSphere * 0.01f;
    }
}