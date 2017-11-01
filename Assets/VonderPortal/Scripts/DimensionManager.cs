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
        private int dimensionIndex;

        public delegate void ChangeDimensionHandler(int dimensionIndex);
        public static event ChangeDimensionHandler ChangeDimension;

        void OnEnable()
        {
            ChangeDimension += ChangeLoadedDimensions;
        }


        void OnDisable()
        {
            ChangeDimension -= ChangeLoadedDimensions;
        }

        private void Awake()
        {
            // Validate List of Scene names
            List<string> scenesInBuild = new List<string>();
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
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
            ChangeDimension(1);

            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("LastScene"));
            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("NextScene")); ;

        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click"))
            {
                if (ChangeDimension != null)
                    ChangeDimension(dimensionIndex + 1);
            }
        }

        void ChangeLoadedDimensions(int _dimensionIndex)
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
