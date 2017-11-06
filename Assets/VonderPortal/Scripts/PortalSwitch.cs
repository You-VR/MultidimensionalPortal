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
        private Camera mainCamera { get { return dimensionManager.mainCamera; } }

        private PortalSurface portalSurface { get { return GetComponent<PortalSurface>(); } }
        private SceneType toDimension;

        private BoxCollider collider;

        void Awake()
        {
            if (automatic)
            {
                toDimension = portalSurface.toDimension;
                toDimension = portalSurface.toDimension;
            }
        }

        private void Start()
        {
            collider = this.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1,1, 0.2f);
            collider.center = new Vector3(0, 0, 0.1f);

        }

        void Update()
        {
            Vector3 convertedPoint = portalSurface.transform.InverseTransformPoint(mainCamera.transform.position);

            if ((convertedPoint.z > 0) != portalSurface.triggerZDirection && Mathf.Abs(convertedPoint.z) > portalSurface.portalSwitchDistance && collider.bounds.Contains(mainCamera.transform.position))
            {
                if (allowSwitch)
                {
                    dimensionManager.ChangeDimension(toDimension);
                
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

            if (!portalSwitch.automatic)
            {

            } else
            {
                GUILayout.TextArea("Settings taken from PortalSurface");
            }

        }
    }
}