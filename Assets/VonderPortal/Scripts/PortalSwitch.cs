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

        private DimensionManager dimensionManager { get { return DimensionManager.dimensionManagerInstance; } }
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
                if (allowSwitch)
                {
                    dimensionManager.ChangeDimension(toDimension);
                    StartCoroutine(DisablePortal());                    
                }

            }
        }
        IEnumerator DisablePortal()
        {
            allowSwitch = false;

            yield return new WaitForSeconds(3.0f);
            allowSwitch = true;
        }
    }



    [CustomEditor(typeof(PortalSwitch))]
    public class MyScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PortalSwitch portalSwitch = target as PortalSwitch;

            portalSwitch.automatic = GUILayout.Toggle(portalSwitch.automatic, "Automatic");

            if (!portalSwitch.automatic)
            {

            } else
            {
                GUILayout.TextArea("Settings taken from PortalSurface");
            }

        }
    }
}