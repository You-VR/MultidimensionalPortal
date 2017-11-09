using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Vonderportal
{
    public class PortalSwitch : MonoBehaviour
    {
        public bool allowSwitch = true;

        public bool automatic = true;

        private DimensionManager dimensionManager { get { return DimensionManager.dimensionManagerInstance; } }
        private Camera mainCamera
        {
            get
            {
                if (dimensionManager != null)
                {
                    return dimensionManager.mainCamera;
                }
                else
                {
                    return portalSurface.mainCamera;
                }
            }
        }

        private PortalSurface portalSurface { get { return GetComponent<PortalSurface>(); } }
        private SceneType toDimension;

        private BoxCollider triggerCollider;

        void Awake()
        {
            if (automatic)
            {
                toDimension = portalSurface.toDimension;
                toDimension = portalSurface.toDimension;
            }         
        }
        private void OnDisable()
        {
            allowSwitch = false;
        }
        void OnEnable()
        {
            StartCoroutine(switchTimeOutCoroutine());
        }

        IEnumerator switchTimeOutCoroutine()
        {
            yield return new WaitForSeconds(3.0f);
            allowSwitch = true;
        }

        private void Start()
        {
            triggerCollider = this.gameObject.AddComponent<BoxCollider>();
            triggerCollider.size = new Vector3(1,1, 0.2f);
            triggerCollider.center = new Vector3(0, 0, 0.1f);
        }

        void Update()
        {
            Vector3 convertedPoint = portalSurface.transform.InverseTransformPoint(mainCamera.transform.position);

            if ((convertedPoint.z > 0) != portalSurface.triggerZDirection && Mathf.Abs(convertedPoint.z) > portalSurface.clipPlaneOffset && triggerCollider.bounds.Contains(mainCamera.transform.position))
            {
                if (allowSwitch && dimensionManager != null && portalSurface.active)
                {
                    dimensionManager.ChangeDimension(toDimension);                
                }
            }
        }
    }

    [CustomEditor(typeof(PortalSwitch))]
    public class PortalSwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PortalSwitch portalSwitch = target as PortalSwitch;

            portalSwitch.automatic = GUILayout.Toggle(portalSwitch.automatic, "Automatic");

            if (!portalSwitch.automatic)
            {
                //TO-DO provide manual portal switch settings
            } else
            {
                GUILayout.TextArea("Settings taken from PortalSurface");
            }

        }
    }
}