using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Vonderportal
{
    public class ActiveSurface : MonoBehaviour
    {

        protected DimensionManager dimensionManager { get { return DimensionManager.dimensionManagerInstance; } }
        public Camera mainCamera { get
            {
                if (dimensionManager != null) {
                    return dimensionManager.mainCamera;
                }
                else
                {
                    return _mainCamera;
                }
            }
        }

        [SerializeField]
        public Camera _mainCamera;


        public bool active {
            get { return _isActive; }
            set {
                if (value) {
                    meshRenderer.enabled = true;
                    _isActive = value;

                } else {
                    meshRenderer.enabled = false;
                    _isActive = value;
                }
            }
        }
        private bool _isActive = true;

        public float clipPlaneOffset = 0.1f;
        [HideInInspector]
        public Vector3 triggerZDirection;


        // Private Variables
        protected Camera surfaceCam;
        protected virtual string cameraName { get; set; }

        private Renderer meshRenderer { get { return GetComponent<Renderer>(); } }
        private MeshFilter meshFilter { get { return GetComponent<MeshFilter>(); } }

        private RenderTexture leftTexture;
        private RenderTexture rightTexture;

        //*************************************************************************************************************************//
        // VIRTUAL FUNCTIONS                                                                                                       //
        //*************************************************************************************************************************//
        protected virtual void SetSurfaceCamCullingMask() { }
        protected virtual void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix, Matrix4x4 worldToCameraMatrix) { }

        //*************************************************************************************************************************//
        // PRIVATE FUNCTIONS                                                                                                       //
        //*************************************************************************************************************************//

        private void Start()
        {

            surfaceCam = CreateCamera();

            this.gameObject.layer = dimensionManager.activeDimensions.portalLayer;

            //Init Textures
            Vector2 texSize = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);
            leftTexture = new RenderTexture((int)(texSize.x ), (int)(texSize.y), 16);
            rightTexture = new RenderTexture((int)(texSize.x), (int)(texSize.y), 16);

            setTriggerZDirection();

        }
        public void setTriggerZDirection()
        {
            triggerZDirection = this.transform.forward;
        }

        private void OnWillRenderObject()
        {
            Camera currentCamera = Camera.current;
            
            if (currentCamera.name == "SceneCamera")
                return;

            Matrix4x4 worldToCameraMatrix = currentCamera.worldToCameraMatrix;

            if (currentCamera.stereoEnabled)
            {
                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    Vector3 eyePos = currentCamera.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
                    Quaternion eyeRot = currentCamera.transform.rotation * SteamVR.instance.eyes[0].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(currentCamera, Valve.VR.EVREye.Eye_Left);
                    


                    RenderPlane(leftTexture, eyePos, eyeRot, projectionMatrix, worldToCameraMatrix);
                    meshRenderer.material.SetTexture("_LeftTex", leftTexture);
                }

                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    Vector3 eyePos = currentCamera.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
                    Quaternion eyeRot = currentCamera.transform.rotation * SteamVR.instance.eyes[1].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(currentCamera, Valve.VR.EVREye.Eye_Right);

                    RenderPlane(rightTexture, eyePos, eyeRot, projectionMatrix, worldToCameraMatrix);
                    meshRenderer.material.SetTexture("_RightTex", rightTexture);
                }
            } else {
                RenderTexture target = leftTexture;
                RenderPlane(target, currentCamera.transform.position, currentCamera.transform.rotation, currentCamera.projectionMatrix, worldToCameraMatrix);
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
                newCam.name = cameraName + "_camera" ;
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

        private void RenderPlane(RenderTexture targetTexture, Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix, Matrix4x4 worldToCameraMatrix)
        {
            // Set camera clipping plane
            Vector3 deltaTransform = transform.position - eyePosition;
            surfaceCam.nearClipPlane = Mathf.Max(deltaTransform.magnitude - meshRenderer.bounds.size.magnitude, 0.01f);

            surfaceCam.targetTexture = targetTexture;

            surfaceCam.farClipPlane = mainCamera.farClipPlane;
            surfaceCam.nearClipPlane = mainCamera.nearClipPlane;
            surfaceCam.orthographic = mainCamera.orthographic;
            surfaceCam.fieldOfView = mainCamera.fieldOfView;
            surfaceCam.aspect = mainCamera.aspect;
            surfaceCam.orthographicSize = mainCamera.orthographicSize;


            SetSurfaceCamPosition(eyePosition, eyeRotation, camProjectionMatrix, worldToCameraMatrix);

            SetSurfaceCamCullingMask();

            surfaceCam.Render();
            
        }


        //*************************************************************************************************************************//
        // STATIC FUNCTIONS                                                                                                        //
        //*************************************************************************************************************************//
        // Given position/normal of the plane, calculates plane in camera space.


        public static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
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

        // Extended sign: returns -1, 0 or 1 based on sign of a
        protected static float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }
    }

    [CustomEditor(typeof(ActiveSurface), true)]
    public class ActiveSurfaceEditor : Editor
    {
        SerializedProperty m_mainCamera;

        protected virtual void OnEnable()
        {
            m_mainCamera = this.serializedObject.FindProperty("_mainCamera");
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            if (DimensionManager.dimensionManagerInstance == null)
            {
                EditorGUILayout.ObjectField(m_mainCamera);
            }
            else
            {
                GUILayout.TextArea("Main  Camera allocated automatically from DimensionManager");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
