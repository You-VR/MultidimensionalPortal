using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        void OnDrawGizmos()
        {
            string iconName;
            switch (trackedObjectIndex)
            {
                case VonderTrackedObject.Tracker_A:
                    iconName = "Icons/ViveTrackerIcon_A.tif";
                    break;
                case VonderTrackedObject.Tracker_B:
                    iconName = "Icons/ViveTrackerIcon_B.tif";
                    break;
                case VonderTrackedObject.Tracker_C:
                    iconName = "Icons/ViveTrackerIcon_C.tif";
                    break;
                case VonderTrackedObject.Tracker_D:
                    iconName = "Icons/ViveTrackerIcon_D.tif";
                    break;
                case VonderTrackedObject.Tracker_E:
                    iconName = "Icons/ViveTrackerIcon_E.tif";
                    break;
                case VonderTrackedObject.Tracker_F:
                    iconName = "Icons/ViveTrackerIcon_F.tif";
                    break;
                default:
                    iconName = "Icons/ViveTrackerIcon.tif";
                    break;

            }

            Gizmos.DrawIcon(transform.position, iconName, false);
        }
    }
}
