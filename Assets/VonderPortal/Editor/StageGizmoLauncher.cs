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
<<<<<<< HEAD
        if (    StageGizmo.stageGizmo == null && 
                stageGizmo == null && 
                EditorApplication.isPlaying == false && 
                FindObjectsOfType(typeof(StageGizmo)).Length == 0) {

           stageGizmo = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load("[StageGizmo]"));
           //stageGizmo.hideFlags = HideFlags.HideInInspector;
        }        
    }
}
=======
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
>>>>>>> be7b4c109c2a8baa030cb61df9c5aa1868abcaab
