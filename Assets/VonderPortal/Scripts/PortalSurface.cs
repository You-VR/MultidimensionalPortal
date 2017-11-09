using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vonderportal
{
    public class PortalSurface : Vonderportal.ActiveSurface
    {
        public SceneType toDimension;

        protected override string cameraName { get { return "Portal"; } }


        protected override void SetSurfaceCamCullingMask() {
            surfaceCam.ResetCullingMatrix();
            surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("Portal"));


            switch (toDimension)
            {
                case SceneType.last:
                    surfaceCam.cullingMask |= (1 << LayerMask.NameToLayer("LastScene"));
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("CurrentScene"));
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("NextScene"));
                    break;
                case SceneType.current:
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("LastScene"));
                    surfaceCam.cullingMask |=  (1 << LayerMask.NameToLayer("CurrentScene"));
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("NextScene"));
                    break;
                case SceneType.next:
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("LastScene"));
                    surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("CurrentScene"));
                    surfaceCam.cullingMask |=  (1 << LayerMask.NameToLayer("NextScene"));
                    break;
            }
        }


        protected override void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation, Matrix4x4 camProjectionMatrix, Matrix4x4 worldToCameraMatrix) {
            surfaceCam.transform.position = eyePosition;
            surfaceCam.transform.rotation = eyeRotation;


            // Change the project matrix to use oblique culling (only show things BEHIND the portal)
            Vector4 clipPlane = CameraSpacePlane();
            CalculateObliqueMatrix(ref camProjectionMatrix, clipPlane);

            surfaceCam.projectionMatrix = camProjectionMatrix;

            surfaceCam.ResetWorldToCameraMatrix();

        }

        public Vector4 CameraSpacePlane()
        {
            Vector3 pos = transform.position;
            Vector3 normal = transform.forward;
            float sideSign = (transform.InverseTransformPoint(surfaceCam.transform.position).z < 0) ? 1.0f : -1.0f;

            Vector3 offsetPos = pos + normal * clipPlaneOffset * (triggerZDirection ? -1 : 1);

            Matrix4x4 m = surfaceCam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }

}
