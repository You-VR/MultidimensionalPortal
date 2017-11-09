using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vonderportal
{
    public enum SceneType
    {
        last,
        current,
        next
    }

    public class Dimension : MonoBehaviour
    {

        public string dimensionName;
        public Scene scene;
        private SceneType sceneType;

        int sceneLayer
        {
            get
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





        public void LoadScene(SceneType _sceneType)
        {
            sceneType = _sceneType;

            if (!scene.isLoaded)
            {
                StartCoroutine(LoadSceneAndInit());
            }
            else
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
            }
            else
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
               

                // Remove everything in the persistant object
                if (rootObject.name == "Persistant")
                {
                    UnityEngine.Object.Destroy(rootObject);
                }
                else if (rootObject.name == "Tracked")
                {
                    Debug.Log("Tracked Objects in scene");
                }

                // Deal with portals in other worlds
                if (rootObject.name == "Portals")
                {
                    bool set;
                    if (sceneType != SceneType.current) { set = false; }
                    else { set = true; }

                    Transform[] childrenTransforms = rootObject.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in childrenTransforms)
                    {

                        PortalSwitch portalSwitch = t.GetComponentInChildren<PortalSwitch>(true);
                        ActiveSurface activeSurface = t.GetComponentInChildren<ActiveSurface>(true);

                        if( portalSwitch || activeSurface) { t.gameObject.SetActive(set); }
                    }




                }
                else
                {
                    Transform[] childrenTransforms = rootObject.GetComponentsInChildren<Transform>();
                    foreach (Transform t in childrenTransforms)
                    {
                        // Place game objects on the right layer
                        t.gameObject.layer = sceneLayer;


                        //Mute audiosources
                        AudioSource[] audioSources = t.GetComponentsInChildren<AudioSource>();
                        foreach (AudioSource audioSource in audioSources)
                        {
                            if (sceneType != SceneType.current)
                            {
                                audioSource.mute = true;
                            }
                            else
                            {
                                audioSource.mute = false;
                            }
                        }
                    }
                }
            }
        }
    }
}