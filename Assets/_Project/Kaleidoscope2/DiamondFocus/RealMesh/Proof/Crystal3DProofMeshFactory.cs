using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.Proof
{
    public static class Crystal3DProofMeshFactory
    {
        public const int SegmentCount = 32;
        public const float MinimumValidDepth = 0.5f;

        public static Mesh CreateCrystalMesh()
        {
            List<Vector3> vertices = new List<Vector3>(SegmentCount * 18);
            List<int> triangles = new List<int>(SegmentCount * 18);

            Vector3 topApex = new Vector3(0f, 1.18f, 0f);
            Vector3 bottomApex = new Vector3(0f, -1.18f, 0f);

            Vector3[] crownRing = CreateRing(0.56f, 0.62f, 0.5f, 0f);
            Vector3[] girdleRing = CreateRing(1.06f, 0.92f, 0f, Mathf.PI / SegmentCount);
            Vector3[] pavilionRing = CreateRing(0.58f, 0.54f, -0.54f, 0f);

            for (int index = 0; index < SegmentCount; index++)
            {
                int next = (index + 1) % SegmentCount;
                AddTriangle(vertices, triangles, topApex, crownRing[index], crownRing[next]);
                AddFacetFace(vertices, triangles, crownRing[index], girdleRing[index], girdleRing[next], crownRing[next]);
                AddFacetFace(vertices, triangles, girdleRing[index], pavilionRing[index], pavilionRing[next], girdleRing[next]);
                AddTriangle(vertices, triangles, pavilionRing[index], bottomApex, pavilionRing[next]);
            }

            Mesh mesh = new Mesh
            {
                name = "Crystal3D_Proof_ProceduralClosedFacetedMesh"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        public static bool HasVolume(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            Vector3 size = mesh.bounds.size;
            return size.x > MinimumValidDepth
                && size.y > MinimumValidDepth
                && size.z > MinimumValidDepth;
        }

        public static bool DepthIsValid(Mesh mesh)
        {
            return mesh != null && mesh.bounds.size.z > MinimumValidDepth;
        }

        private static Vector3[] CreateRing(float radiusX, float radiusZ, float height, float phaseOffset)
        {
            Vector3[] ring = new Vector3[SegmentCount];
            for (int index = 0; index < SegmentCount; index++)
            {
                float angle = (Mathf.PI * 2f * index / SegmentCount) + phaseOffset;
                float facetPulse = index % 2 == 0 ? 1f : 0.92f;
                ring[index] = new Vector3(
                    Mathf.Cos(angle) * radiusX * facetPulse,
                    height,
                    Mathf.Sin(angle) * radiusZ * facetPulse);
            }

            return ring;
        }

        private static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        private static void AddFacetFace(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);

            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }
    }
}
