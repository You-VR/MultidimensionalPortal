using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace Vonderportal
{
    public enum PortalType { incoming, outgoing }

    public class Portal : MonoBehaviour
    {
        [Tooltip("The Main Camera. On Vive this is [CameraRig] -> Camera (Head) -> Camera (Eye)")]
        public Camera mainCamera;
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
            foreach (Transform child in portalCam.transform)
            {
                Destroy(child.gameObject);
            }

            portalCam.enabled = false;

            Shader.SetGlobalInt("OpenVRRender", 0);
        }
        private void Start()
        {
            Vector3 convertedPoint = transform.InverseTransformPoint(mainCamera.transform.position);
            triggerZDirection = (convertedPoint.z > 0);
        }

        void Update()
        {


        }


    }
}