using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public static class RealCrystalVolumetricMeshFactory
    {
        public const int SegmentCount = 32;
        public const float MinimumValidDepth = 0.5f;

        public static Mesh CreateMesh(CrystalShape shape)
        {
            List<Vector3> vertices = new List<Vector3>(SegmentCount * 18);
            List<Vector2> uvs = new List<Vector2>(SegmentCount * 18);
            List<int> triangles = new List<int>(SegmentCount * 18);

            CrystalProfile profile = ResolveProfile(shape);
            Vector3 topApex = new Vector3(0f, profile.TopY, 0f);
            Vector3 bottomApex = new Vector3(0f, profile.BottomY, 0f);
            Vector3[] crownRing = CreateRing(profile.CrownRadiusX, profile.CrownRadiusZ, profile.CrownY, 0f, profile.FacetPulse);
            Vector3[] girdleRing = CreateRing(profile.GirdleRadiusX, profile.GirdleRadiusZ, profile.GirdleY, Mathf.PI / SegmentCount, profile.FacetPulse);
            Vector3[] pavilionRing = CreateRing(profile.PavilionRadiusX, profile.PavilionRadiusZ, profile.PavilionY, 0f, profile.FacetPulse);

            for (int index = 0; index < SegmentCount; index++)
            {
                int next = (index + 1) % SegmentCount;
                AddTriangle(vertices, uvs, triangles, topApex, crownRing[index], crownRing[next], 0.96f);
                AddFacetFace(vertices, uvs, triangles, crownRing[index], girdleRing[index], girdleRing[next], crownRing[next], 0.66f);
                AddFacetFace(vertices, uvs, triangles, girdleRing[index], pavilionRing[index], pavilionRing[next], girdleRing[next], 0.32f);
                AddTriangle(vertices, uvs, triangles, pavilionRing[index], bottomApex, pavilionRing[next], 0.05f);
            }

            Mesh mesh = new Mesh
            {
                name = "Kaleidoscope2_RuntimeVolumetricCrystal_" + shape,
                hideFlags = HideFlags.HideAndDontSave
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
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

        private static Vector3[] CreateRing(float radiusX, float radiusZ, float height, float phaseOffset, float facetPulse)
        {
            Vector3[] ring = new Vector3[SegmentCount];
            for (int index = 0; index < SegmentCount; index++)
            {
                float angle = (Mathf.PI * 2f * index / SegmentCount) + phaseOffset;
                float pulse = index % 2 == 0 ? 1f : Mathf.Clamp(facetPulse, 0.78f, 1f);
                ring[index] = new Vector3(
                    Mathf.Cos(angle) * radiusX * pulse,
                    height,
                    Mathf.Sin(angle) * radiusZ * pulse);
            }

            return ring;
        }

        private static void AddTriangle(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            float uvY)
        {
            OrientOutward(ref a, ref b, ref c);
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            uvs.Add(ToUv(a, uvY));
            uvs.Add(ToUv(b, uvY));
            uvs.Add(ToUv(c, uvY));
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        private static void AddFacetFace(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            float uvY)
        {
            AddTriangle(vertices, uvs, triangles, a, b, c, uvY);
            AddTriangle(vertices, uvs, triangles, a, c, d, uvY);
        }

        private static void OrientOutward(ref Vector3 a, ref Vector3 b, ref Vector3 c)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 center = (a + b + c) / 3f;
            if (Vector3.Dot(normal, center) >= 0f)
            {
                return;
            }

            Vector3 swap = b;
            b = c;
            c = swap;
        }

        private static Vector2 ToUv(Vector3 vertex, float uvY)
        {
            float u = Mathf.Clamp01(vertex.x * 0.22f + 0.5f);
            float projectedY = Mathf.Clamp01(vertex.y * 0.28f + 0.5f);
            float v = Mathf.Lerp(projectedY, uvY, 0.35f);
            return new Vector2(u, v);
        }

        private static CrystalProfile ResolveProfile(CrystalShape shape)
        {
            CrystalProfile profile = CrystalProfile.Default;
            switch (shape)
            {
                case CrystalShape.FacetedCube:
                    profile.CrownRadiusX = 0.82f;
                    profile.CrownRadiusZ = 0.78f;
                    profile.GirdleRadiusX = 1.12f;
                    profile.GirdleRadiusZ = 1.04f;
                    profile.PavilionRadiusX = 0.76f;
                    profile.PavilionRadiusZ = 0.7f;
                    profile.FacetPulse = 0.86f;
                    break;
                case CrystalShape.DiscoBall:
                    profile.TopY = 1.02f;
                    profile.BottomY = -1.02f;
                    profile.CrownRadiusX = 0.72f;
                    profile.CrownRadiusZ = 0.72f;
                    profile.GirdleRadiusX = 1.02f;
                    profile.GirdleRadiusZ = 1.02f;
                    profile.PavilionRadiusX = 0.72f;
                    profile.PavilionRadiusZ = 0.72f;
                    profile.FacetPulse = 0.9f;
                    break;
                case CrystalShape.TetrahedralCrystal:
                    profile.CrownRadiusX = 0.48f;
                    profile.CrownRadiusZ = 0.62f;
                    profile.GirdleRadiusX = 1.08f;
                    profile.GirdleRadiusZ = 1.12f;
                    profile.PavilionRadiusX = 0.42f;
                    profile.PavilionRadiusZ = 0.52f;
                    profile.FacetPulse = 0.82f;
                    break;
                case CrystalShape.RhombicCrystal:
                    profile.TopY = 1.3f;
                    profile.BottomY = -1.3f;
                    profile.CrownRadiusX = 0.5f;
                    profile.CrownRadiusZ = 0.74f;
                    profile.GirdleRadiusX = 0.94f;
                    profile.GirdleRadiusZ = 1.2f;
                    profile.PavilionRadiusX = 0.36f;
                    profile.PavilionRadiusZ = 0.62f;
                    profile.FacetPulse = 0.84f;
                    break;
                case CrystalShape.OvalRingGem:
                    profile.TopY = 0.96f;
                    profile.BottomY = -0.96f;
                    profile.CrownRadiusX = 0.86f;
                    profile.CrownRadiusZ = 0.58f;
                    profile.GirdleRadiusX = 1.28f;
                    profile.GirdleRadiusZ = 0.86f;
                    profile.PavilionRadiusX = 0.78f;
                    profile.PavilionRadiusZ = 0.52f;
                    profile.FacetPulse = 0.94f;
                    break;
            }

            return profile;
        }

        private struct CrystalProfile
        {
            public float TopY;
            public float CrownY;
            public float GirdleY;
            public float PavilionY;
            public float BottomY;
            public float CrownRadiusX;
            public float CrownRadiusZ;
            public float GirdleRadiusX;
            public float GirdleRadiusZ;
            public float PavilionRadiusX;
            public float PavilionRadiusZ;
            public float FacetPulse;

            public static CrystalProfile Default
            {
                get
                {
                    return new CrystalProfile
                    {
                        TopY = 1.18f,
                        CrownY = 0.62f,
                        GirdleY = 0f,
                        PavilionY = -0.54f,
                        BottomY = -1.18f,
                        CrownRadiusX = 0.56f,
                        CrownRadiusZ = 0.62f,
                        GirdleRadiusX = 1.06f,
                        GirdleRadiusZ = 0.92f,
                        PavilionRadiusX = 0.58f,
                        PavilionRadiusZ = 0.54f,
                        FacetPulse = 0.92f
                    };
                }
            }
        }
    }
}
