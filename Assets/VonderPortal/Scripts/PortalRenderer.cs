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

        private void Start()
        {
            this.gameObject.layer = LayerMask.NameToLayer("Portal");

            Vector2 texSize = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);

            leftTexture = new RenderTexture((int)(texSize.x ), (int)(texSize.y), 16);
            rightTexture = new RenderTexture((int)(texSize.x), (int)(texSize.y), 16);
        }

        private void OnWillRenderObject()
        {
            Camera camera = Camera.current;
            //meshRenderer.material.SetFloat("_RecursiveRender", (Random.value < 0.5) ? 1: 0);

            //Vector3 deltaTransform = transform.position - camera.transform.position;
            //portalCam.nearClipPlane = Mathf.Max(deltaTransform.magnitude - meshRenderer.bounds.size.magnitude, 0.01f);

            if (camera.name == "SceneCamera")
                return;

            if (camera.stereoEnabled)
            {
                if (camera.stereoTargetEye == StereoTargetEyeMask.Both || camera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    Debug.Log("Left");
                    Vector3 eyePos = camera.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
                    Quaternion eyeRot = camera.transform.rotation * SteamVR.instance.eyes[0].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(camera, Valve.VR.EVREye.Eye_Left);

                    RenderPlane(leftTexture, eyePos, eyeRot, projectionMatrix);
                    

                    meshRenderer.material.SetTexture("_LeftTex", leftTexture);
                }

                if (camera.stereoTargetEye == StereoTargetEyeMask.Both || camera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    Vector3 eyePos = camera.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
                    Quaternion eyeRot = camera.transform.rotation * SteamVR.instance.eyes[1].rot;
                    Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(camera, Valve.VR.EVREye.Eye_Right);

                    RenderPlane(rightTexture, eyePos, eyeRot, projectionMatrix);
                    meshRenderer.material.SetTexture("_RightTex", rightTexture);
                }
            }
            //} else
            //{
            //    Debug.Log("Other");
            //    RenderTexture target = leftTexture;
            //    RenderPlane(target, camera.transform.position, camera.transform.rotation, camera.projectionMatrix);
            //    meshRenderer.material.SetTexture("_LeftTex", target);
            //    meshRenderer.material.SetFloat("_RecursiveRender", 1);  // Using Recursive render here will force the shader to only read from the LeftTex texture
            //}
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
        protected void RenderPlane(RenderTexture targetTexture, Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix)
        {
            portalCam.targetTexture = targetTexture;

            // Copy camera position/rotation/projection data into the reflectionCamera
            portalCam.transform.position = eyePosition;
            portalCam.transform.rotation = eyeRotation;
            portalCam.ResetWorldToCameraMatrix();



            portalCam.enabled = false;
            portalCam.ResetCullingMatrix();
            portalCam.cullingMask &= ~(1 << LayerMask.NameToLayer("Portal"));
            portalCam.cullingMask &= ~(1 << LayerMask.NameToLayer("CurrentScene"));

            Matrix4x4 projection = camProjectionMatrix;

            // Change the project matrix to use oblique culling (only show things BEHIND the portal)
            //Vector3 pos = transform.position;
            //Vector3 normal = transform.forward;
            //bool isForward = transform.InverseTransformPoint(portalCam.transform.position).z < 0;
            //Vector4 clipPlane = CameraSpacePlane(portalCam, pos, normal, isForward ? 1.0f : -1.0f);
  
            //CalculateObliqueMatrix(ref projection, clipPlane);


            portalCam.projectionMatrix = projection;

            // Hide the other dimensions
            //portalCamera.enabled = false;
            //portalCamera.cullingMask = 0;

            //CameraExtensions.LayerCullingShow(portalCamera, ToDimension().layer);
            //CameraExtensions.LayerCullingShowMask(portalCamera, alwaysVisibleMask);

            // Update values that are used to generate the Skybox and whatnot.
            portalCam.farClipPlane = mainCamera.farClipPlane;
            portalCam.nearClipPlane = mainCamera.nearClipPlane;
            portalCam.orthographic = mainCamera.orthographic;
            portalCam.fieldOfView = mainCamera.fieldOfView;
            portalCam.aspect = mainCamera.aspect;
            portalCam.orthographicSize = mainCamera.orthographicSize;

            portalCam.Render();
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
