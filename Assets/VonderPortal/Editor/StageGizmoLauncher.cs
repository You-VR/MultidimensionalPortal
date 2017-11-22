using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
class StageGizmoLauncher : Editor
{
    static GameObject stageGizmo;
    static StageGizmoLauncher()
    {
        EditorApplication.update += makeStageGizmo;
    }

    static void makeStageGizmo()
    {
        if (StageGizmo.stageGizmo == null &&
                stageGizmo == null &&
                EditorApplication.isPlaying == false &&
                FindObjectsOfType(typeof(StageGizmo)).Length == 0)
        {

            stageGizmo = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load("[StageGizmo]"));
            //stageGizmo.hideFlags = HideFlags.HideInInspector;
        }
    }
}