using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vonderportal
{
    public class MirrorSurface : Vonderportal.ActiveSurface
    {

        protected override string cameraName { get { return "Mirror"; } }

        protected override void SetSurfaceCamCullingMask() {
            surfaceCam.ResetCullingMatrix();
            surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.portalLayer);

            surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.lastLayer);
            surfaceCam.cullingMask |=  (1 << dimensionManager.activeDimensions.currLayer);
            surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.nextLayer);

        }


        protected override void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix, Matrix4x4 worldToCameraMatrix) {
            Vector3 pos = transform.position;
            Vector3 normal = -transform.forward;

            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            Vector3 newpos = reflection.MultiplyPoint(eyePosition);

            surfaceCam.worldToCameraMatrix = worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane();
            //Matrix4x4 projection = cam.projectionMatrix;
            CalculateObliqueMatrix(ref camProjectionMatrix, clipPlane);

            surfaceCam.projectionMatrix = camProjectionMatrix;

            surfaceCam.transform.position = newpos;
            Vector3 euler = eyeRotation.eulerAngles;
            surfaceCam.transform.localEulerAngles = new Vector3(0, euler.y, euler.z);

            surfaceCam.ResetWorldToCameraMatrix();
        }


        public Vector4 CameraSpacePlane()
        {
            Vector3 pos = transform.position;
            Vector3 normal = -transform.forward;
            float sideSign = 1.0f;

            Vector3 offsetPos = pos + normal * clipPlaneOffset;

            Matrix4x4 m = surfaceCam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1.0f - 2.0f * plane[0] * plane[0]);
            reflectionMat.m01 = (-2.0f * plane[0] * plane[1]);
            reflectionMat.m02 = (-2.0f * plane[0] * plane[2]);
            reflectionMat.m03 = (-2.0f * plane[3] * plane[0]);

            reflectionMat.m10 = (-2.0f * plane[1] * plane[0]);
            reflectionMat.m11 = (1.0f - 2.0f * plane[1] * plane[1]);
            reflectionMat.m12 = (-2.0f * plane[1] * plane[2]);
            reflectionMat.m13 = (-2.0f * plane[3] * plane[1]);

            reflectionMat.m20 = (-2.0f * plane[2] * plane[0]);
            reflectionMat.m21 = (-2.0f * plane[2] * plane[1]);
            reflectionMat.m22 = (1.0f - 2.0f * plane[2] * plane[2]);
            reflectionMat.m23 = (-2.0f * plane[3] * plane[2]);

            reflectionMat.m30 = 0.0f;
            reflectionMat.m31 = 0.0f;
            reflectionMat.m32 = 0.0f;
            reflectionMat.m33 = 1.0f;
        }


    }

}
