using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dimension : MonoBehaviour {

    public string dimensionName;
    public Scene scene;
    private SceneType sceneType;

    int sceneLayer { get
        {
            switch (sceneType)
            {
                case SceneType.last:
                    return LayerMask.NameToLayer("LastScene");
                case SceneType.current:
                    return LayerMask.NameToLayer("CurrentScene");
                case SceneType.next:
                    return LayerMask.NameToLayer("NextScene");
                default:
                    Debug.LogError("Scene layers not set up");
                    return LayerMask.NameToLayer("Default");
            }
        }
    }

    public List<GameObject> rootObjects;

    public enum SceneType
    {
        last,
        current,
        next
    }



    public void LoadScene( SceneType _sceneType) {
        sceneType = _sceneType;

        if ( !scene.isLoaded)
        {
            StartCoroutine(LoadSceneAndInit());
        } else
        {
            pruneSceneAndSetLayers();
        }        
    }

    public void UnloadScene()
    {
        SceneManager.UnloadSceneAsync(scene);
    }


    IEnumerator LoadSceneAndInit()
    {
        AsyncOperation asyncLoad;
        
        asyncLoad = SceneManager.LoadSceneAsync(dimensionName, LoadSceneMode.Additive);
        if (asyncLoad.isDone)
        {
            pruneSceneAndSetLayers();
        } else
        {
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            pruneSceneAndSetLayers();
        }

    }
    void pruneSceneAndSetLayers()
    {
        scene = SceneManager.GetSceneByName(dimensionName);
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == "Persistant")
            {
                UnityEngine.Object.Destroy(rootObject);
            }
            else
            {
                Transform[] childrenTransforms = rootObject.GetComponentsInChildren<Transform>();
                foreach (Transform t in childrenTransforms)
                {
                    t.gameObject.layer = sceneLayer;
                }
            }
        }
    }
}
