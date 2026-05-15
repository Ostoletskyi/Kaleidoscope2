using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public static class RealCrystalShapeLibrary
    {
        private const float MinimumDepth = RealCrystalVolumetricMeshFactory.MinimumValidDepth;
        private const int MinimumRadialSegments = RealCrystalVolumetricMeshFactory.SegmentCount;

        public static Mesh CreateMesh(CrystalShape shape)
        {
            Mesh mesh = RealCrystalVolumetricMeshFactory.CreateMesh(shape);
            if (!HasThickness(mesh))
            {
                Debug.LogWarning("[RealCrystalShapeLibrary] Generated RealMesh3D crystal has insufficient depth and may look flat from side view.");
            }

            return mesh;
        }

        public static bool HasThickness(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            Bounds bounds = mesh.bounds;
            Vector3 size = bounds.size;
            return size.x > MinimumDepth && size.y > MinimumDepth && size.z > MinimumDepth;
        }

        public static bool HasSideFaces(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            int lateralFaceCount = 0;
            for (int index = 0; index + 2 < triangles.Length; index += 3)
            {
                Vector3 a = vertices[triangles[index]];
                Vector3 b = vertices[triangles[index + 1]];
                Vector3 c = vertices[triangles[index + 2]];
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                if (Mathf.Abs(normal.y) < 0.92f && (Mathf.Abs(normal.x) > 0.12f || Mathf.Abs(normal.z) > 0.12f))
                {
                    lateralFaceCount++;
                }
            }

            return lateralFaceCount >= MinimumRadialSegments;
        }

        public static bool HasSeparatedFrontBack(Mesh mesh)
        {
            return mesh != null && mesh.bounds.size.z > MinimumDepth;
        }

        private static int ResolveSideCount(CrystalShape shape)
        {
            switch (shape)
            {
                case CrystalShape.TetrahedralCrystal:
                    return MinimumRadialSegments;
                case CrystalShape.RhombicCrystal:
                    return MinimumRadialSegments;
                case CrystalShape.DiscoBall:
                    return 24;
                default:
                    return MinimumRadialSegments;
            }
        }

        private static CrystalProfile ResolveProfile(CrystalShape shape)
        {
            CrystalProfile profile = CrystalProfile.Default;
            switch (shape)
            {
                case CrystalShape.FacetedCube:
                    profile.TableRadius = 0.36f;
                    profile.CrownRadius = 0.52f;
                    profile.GirdleRadius = 0.58f;
                    profile.PavilionRadius = 0.38f;
                    profile.DepthScale = 0.92f;
                    break;
                case CrystalShape.DiscoBall:
                    profile.TableRadius = 0.18f;
                    profile.CrownRadius = 0.48f;
                    profile.GirdleRadius = 0.55f;
                    profile.PavilionRadius = 0.42f;
                    profile.DepthScale = 1.02f;
                    break;
                case CrystalShape.TetrahedralCrystal:
                    profile.TableRadius = 0.08f;
                    profile.CrownRadius = 0.44f;
                    profile.GirdleRadius = 0.57f;
                    profile.PavilionRadius = 0.26f;
                    profile.DepthScale = 1.08f;
                    break;
                case CrystalShape.RhombicCrystal:
                    profile.TableRadius = 0.14f;
                    profile.CrownRadius = 0.42f;
                    profile.GirdleRadius = 0.62f;
                    profile.PavilionRadius = 0.18f;
                    profile.DepthScale = 0.84f;
                    profile.BottomY = -0.88f;
                    break;
                case CrystalShape.OvalRingGem:
                    profile.TableRadius = 0.32f;
                    profile.CrownRadius = 0.58f;
                    profile.GirdleRadius = 0.72f;
                    profile.PavilionRadius = 0.46f;
                    profile.DepthScale = 0.62f;
                    profile.TopY = 0.54f;
                    profile.BottomY = -0.62f;
                    break;
            }

            return profile;
        }

        private static Vector3[] BuildRing(
            int sides,
            float radius,
            float y,
            CrystalShape shape,
            float depthScale)
        {
            Vector3[] ring = new Vector3[sides];
            for (int index = 0; index < sides; index++)
            {
                float t = index / (float)sides;
                float angle = t * Mathf.PI * 2f;
                float angularScale = ResolveAngularScale(shape, angle);
                float x = Mathf.Cos(angle) * radius * angularScale;
                float z = Mathf.Sin(angle) * radius * depthScale;
                ring[index] = new Vector3(x, y, z);
            }

            return ring;
        }

        private static float ResolveAngularScale(CrystalShape shape, float angle)
        {
            switch (shape)
            {
                case CrystalShape.FacetedCube:
                    return 0.86f + Mathf.Abs(Mathf.Cos(angle * 2f)) * 0.18f;
                case CrystalShape.OvalRingGem:
                    return 1.32f;
                default:
                    return 1f;
            }
        }

        private static void AddFacetTriangle(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            float uvY)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 center = (a + b + c) / 3f;
            if (Vector3.Dot(normal, center) < 0f)
            {
                Vector3 swap = b;
                b = c;
                c = swap;
            }

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

        private static void AddFacetQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            float uvY)
        {
            AddFacetTriangle(vertices, uvs, triangles, a, b, c, uvY);
            AddFacetTriangle(vertices, uvs, triangles, a, c, d, uvY);
        }

        private static Vector2 ToUv(Vector3 vertex, float uvY)
        {
            float u = Mathf.Clamp01(vertex.x * 0.68f + 0.5f);
            float projectedY = Mathf.Clamp01(vertex.y * 0.58f + 0.5f);
            float v = Mathf.Lerp(projectedY, uvY, 0.18f);
            return new Vector2(u, Mathf.Clamp01(v));
        }

        private struct CrystalProfile
        {
            public float TopY;
            public float TableY;
            public float CrownY;
            public float UpperGirdleY;
            public float LowerGirdleY;
            public float PavilionY;
            public float BottomY;
            public float TableRadius;
            public float CrownRadius;
            public float GirdleRadius;
            public float PavilionRadius;
            public float DepthScale;

            public static CrystalProfile Default
            {
                get
                {
                    return new CrystalProfile
                    {
                        TopY = 0.62f,
                        TableY = 0.5f,
                        CrownY = 0.32f,
                        UpperGirdleY = 0.06f,
                        LowerGirdleY = -0.08f,
                        PavilionY = -0.38f,
                        BottomY = -0.78f,
                        TableRadius = 0.22f,
                        CrownRadius = 0.48f,
                        GirdleRadius = 0.62f,
                        PavilionRadius = 0.32f,
                        DepthScale = 1f
                    };
                }
            }
        }
    }
}
