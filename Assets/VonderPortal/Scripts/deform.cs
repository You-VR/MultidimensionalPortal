using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deform : MonoBehaviour {
    public Camera mainCamera;
    public float threshold = 0.3f;

    Vector3 faceDir;

    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;

    // Use this for initialization
    void Start () {
        faceDir = this.transform.up;

        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
    }
	
	// Update is called once per frame
	void Update () {
		for( int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 dist = (mainCamera.transform.position - vertextToWorldSpace(originalVertices[i]));

            if ( dist.magnitude < threshold)
            {
                
                displacedVertices[i] = originalVertices[i] - (1.0f / dist.magnitude * transform.InverseTransformDirection( faceDir));
            } else
            {
                displacedVertices[i] = originalVertices[i];
            }
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();

    }

    Vector3 vertextToWorldSpace( Vector3 vertex)
    {
        return transform.TransformPoint( vertex );
    }
}
