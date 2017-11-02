using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vonderportal
{
    public class ActiveSurface : MonoBehaviour
    {
        public Camera mainCamera;
        public bool deform;
        public bool useObliqueCulling;


        // Private Variables
        protected Camera surfaceCam;

        private Renderer meshRenderer { get { return GetComponent<Renderer>(); } }
        private MeshFilter meshFilter { get { return GetComponent<MeshFilter>(); } }

        public bool triggerZDirection;

        private RenderTexture leftTexture;
        private RenderTexture rightTexture;

        public float portalSwitchDistance = 0.03f;

        protected virtual string cameraName { get;  }

        //*************************************************************************************************************************//
        // VIRTUAL FUNCTIONS                                                                                                       //
        //*************************************************************************************************************************//
        protected virtual void SetSurfaceCamCullingMask() { }
        protected virtual void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation) { }

        //*************************************************************************************************************************//
        // PRIVATE FUNCTIONS                                                                                                       //
        //*************************************************************************************************************************//

        private void Awake()
        {
            surfaceCam = CreateCamera();
        }


        private void Start()
        {
            this.gameObject.layer = LayerMask.NameToLayer("Portal");

            //Init Textures
            Vector2 texSize = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);
            leftTexture = new RenderTexture((int)(texSize.x ), (int)(texSize.y), 16);
            rightTexture = new RenderTexture((int)(texSize.x), (int)(texSize.y), 16);

            //Set Trigger Z Direction
            Vector3 convertedPoint = this.transform.InverseTransformPoint(mainCamera.transform.position);
            triggerZDirection = (convertedPoint.z > 0);
        }

        private void OnWillRenderObject()
        {
            Camera currentCamera = Camera.current;


            // Set camera clipping plane
            Vector3 deltaTransform = transform.position - currentCamera.transform.position;
            surfaceCam.nearClipPlane = Mathf.Max(deltaTransform.magnitude - meshRenderer.bounds.size.magnitude, 0.01f);

            if (currentCamera.name == "SceneCamera")
                return;

            if (currentCamera.stereoEnabled)
            {
                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    Vector3 eyePos = currentCamera.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
                    Quaternion eyeRot = currentCamera.transform.rotation * SteamVR.instance.eyes[0].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(currentCamera, Valve.VR.EVREye.Eye_Left);

                    RenderPlane(leftTexture, eyePos, eyeRot, projectionMatrix);
                    meshRenderer.material.SetTexture("_LeftTex", leftTexture);
                }

                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    Vector3 eyePos = currentCamera.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
                    Quaternion eyeRot = currentCamera.transform.rotation * SteamVR.instance.eyes[1].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(currentCamera, Valve.VR.EVREye.Eye_Right);

                    RenderPlane(rightTexture, eyePos, eyeRot, projectionMatrix);
                    meshRenderer.material.SetTexture("_RightTex", rightTexture);
                }
            } else {
                RenderTexture target = leftTexture;
                RenderPlane(target, currentCamera.transform.position, currentCamera.transform.rotation, currentCamera.projectionMatrix);
                meshRenderer.material.SetTexture("_LeftTex", target);
                meshRenderer.material.SetFloat("_RecursiveRender", 1);  // Using Recursive render here will force the shader to only read from the LeftTex texture
            }
        }
        private Camera CreateCamera()
        {
            if(surfaceCam == null)
            {
                //Create Portal Camera
                Camera newCam = Instantiate(mainCamera);
                newCam.name = cameraName + "_" + gameObject.name;
                newCam.transform.parent = this.transform;

                // Get rid of extra components
                if (newCam.GetComponent<AudioListener>())
                {
                    Destroy(newCam.GetComponent<AudioListener>());
                }
                if (newCam.GetComponent<FlareLayer>())
                {
                    Destroy(newCam.GetComponent<FlareLayer>());
                }
                if (newCam.GetComponent<GUILayer>())
                {
                    Destroy(newCam.GetComponent<GUILayer>());
                }
                foreach (Transform child in newCam.transform)
                {
                    Destroy(child.gameObject);
                }

                newCam.enabled = false;
                return newCam;
            } else
            {
                return surfaceCam;
            }
        }

        private void RenderPlane(RenderTexture targetTexture, Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix)
        {
            surfaceCam.targetTexture = targetTexture;

            // Copy camera position/rotation/projection data into the reflectionCamera
            SetSurfaceCamPosition(eyePosition, eyeRotation);

            // Copy camera position/rotation/projection data into the reflectionCamera
            SetSurfaceCamCullingMask();

            if (useObliqueCulling)
            {
                // Change the project matrix to use oblique culling (only show things BEHIND the portal)
                Vector4 clipPlane = CameraSpacePlane();

                CalculateObliqueMatrix(ref camProjectionMatrix, clipPlane);
                surfaceCam.projectionMatrix = camProjectionMatrix;
            }
            else
            {
                surfaceCam.projectionMatrix = camProjectionMatrix;
            }

        
            // Update values that are used to generate the Skybox and whatnot.
            surfaceCam.farClipPlane = mainCamera.farClipPlane;
            surfaceCam.nearClipPlane = mainCamera.nearClipPlane;
            surfaceCam.orthographic = mainCamera.orthographic;
            surfaceCam.fieldOfView = mainCamera.fieldOfView;
            surfaceCam.aspect = mainCamera.aspect;
            surfaceCam.orthographicSize = mainCamera.orthographicSize;

            surfaceCam.Render();
        }


        // Given position/normal of the plane, calculates plane in camera space.
        public Vector4 CameraSpacePlane()
        {
            Vector3 pos = transform.position;
            Vector3 normal = transform.forward;
            float sideSign = (transform.InverseTransformPoint(surfaceCam.transform.position).z < 0) ? 1.0f : -1.0f;

            Vector3 offsetPos = pos + normal * portalSwitchDistance * (triggerZDirection ? -1 : 1);
            Matrix4x4 m = surfaceCam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        //*************************************************************************************************************************//
        // STATIC FUNCTIONS                                                                                                        //
        //*************************************************************************************************************************//

        public static Matrix4x4 GetSteamVRProjectionMatrix(Camera cam, Valve.VR.EVREye eye)
        {
            Valve.VR.HmdMatrix44_t proj = SteamVR.instance.hmd.GetProjectionMatrix(eye, cam.nearClipPlane, cam.farClipPlane);
            Matrix4x4 m = new Matrix4x4();
            m.m00 = proj.m0;
            m.m01 = proj.m1;
            m.m02 = proj.m2;
            m.m03 = proj.m3;
            m.m10 = proj.m4;
            m.m11 = proj.m5;
            m.m12 = proj.m6;
            m.m13 = proj.m7;
            m.m20 = proj.m8;
            m.m21 = proj.m9;
            m.m22 = proj.m10;
            m.m23 = proj.m11;
            m.m30 = proj.m12;
            m.m31 = proj.m13;
            m.m32 = proj.m14;
            m.m33 = proj.m15;
            return m;
        }
        private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(
                sgn(clipPlane.x),
                sgn(clipPlane.y),
                1.0f,
                1.0f
            );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

            // third row = clip plane - fourth row
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];
        }
        // Extended sign: returns -1, 0 or 1 based on sign of a
        private static float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }
    }
}
