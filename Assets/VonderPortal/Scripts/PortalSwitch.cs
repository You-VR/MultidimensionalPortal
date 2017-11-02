using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Vonderportal
{
    public class PortalSwitch : MonoBehaviour
    {
        bool allowSwitch = true;

        public bool automatic = true;
        public DimensionManager dimensionManager;
        public Camera mainCamera;

        private PortalSurface portalSurface { get { return GetComponent<PortalSurface>(); } }
        private SceneType toDimension;

        void Awake()
        {
            if (automatic)
            {
                toDimension = portalSurface.toDimension;
                mainCamera  = portalSurface.mainCamera;
                toDimension = portalSurface.toDimension;
            }
        }

        void Update()
        {
            Vector3 convertedPoint = portalSurface.transform.InverseTransformPoint(mainCamera.transform.position);

            if ((convertedPoint.z > 0) != portalSurface.triggerZDirection && Mathf.Abs(convertedPoint.z) > portalSurface.portalSwitchDistance)
            {
                Debug.Log("!");
                if (allowSwitch)
                {
                    Debug.Log("!");
                    dimensionManager.ChangeDimension(toDimension);
                    allowSwitch = false;
                }

            }
        }
    }


    [CustomEditor(typeof(PortalSwitch))]
    public class MyScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PortalSwitch portalSwitch = target as PortalSwitch;

            portalSwitch.automatic = GUILayout.Toggle(portalSwitch.automatic, "Automatic");
            portalSwitch.dimensionManager = (DimensionManager)EditorGUILayout.ObjectField("Dimension Manager", portalSwitch.dimensionManager, typeof(DimensionManager), true);

            if (!portalSwitch.automatic)
            {

            } else
            {
                GUILayout.TextArea("Settings taken from PortalSurface");
            }

        }
    }
}