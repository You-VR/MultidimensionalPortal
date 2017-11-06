using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Vonderportal
{
    public class TrackedObject : MonoBehaviour
    {
        public enum VonderTrackedObject
        {
            Tracker_A,
            Tracker_B,
            Tracker_C,
            Tracker_D,
            Tracker_E,
            Tracker_F,
            Tracker_G,
        }

        public VonderTrackedObject trackedObjectIndex;        
        public bool isAllocated { get; private set; }

        const int requiredMeasurements = 50;
        int measurements = 0;

        SteamVR_TrackedObject steamVR_TrackedObject;

        SteamVR_Events.Action newPosesAction;

        Dictionary<SteamVR_TrackedObject.EIndex, List<float>> distances;

        void OnEnable()
        {
            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
            newPosesAction.enabled = true;
        }

        // Use this for initialization
        void Awake()
        {
            distances = new Dictionary<SteamVR_TrackedObject.EIndex, List<float>>();
        }

        void Start()
        {
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            for( int i = 0; i < poses.Length; i++)
            {
                TrackedDevicePose_t trackedDevicePose = poses[i];
                SteamVR_TrackedObject.EIndex eIndex = (SteamVR_TrackedObject.EIndex)i;

                if (    trackedDevicePose.bDeviceIsConnected && 
                        trackedDevicePose.bPoseIsValid && 
                        eIndex != SteamVR_TrackedObject.EIndex.Hmd)
                {
                    var pose = new SteamVR_Utils.RigidTransform(trackedDevicePose.mDeviceToAbsoluteTracking);

                    float dist = Vector3.Distance(pose.pos, transform.position);

                    if (!distances.ContainsKey(eIndex))
                    {
                        distances.Add(eIndex, new List<float>() { dist });
                    } else
                    {
                        distances[eIndex].Add(dist);
                    }
                } else
                {

                }         
            }

            measurements++;

            if ( measurements >= requiredMeasurements)
            {
                newPosesAction.enabled = false;
                CreateTrackedObject();
            }
        }

        private void CreateTrackedObject()
        {
            var eIndecies = distances.Keys;
            float min = -1;
            SteamVR_TrackedObject.EIndex allocatedEIndex = SteamVR_TrackedObject.EIndex.None;


            foreach ( var eIndex in eIndecies)
            {
                float mean = distances[eIndex].Sum() / distances[eIndex].Count;

                if( min == -1)
                {
                    min = mean;
                    allocatedEIndex = eIndex;
                } else if (mean < min)
                {
                    min = mean;
                    allocatedEIndex = eIndex;
                }
   
            }

            steamVR_TrackedObject = this.gameObject.AddComponent<SteamVR_TrackedObject>();
            steamVR_TrackedObject.SetDeviceIndex((int)allocatedEIndex);
        }
    }
}
