using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoadAttribute]
public class StageGizmo : Editor
{
    public GameObject stageModel;
    private MeshFilter[] meshes;
    public Material mat;
    bool wireframe = true;

    void OnEnable()
    {
        stageModel = EditorGUIUtility.Load("Models/VR_Stage2") as GameObject;
        mat = EditorGUIUtility.Load("Materials/StageGizmoMat") as Material;


        if (stageModel != null)
        {
            meshes = stageModel.GetComponentsInChildren<MeshFilter>();
        }
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    void DrawStage()
    {

        if (stageModel != null && mat != null) {   
            foreach (MeshFilter mesh in meshes)
            {
                mesh.sharedMesh.RecalculateNormals();

                if (wireframe)
                {
                    Gizmos.DrawWireMesh(mesh.sharedMesh, mesh.transform.position, mesh.transform.rotation, mesh.transform.localScale);
                }
                else
                {
                    for (int i = 0; i < mat.passCount; i++)
                        if (mat.SetPass(0))
                        {
                            Graphics.DrawMeshNow(mesh.sharedMesh, mesh.transform.localToWorldMatrix);
                        }                            
                }
            }
        }
    }
}
