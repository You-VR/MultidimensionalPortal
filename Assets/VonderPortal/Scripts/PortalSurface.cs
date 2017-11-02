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
                    surfaceCam.cullingMask &= (1 << LayerMask.NameToLayer("CurrentScene"));
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


        protected override void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation) {
            surfaceCam.transform.position = eyePosition;
            surfaceCam.transform.rotation = eyeRotation;
            surfaceCam.ResetWorldToCameraMatrix();
        }


    }

}
