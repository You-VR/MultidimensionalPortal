using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class StageGizmo : MonoBehaviour
{
    public bool wireframe = false;
    [Range(0.0f, 1.0f)]
    public float transparency = 0.5f;

    private GameObject stageModel;
    private MeshFilter[] meshes;
    private Material mat;


    public static StageGizmo stageGizmo;

    private void Awake()
    {
        if( stageGizmo == null)
        {
            stageGizmo = this;
            stageModel = Resources.Load("VR_Stage2") as GameObject;
            mat = Resources.Load("StageGizmoMat") as Material;
            meshes = stageModel.GetComponentsInChildren<MeshFilter>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnDrawGizmos()
    {

<<<<<<< HEAD
        if (stageModel == null || mat == null)
=======
        if (stageModel == null || mat == null || meshes == null)
>>>>>>> be7b4c109c2a8baa030cb61df9c5aa1868abcaab
        {
            stageModel = Resources.Load("VR_Stage2") as GameObject;
            mat = Resources.Load("StageGizmoMat") as Material;
            meshes = stageModel.GetComponentsInChildren<MeshFilter>();
        }

        if (meshes != null && mat != null)
        {
            Color col = mat.GetColor("_Color");
            col.a = transparency;
            mat.SetColor("_Color", col);
            foreach (MeshFilter mesh in meshes)
            {
                if (mat.SetPass(0))
                {
                    if (!wireframe) { Graphics.DrawMeshNow(mesh.sharedMesh, mesh.transform.localToWorldMatrix); }
                    else
                    {
                        Gizmos.color = col;
                        Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.transform.position, mesh.transform.rotation, mesh.transform.localScale);
                    }
                    
                }
            }
        }
        else
        {
            Debug.LogError("Stage model not loaded");
        }
    }
}
