using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vonderportal
{
    class ActiveDimensions
    {
        private Dimension[] activeDimensions = new Dimension[3];

        public Dimension lastDimension { get { return activeDimensions[0]; } }
        public Dimension currDimension { get { return activeDimensions[1]; } }
        public Dimension nextDimension { get { return activeDimensions[2]; } }



        public void set(Dimension[] dimensions)
        {
            var unloadDimensions = activeDimensions.Except(dimensions);
            activeDimensions = dimensions;

            foreach (Dimension unloadDimension in unloadDimensions)
            {
                if (unloadDimension != null) { unloadDimension.UnloadScene(); }
            }

            if (activeDimensions[0] != null) { activeDimensions[0].LoadScene(SceneType.last); }
            if (activeDimensions[1] != null) { activeDimensions[1].LoadScene(SceneType.current); }
            if (activeDimensions[2] != null) { activeDimensions[2].LoadScene(SceneType.next); }
        }
    }


    public class DimensionManager : MonoBehaviour
    {

        public Camera mainCamera;
        public string[] dimension_names;

        private List<Dimension> dimensions;
        private ActiveDimensions activeDimensions;
        public int dimensionIndex { get; private set; }

        public static DimensionManager dimensionManagerInstance;

        public delegate void ChangeDimensionHandler(int dimensionIndex);
        public static event ChangeDimensionHandler onChangeDimension;
        public void ChangeDimension(SceneType sceneType) {
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

        void OnEnable()
        {
            onChangeDimension += ChangeLoadedDimensions;
        }


        void OnDisable()
        {
            onChangeDimension -= ChangeLoadedDimensions;

            
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
                    dimensions.Add(newDimension);
                }
            }

            activeDimensions = new ActiveDimensions();
        }

        // Use this for initialization
        void Start()
        {
            onChangeDimension(1);

            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("LastScene"));
            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("NextScene")); ;

        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click"))
            {
                if (onChangeDimension != null)
                    ChangeDimension(SceneType.next);
            }
        }

        public void ChangeLoadedDimensions(int _dimensionIndex)
        {
            if (_dimensionIndex <= dimensions.Count)
            {
                dimensionIndex = _dimensionIndex;
                assignDimensions();

            }
            else
            {
                Debug.LogError("Invalid dimension index: " + dimensionIndex);
            }

        }

        void assignDimensions()
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
    }
}
