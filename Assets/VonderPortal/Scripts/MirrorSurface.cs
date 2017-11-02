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
            surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("Portal"));

            surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("LastScene"));
            surfaceCam.cullingMask |=  (1 << LayerMask.NameToLayer("CurrentScene"));
            surfaceCam.cullingMask &= ~(1 << LayerMask.NameToLayer("NextScene"));

        }


        protected override void SetSurfaceCamPosition(Vector3 eyePosition, Quaternion eyeRotation) {
            Vector3 normal = transform.forward;
            Vector3 pos = transform.position;

            surfaceCam.transform.position = Vector3.Reflect(eyePosition, normal); 
            surfaceCam.transform.rotation = Quaternion.Inverse(eyeRotation);
            surfaceCam.ResetWorldToCameraMatrix();
        }


    }

}
