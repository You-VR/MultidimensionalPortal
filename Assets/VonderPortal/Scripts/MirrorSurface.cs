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

            Vector3 posInPlane = transform.InverseTransformPoint(eyePosition);
            Vector3 newPos = new Vector3(posInPlane.x, -posInPlane.y, posInPlane.z);
            newPos = transform.TransformPoint(newPos);
            surfaceCam.transform.position = newPos;

            Vector3 euler = eyeRotation.eulerAngles;
            //surfaceCam.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

            surfaceCam.ResetWorldToCameraMatrix();
        }


    }

}
