using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vonderportal
{
    public class PortalSurface : Vonderportal.ActiveSurface
    {
        public SceneType toDimension;

        protected override string cameraName { get { return "Portal"; } }


        public float threshold = 0.3f;
        Vector3 faceDir;
        int gridSize = 20;
        Mesh mesh;
        Vector3[] originalVertices, displacedVertices;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            Bounds bounds = mesh.bounds;

            int size = 20;
            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            for (int i = 0, y = 0; y <= size; y++)
            {
                for (int x = 0; x <= size; x++, i++)
                {
                    float x_pos = (float)x / size - bounds.extents.x;
                    float y_pos = (float)y / size - bounds.extents.y;

                    vertices[i] = new Vector3(x_pos, y_pos, 0);
                }
            }

            int[] triangles = new int[size * size * 6];
            for (int ti = 0, vi = 0, y = 0; y < size; y++, vi++)
            {
                for (int x = 0; x < size; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + size + 1;
                    triangles[ti + 5] = vi + size + 2;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            originalVertices = vertices;
            displacedVertices = new Vector3[originalVertices.Length];
        }

        void Update()
        {
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 dist = mainCamera.transform.position - transform.TransformPoint(originalVertices[i]);
                bool isFree = (i % gridSize > 1) && (i > gridSize) && (i < originalVertices.Length - gridSize);


                if (dist.magnitude < threshold && isFree)
                {

                    displacedVertices[i] = originalVertices[i] + (1.0f / dist.magnitude * transform.InverseTransformDirection(transform.forward));
                }
                else
                {
                    displacedVertices[i] = originalVertices[i];
                }
            }
            mesh.vertices = displacedVertices;
        }




        protected override void SetSurfaceCamCullingMask() {
            surfaceCam.ResetCullingMatrix();
            surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.portalLayer);


            switch (toDimension)
            {
                case SceneType.last:
                    surfaceCam.cullingMask |= (1 << dimensionManager.activeDimensions.lastLayer);
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.currLayer);
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.nextLayer);
                    break;
                case SceneType.current:
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.lastLayer);
                    surfaceCam.cullingMask |= (1 << dimensionManager.activeDimensions.currLayer);
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.nextLayer);
                    break;
                case SceneType.next:
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.lastLayer);
                    surfaceCam.cullingMask &= ~(1 << dimensionManager.activeDimensions.currLayer);
                    surfaceCam.cullingMask |= (1 << dimensionManager.activeDimensions.nextLayer);
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

            Vector3 offsetPos = pos + normal * clipPlaneOffset;

            Matrix4x4 m = surfaceCam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }

}
