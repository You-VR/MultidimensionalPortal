using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGizmo : MonoBehaviour {
    public Mesh mesh;

    void OnDrawGizmos()
    {
        Gizmos.DrawWireMesh(mesh);
    }
}
