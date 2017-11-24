using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Callbacks;

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

        public Scene scene { get { return SceneManager.GetSceneByName(dimensionName); } }
        public TrackedObjectManager trackedObjectManager { get { return TrackedObjectManager.trackedObjectManagerInstance; } }


        private SceneType sceneType;
        public int layer { get; private set; }
        bool layerInit = true;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += onSceneLoad;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= onSceneLoad;
        }


        public void LoadScene(SceneType _sceneType, int _layer)
        {
            sceneType = _sceneType;
            if (layer != _layer) { layerInit = true; }
            layer = _layer;

            if (!scene.isLoaded)
            {
                AsyncOperation asyncLoad;
                asyncLoad = SceneManager.LoadSceneAsync(dimensionName, LoadSceneMode.Additive);
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
            
        void onSceneLoad(Scene _scene, LoadSceneMode mode)
        {
            if( _scene == scene)
            {
                pruneSceneAndSetLayers();
            }        
        }

        //[PostProcessSceneAttribute(0)]
        //public static void OnPostprocessScene()
        //{
        //    Debug.Log("Test");
        //}

        void pruneSceneAndSetLayers()
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                switch (rootObject.name)
                {
                    case "Persistant":
                        UnityEngine.Object.Destroy(rootObject);
                        break;

                    case "Tracked":
                        if (sceneType == SceneType.current)
                        {
                            rootObject.SetActive(true);

                            TrackedObject[] oldTrackedObjects = trackedObjectManager.gameObject.GetComponentsInChildren<TrackedObject>(true);
                            TrackedObject[] newTrackedObjects = rootObject.GetComponentsInChildren<TrackedObject>(true);
                            foreach (TrackedObject newTrackedObject in newTrackedObjects)
                            {
                                TrackedObject.VonderTrackedObject newTrackedObjectIndex = newTrackedObject.trackedObjectIndex;

                                foreach (TrackedObject oldTrackedObject in oldTrackedObjects)
                                {
                                    if( newTrackedObjectIndex == oldTrackedObject.trackedObjectIndex)
                                    {
                                        foreach (Transform oldTrackedObjectChild in oldTrackedObject.transform)
                                        {
                                            Destroy(oldTrackedObjectChild.gameObject);
                                        }


                                        foreach (Transform newTrackedObjectChild in newTrackedObject.transform)
                                        {
                                            

                                            Instantiate(    newTrackedObjectChild.gameObject, 
                                                            newTrackedObjectChild.localPosition, 
                                                            newTrackedObjectChild.localRotation, 
                                                            oldTrackedObject.transform);
                                        }
                                    }
                                }
                            }
                        }
                        rootObject.SetActive(false);

                        //Debug.Log("Tracked Objects in scene");
                        break;


                    case "Portals":
                        bool set;
                        if (sceneType != SceneType.current) { set = false; }
                        else { set = true; }

                        PortalSwitch portalSwitch = rootObject.GetComponentInChildren<PortalSwitch>(true);
                        ActiveSurface activeSurface = rootObject.GetComponentInChildren<ActiveSurface>(true);

                        if (portalSwitch || activeSurface) { rootObject.gameObject.SetActive(set); }

                        break;

                    default:
                        // Place game objects on the right layer
                        if (layerInit) {
                            Transform[] childrenTransforms = rootObject.GetComponentsInChildren<Transform>();
                            foreach (Transform t in childrenTransforms)
                            {
                                t.gameObject.layer = layer;
                            }

                            //Mute audiosources
                            Light[] lightSources = rootObject.GetComponentsInChildren<Light>();
                            foreach (Light light in lightSources)
                            {
                                light.cullingMask = 0;
                                light.cullingMask |= ( 1 << layer);
                            }
                        }

                        //Mute audiosources
                        AudioSource[] audioSources = rootObject.GetComponentsInChildren<AudioSource>();
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
                        break;
                }
            }

            layerInit = false;
        }
    }
}