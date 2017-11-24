using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CameraRigRegistration : MonoBehaviour {

    CVRChaperoneSetup chaperoneSetup;
    CVRChaperone chaperone;

    HmdQuad_t[] pCollisionQuadsBuffer1;
    Vector3[] vertices1;

    HmdQuad_t[] pCollisionQuadsBuffer2;
    Vector3[] vertices2;

    HmdQuad_t playArea;

    // Use this for initialization
    void Awake () {
        chaperoneSetup = OpenVR.ChaperoneSetup;
        chaperone = OpenVR.Chaperone;

    }

    void Update()
    {
        chaperone.GetPlayAreaRect(ref playArea);
        chaperoneSetup.GetLiveCollisionBoundsInfo( out pCollisionQuadsBuffer1);
        chaperoneSetup.GetWorkingCollisionBoundsInfo(   out pCollisionQuadsBuffer2);
        Debug.Log(" Working Col bounds: " + pCollisionQuadsBuffer2.Length.ToString());


        GetCollisionBoundsMesh();


        //chaperoneSetup.GetWorkingStandingZeroPoseToRawTrackingPose(ref pmatStandingZeroPoseToRawTrackingPose);
    }

    void GetCollisionBoundsMesh()
    {
        List<Vector3> _vertices = new List<Vector3>();
        foreach (HmdQuad_t quad in pCollisionQuadsBuffer1)
        {
            HmdVector3_t vCorners0 = quad.vCorners0;
            HmdVector3_t vCorners1 = quad.vCorners1;
            HmdVector3_t vCorners2 = quad.vCorners2;
            HmdVector3_t vCorners3 = quad.vCorners3;

            _vertices.Add( new Vector3(vCorners0.v0, vCorners0.v1, vCorners0.v2));
            _vertices.Add( new Vector3(vCorners1.v0, vCorners1.v1, vCorners1.v2));
            _vertices.Add( new Vector3(vCorners2.v0, vCorners2.v1, vCorners2.v2));
            _vertices.Add( new Vector3(vCorners3.v0, vCorners3.v1, vCorners3.v2));
        }
        vertices1 = _vertices.ToArray();

        _vertices.Clear();
        foreach (HmdQuad_t quad in pCollisionQuadsBuffer2)
        {
            HmdVector3_t vCorners0 = quad.vCorners0;
            HmdVector3_t vCorners1 = quad.vCorners1;
            HmdVector3_t vCorners2 = quad.vCorners2;
            HmdVector3_t vCorners3 = quad.vCorners3;

            _vertices.Add(new Vector3(vCorners0.v0, vCorners0.v1, vCorners0.v2));
            _vertices.Add(new Vector3(vCorners1.v0, vCorners1.v1, vCorners1.v2));
            _vertices.Add(new Vector3(vCorners2.v0, vCorners2.v1, vCorners2.v2));
            _vertices.Add(new Vector3(vCorners3.v0, vCorners3.v1, vCorners3.v2));
        }
        vertices2 = _vertices.ToArray();
    }


    private void OnDrawGizmos()
    {
        
        //for (int i = 0; i < vertices1.Length; i++)
        //{
        //    Gizmos.color = Color.Lerp(Color.red, Color.blue, (float)i / vertices1.Length) ;
        //    Gizmos.DrawSphere(vertices1[i], 0.1f);
        //}
        
        for (int i = 0; i < vertices2.Length; i++)
        {
            Gizmos.color = Color.Lerp(Color.green, Color.yellow, (float)i / vertices2.Length);
            Gizmos.DrawSphere(vertices2[i], 0.05f);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawLine(    new Vector3(playArea.vCorners0.v0, playArea.vCorners0.v1, playArea.vCorners0.v2),
                            new Vector3(playArea.vCorners1.v0, playArea.vCorners1.v1, playArea.vCorners1.v2));

        Gizmos.DrawLine(    new Vector3(playArea.vCorners1.v0, playArea.vCorners1.v1, playArea.vCorners1.v2),
                            new Vector3(playArea.vCorners2.v0, playArea.vCorners2.v1, playArea.vCorners2.v2));

        Gizmos.DrawLine(    new Vector3(playArea.vCorners2.v0, playArea.vCorners2.v1, playArea.vCorners2.v2),
                            new Vector3(playArea.vCorners3.v0, playArea.vCorners3.v1, playArea.vCorners3.v2));

        Gizmos.DrawLine(    new Vector3(playArea.vCorners3.v0, playArea.vCorners3.v1, playArea.vCorners3.v2),
                            new Vector3(playArea.vCorners0.v0, playArea.vCorners0.v1, playArea.vCorners0.v2));
    }

}
