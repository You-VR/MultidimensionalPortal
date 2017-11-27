using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vonderportal
{
    public class ActiveDimensions
    {
        public Dimension lastDimension { get { return activeDimensions[0]; } }
        public Dimension currDimension { get { return activeDimensions[1]; } }
        public Dimension nextDimension { get { return activeDimensions[2]; } }

        public int portalLayer { get { return PortalLayer;         } }

        public int lastLayer   {  get {
                if(lastDimension != null) { return lastDimension.layer;  }
                else                      { return 0;                    } } }

        public int currLayer   { get {
                if (currDimension != null) { return currDimension.layer; }
                else                       { return 0;                   } } }

        public int nextLayer   { get {
                if (nextDimension != null) { return nextDimension.layer; }
                else                       { return 0;                   } } }

		public int currentCullingMask{ 
			get{
				int cullingMask = 0;

				foreach (int defaultLayer in defaultLayers) {
					cullingMask |=  (1 << defaultLayer);
				}
				cullingMask |=  (1 << PortalLayer);
				cullingMask |=  (1 << currLayer);

				return cullingMask;
			}
		}

        private int PortalLayer;
        private int SceneLayer1;
        private int SceneLayer2;
        private int SceneLayer3;

        public int[] defaultLayers;

        private int[] sceneLayers = new int[3];

        private Dimension[] activeDimensions = new Dimension[3];
        private bool unidirectional;

        public ActiveDimensions( bool _unidirectional)
        {
            unidirectional = _unidirectional;


			// Default Layers
			defaultLayers = new int[]{
				LayerMask.NameToLayer("Default"),
				//LayerMask.NameToLayer("TransparentFX")
			};


            PortalLayer = LayerMask.NameToLayer("Portal");
            SceneLayer1 = LayerMask.NameToLayer("SceneLayer1");
            SceneLayer2 = LayerMask.NameToLayer("SceneLayer2");
            SceneLayer3 = LayerMask.NameToLayer("SceneLayer3");

            if (PortalLayer == -1) { Debug.Log("Portal Layer not found"); }
            if (SceneLayer1 == -1) { Debug.Log("SceneLayer1 not found"); }
            if (SceneLayer2 == -1) { Debug.Log("SceneLayer2 not found"); }
            if (SceneLayer3 == -1) { Debug.Log("SceneLayer3 not found"); }

            sceneLayers[0] = SceneLayer1;
            sceneLayers[1] = SceneLayer2;
            sceneLayers[2] = SceneLayer3;
        }

        public void set(Dimension[] dimensions)
        {
            // Unload layers that are no longer in the list

            var unloadDimensions = activeDimensions.Except(dimensions);
            activeDimensions = dimensions;
            if (!unidirectional)
            {
                foreach (Dimension unloadDimension in unloadDimensions)
                {
                    if (unloadDimension != null)
                    {
                        unloadDimension.UnloadScene();
                    }
                }
            }

            // Find unallocated scene layers
            List<int> usedLayers = new List<int>();
            foreach(Dimension dimension in activeDimensions)
            {
                if(dimension!= null && dimension.layer != 0) { usedLayers.Add(dimension.layer); }
            }
            List<int> unusedLayers = sceneLayers.ToList().Except(usedLayers).ToList();

            int layer;
            int i = 0;
            if (activeDimensions[0] != null) {
                if(activeDimensions[0].layer != 0) { layer = activeDimensions[0].layer; }
                else { layer = unusedLayers[i++]; }

                if (unidirectional)
                {
                    activeDimensions[0].UnloadScene();
                    Debug.Log("Unload");
                } else
                {
                    activeDimensions[0].LoadScene(SceneType.last, layer);
                }
                    
            }
            if (activeDimensions[1] != null) {
                if (activeDimensions[1].layer != 0) { layer = activeDimensions[1].layer; }
                else { layer = unusedLayers[i++]; }

                activeDimensions[1].LoadScene(SceneType.current, layer);
            }
            if (activeDimensions[2] != null) {
                if (activeDimensions[2].layer != 0) { layer = activeDimensions[2].layer; }
                else { layer = unusedLayers[i++]; }

                activeDimensions[2].LoadScene(SceneType.next, layer);
            }
        }
    }
    public class DimensionManager : MonoBehaviour
    {
        //*************************************************************************************************************************//
        // PUBLIC PROPERTIES                                                                                                       //
        //*************************************************************************************************************************//
        public static DimensionManager dimensionManagerInstance;

        // SET IN EDITOR
        public Camera mainCamera;
        public TrackedObjectManager trackedObjectManager;
        public string[] dimension_names;
        public bool unidirectional = true;

        // OTHER
        public ActiveDimensions activeDimensions;
        public int dimensionIndex { get; private set; }

        //  EVENTS
        public delegate void ChangeDimensionHandler(int dimensionIndex);
        public static event ChangeDimensionHandler onChangeDimension;

        //*************************************************************************************************************************//
        // PRIVATE PROPERTIES                                                                                                      //
        //*************************************************************************************************************************//
        private List<Dimension> dimensions;


        //*************************************************************************************************************************//
        // PRIVATE FUNCTIONS                                                                                                       //
        //*************************************************************************************************************************//

        void OnEnable()
        {
            onChangeDimension += ChangeLoadedDimensions;
            onChangeDimension += ChangeMainCameraCullingMask;
        }
        void OnDisable()
        {
            onChangeDimension -= ChangeLoadedDimensions;
            onChangeDimension -= ChangeMainCameraCullingMask;

        }
        private void Awake()
        {
            if (dimensionManagerInstance == null) { dimensionManagerInstance = this; }
            else
            {
                Debug.LogError("Only one instance of dimension manager allowed");
            }


            // Validate List of Scene names
            List<string> scenesInBuild = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/");
                scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            }

            dimensions = new List<Dimension>();

#if UNITY_EDITOR
            // If single dimension was loaded, not root
            if (EditorSceneLoader.dimension_names_override != null)
            {
                dimension_names = EditorSceneLoader.dimension_names_override;
            }
#endif

            foreach (string dimension_name in dimension_names)
            {
                if (!scenesInBuild.Contains(dimension_name))
                {
                    Debug.LogError("Scene not found: " + dimension_name);
                }
                else
                {
                    Dimension newDimension = this.gameObject.AddComponent<Dimension>();
                    newDimension.dimensionName = dimension_name;
                    newDimension.hideFlags = HideFlags.HideInInspector;
                    dimensions.Add(newDimension);
                }
            }

            activeDimensions = new ActiveDimensions(unidirectional);
        }

        // Use this for initialization
        void Start()
        {
            onChangeDimension(1);
            ChangeMainCameraCullingMask(1);
        }

        public void ChangeDimension(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.last:
                    onChangeDimension.Invoke(dimensionIndex - 1);
                    break;
                case SceneType.current:
                    onChangeDimension.Invoke(dimensionIndex);
                    break;
                case SceneType.next:
                    onChangeDimension.Invoke(dimensionIndex + 1);
                    break;
            }
        }

        public void ChangeLoadedDimensions(int _dimensionIndex)
        {
            if (_dimensionIndex <= dimensions.Count)
            {
                dimensionIndex = _dimensionIndex;
                AssignDimensions();
            }
            else
            {
                Debug.LogError("Invalid dimension index: " + dimensionIndex);
            }

        }
        public void ChangeMainCameraCullingMask(int _dimensionIndex)
        {
			mainCamera.cullingMask = activeDimensions.currentCullingMask;

        }


        void AssignDimensions()
        {
            // Check dimension index is valid
            if (dimensionIndex < 0 || dimensionIndex > dimensions.Count)
            {
                Debug.LogError("Invalid dimension index: " + dimensionIndex);
                return;
            }

            switch (dimensions.Count)
            {
                case 1:
                    activeDimensions.set(new Dimension[] { null, dimensions[0], null });
                    break;
                case 2:
                    // First dimension
                    if (dimensionIndex == 1)
                    {
                        activeDimensions.set(new Dimension[] { null, dimensions[0], dimensions[1] });
                    }
                    // Last dimension
                    else
                    {
                        activeDimensions.set(new Dimension[] { dimensions[0], dimensions[1], null });
                    }
                    break;
                default:
                    // First dimension
                    if (dimensionIndex == 1)
                    {
                        activeDimensions.set(new Dimension[] { null, dimensions[0], dimensions[1] });
                    }
                    // Last dimension
                    else if (dimensionIndex == dimensions.Count)
                    {
                        activeDimensions.set(new Dimension[] { dimensions[dimensionIndex - 2], dimensions[dimensionIndex - 1], null });
                    }
                    // Middle dimension
                    else
                    {
                        activeDimensions.set(new Dimension[] { dimensions[dimensionIndex - 2], dimensions[dimensionIndex - 1], dimensions[dimensionIndex] });
                    }
                    break;
            }
        }

        //*************************************************************************************************************************//
        // GUI FUNCTIONS                                                                                                           //
        //*************************************************************************************************************************//

        void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Next Level"))
            {
                if (onChangeDimension != null)
                    ChangeDimension(SceneType.next);
            }
        }
    }


}
