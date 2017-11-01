using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vonderportal
{
    public class PortalRenderer : MonoBehaviour
    {

        private Portal portal { get { return GetComponentInParent<Portal>(); } }
        private Camera mainCamera { get { return portal.mainCamera; } }
        private Camera portalCam { get { return portal.portalCam; } }
        private bool triggerZDirection { get { return portal.triggerZDirection; } }

        private Renderer meshRenderer { get { return GetComponent<Renderer>(); } }
        private MeshFilter meshFilter { get { return GetComponent<MeshFilter>(); } }

        private RenderTexture leftTexture;
        private RenderTexture rightTexture;

        private float portalSwitchDistance = 0.03f;

        private void Awake()
        {
            Vector2 texSize = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);
            leftTexture = rightTexture = new RenderTexture((int)(texSize.x ), (int)(texSize.y), 16);
        }

        private void OnWillRenderObject()
        {
            Camera camera = Camera.current;
            //meshRenderer.material.SetFloat("_RecursiveRender", (gameObject.layer != Camera.current.gameObject.layer) ? 1 : 0);
            try
            {
                Debug.Log("Success");
                meshRenderer.material.SetFloat("_RecursiveRender", 1);
            }
            finally
            {
                Debug.Log("Failure");
            }


#if UNITY_EDITOR
            if (camera.name == "SceneCamera")
                return;
#endif

            if (camera.stereoEnabled)
            {
                //#if USES_STEAM_VR
                if (camera.stereoTargetEye == StereoTargetEyeMask.Both || camera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    Vector3 eyePos = camera.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
                    Quaternion eyeRot = camera.transform.rotation * SteamVR.instance.eyes[0].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(camera, Valve.VR.EVREye.Eye_Left);

                    RenderTexture target = leftTexture;

                    RenderPlane(portalCam, target, eyePos, eyeRot, projectionMatrix);
                    meshRenderer.material.SetTexture("_LeftTex", target);
                }

                if (camera.stereoTargetEye == StereoTargetEyeMask.Both || camera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    Vector3 eyePos = camera.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
                    Quaternion eyeRot = camera.transform.rotation * SteamVR.instance.eyes[1].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(camera, Valve.VR.EVREye.Eye_Right);

                    RenderTexture target = rightTexture;

                    RenderPlane(portalCam, target, eyePos, eyeRot, projectionMatrix);
                    meshRenderer.material.SetTexture("_RightTex", target);
                }
                //#endif
            }
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
        protected void RenderPlane(Camera portalCamera, RenderTexture targetTexture, Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix)
        {
            portalCam.targetTexture = targetTexture;

            // Change the project matrix to use oblique culling (only show things BEHIND the portal)
            Vector3 pos = transform.position;
            Vector3 normal = transform.forward;
            bool isForward = transform.InverseTransformPoint(portalCamera.transform.position).z < 0;
            Vector4 clipPlane = CameraSpacePlane(portalCamera, pos, normal, isForward ? 1.0f : -1.0f);
            Matrix4x4 projection = camProjectionMatrix;

            portalCamera.projectionMatrix = projection;

            // Hide the other dimensions
            portalCamera.enabled = false;
            portalCamera.cullingMask = 0;

            //CameraExtensions.LayerCullingShow(portalCamera, ToDimension().layer);
            //CameraExtensions.LayerCullingShowMask(portalCamera, alwaysVisibleMask);

            // Update values that are used to generate the Skybox and whatnot.


            portalCamera.Render();
        }
        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * portalSwitchDistance * (triggerZDirection ? -1 : 1);
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}
