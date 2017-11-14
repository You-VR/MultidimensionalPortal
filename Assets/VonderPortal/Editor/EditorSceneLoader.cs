using UnityEngine;
using UnityEditor;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public static class EditorSceneLoader
{
    // register an event handler when the class is initialized
    static EditorSceneLoader()
    {
        //EditorApplication.playModeStateChanged += LogPlayModeState;
        EditorApplication.playmodeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState()
    {

        if (EditorApplication.isPlaying)
        {
            Debug.Log("isPlaying");
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.Log("isPlayingOrWillChangePlaymode");
        }
        
    }
}