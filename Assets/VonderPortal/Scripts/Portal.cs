using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vonderportal
{
    public class Portal : MonoBehaviour
    {
        [Tooltip("The Main Camera. On Vive this is [CameraRig] -> Camera (Head) -> Camera (Eye)")]
        public Camera mainCamera;

        public PortalRenderer portalPlane;
        public Camera portalCam { get; private set; }

        public bool triggerZDirection { get; private set; }


        void Awake()
        {


            //Create Portal Camera
            portalCam = Instantiate(mainCamera);
            portalCam.name = "Portal Camera";
            portalCam.transform.parent = this.transform;

            // Get rid of extra components
            if (portalCam.GetComponent<AudioListener>())
            {
                Destroy(portalCam.GetComponent<AudioListener>());
            }
            if (portalCam.GetComponent<FlareLayer>())
            {
                Destroy(portalCam.GetComponent<FlareLayer>());
            }
            if (portalCam.GetComponent<GUILayer>())
            {
                Destroy(portalCam.GetComponent<GUILayer>());
            }

            portalCam.enabled = false;
        }
        private void Start()
        {
            Vector3 convertedPoint = transform.InverseTransformPoint(mainCamera.transform.position);
            triggerZDirection = (convertedPoint.z > 0);
        }

        void Update()
        {
            // Copy camera position/rotation/projection data into the reflectionCamera
            portalCam.transform.position = mainCamera.transform.position;
            portalCam.transform.rotation = mainCamera.transform.rotation;
            portalCam.ResetWorldToCameraMatrix();

            portalCam.farClipPlane = mainCamera.farClipPlane;
            portalCam.nearClipPlane = mainCamera.nearClipPlane;
            portalCam.orthographic = mainCamera.orthographic;
            portalCam.fieldOfView = mainCamera.fieldOfView;
            portalCam.aspect = mainCamera.aspect;
            portalCam.orthographicSize = mainCamera.orthographicSize;
        }


    }
}