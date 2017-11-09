using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Valve.VR;

namespace Vonderportal
{
    public class TrackedObjectManager : MonoBehaviour
    {

        static TrackedObjectManager trackedObjectManagerInstance;

        public TrackedObject[] trackedObjects;

        private List<TrackedObject> _trackedObjects;

        public bool isAllocated { get; private set; }

        const int requiredMeasurements = 50;
        int measurements = 0;

        SteamVR_TrackedObject steamVR_TrackedObject;

        SteamVR_Events.Action newPosesAction;

        List<Dictionary<SteamVR_TrackedObject.EIndex, List<float>>> distances;

        TrackedDevicePose_t[] latestPoses;

        void Awake()
        {
            if (trackedObjectManagerInstance == null) { trackedObjectManagerInstance = this; }
            else
            {
                Debug.LogError("Only one instance of tracked object manager allowed");
            }

            
        }

        void Start()
        {
            _trackedObjects = new List<TrackedObject>();
            List<TrackedObject.VonderTrackedObject> trackedObjectIndexes = new List<TrackedObject.VonderTrackedObject>();
            foreach (TrackedObject trackedObject in trackedObjects)
            {
                if (trackedObjectIndexes.Contains(trackedObject.trackedObjectIndex))
                {
                    Destroy(trackedObject.gameObject);
                    Debug.LogWarning("Duplicate Tracked object Removed");
                }
                else
                {
                    trackedObjectIndexes.Add(trackedObject.trackedObjectIndex);
                    _trackedObjects.Add(trackedObject);
                }
            }


            distances = new List<Dictionary<SteamVR_TrackedObject.EIndex, List<float>>>(_trackedObjects.Count);
        }

        void OnEnable()
        {
            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
            newPosesAction.enabled = true;
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            latestPoses = poses;
            for (int to_i = 0; to_i < _trackedObjects.Count; to_i++)
            {
                if(distances.Count == to_i)
                {
                    distances.Add(new Dictionary<SteamVR_TrackedObject.EIndex, List<float>>());
                }
                addPoseTrackerDistance(to_i);
            }
            measurements++;

            if (measurements >= requiredMeasurements)
            {
                newPosesAction.enabled = false;
                CreateTrackedObjects();
            }
        }

        private void addPoseTrackerDistance(int trackerNum)
        {
            for (int po_i = 0; po_i < latestPoses.Length; po_i++)
            {
                TrackedDevicePose_t trackedDevicePose = latestPoses[po_i];
                SteamVR_TrackedObject.EIndex eIndex = (SteamVR_TrackedObject.EIndex)po_i;

                if (trackedDevicePose.bDeviceIsConnected &&
                        trackedDevicePose.bPoseIsValid &&
                        eIndex != SteamVR_TrackedObject.EIndex.Hmd)
                {
                    var pose = new SteamVR_Utils.RigidTransform(trackedDevicePose.mDeviceToAbsoluteTracking);
                    Vector3 trackedObjectOrigin = trackedObjects[trackerNum].gameObject.transform.position;



                    float dist = Vector3.Distance(pose.pos, trackedObjectOrigin);

                    if (!distances[trackerNum].ContainsKey(eIndex))
                    {
                        distances[trackerNum].Add(eIndex, new List<float>() { dist });
                    }
                    else
                    {
                        distances[trackerNum][eIndex].Add(dist);
                    }
                }
            }
        }

        private void CreateTrackedObjects()
        {

            // Get list of Tracked device IDs
            List<SteamVR_TrackedObject.EIndex> AvailableEIndecies = new List<SteamVR_TrackedObject.EIndex>();
            List<SteamVR_TrackedObject.EIndex> AllocatedEIndecies = new List<SteamVR_TrackedObject.EIndex>();

            foreach (Dictionary<SteamVR_TrackedObject.EIndex, List<float>> trackedObjectDistances in distances) {
                AvailableEIndecies = AvailableEIndecies.Union(trackedObjectDistances.Keys).ToList();
            }

            for(int to_i = 0; to_i < _trackedObjects.Count; to_i++) {
                Dictionary<SteamVR_TrackedObject.EIndex, List<float>> trackedObjectDistances = distances[to_i];
                float min = -1;
                SteamVR_TrackedObject.EIndex allocatedEIndex = SteamVR_TrackedObject.EIndex.None;


                foreach (var eIndex in AvailableEIndecies)
                {
                    float mean = trackedObjectDistances[eIndex].Sum() / trackedObjectDistances[eIndex].Count;

                    if (min == -1)
                    {
                        min = mean;
                        allocatedEIndex = eIndex;
                    }
                    else if (mean < min)
                    {
                        min = mean;
                        allocatedEIndex = eIndex;
                    }

                    Debug.Log("Tracker-" + to_i + " EIndex: " + eIndex + " Mean: " + mean);

                }
                

                if(AllocatedEIndecies.Contains(allocatedEIndex))
                {
                    Debug.LogWarning("Tracker " + allocatedEIndex + " allocated twice");
                }
                else
                {
                    AllocatedEIndecies.Add(allocatedEIndex);
                }

                steamVR_TrackedObject = _trackedObjects[to_i].gameObject.AddComponent<SteamVR_TrackedObject>();
                steamVR_TrackedObject.SetDeviceIndex((int)allocatedEIndex);
            }
        }
    }


    //[CustomEditor(typeof(TrackedObjectManager))]
    //public class TrackedObjectManagerEditor : Editor {

    //    SerializedObject trackedObjects;
    //    TrackedObjectManager trackedObjectManagerInstance;

    //    void OnEnable()
    //    {
    //        trackedObjects = new SerializedObject(target);
    //        trackedObjectManagerInstance = target as TrackedObjectManager;
    //    }

    //    public override void OnInspectorGUI()
    //    {
    //        trackedObjects.Update();

    //        EditorGUILayout.BeginVertical();

    //        foreach (TrackedObject trackedObject in trackedObjectManagerInstance.trackedObjects)
    //        {
    //            trackedObject.initialPosition = (Transform)EditorGUILayout.ObjectField("Tracked Object:", trackedObject.initialPosition, typeof(Transform), true);
    //        }

    //        EditorGUILayout.EndVertical();

    //        if (GUILayout.Button("Add point"))
    //        {
    //            trackedObjectManagerInstance.trackedObjects.Add(new TrackedObject());
    //        }

    //        if (GUI.changed)
    //        {
    //            EditorUtility.SetDirty(target);
    //            EditorUtility.SetDirty(trackedObjectManagerInstance);
    //        }

    //        trackedObjects.ApplyModifiedProperties();


    //    }
    //}

}
