using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

namespace Vonderportal
{
    // ensure class initializer is called whenever scripts recompile
    [InitializeOnLoadAttribute]
    public static class EditorSceneLoader
    {

        public static string[] dimension_names_override;
        // register an event handler when the class is initialized
        static EditorSceneLoader()
        {
            EditorApplication.playmodeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying && scene.name != "Root")
            {
                dimension_names_override = new string[] { scene.name };

                SceneManager.LoadScene("Root", LoadSceneMode.Single);
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
            }

        }
    }
}

#endif