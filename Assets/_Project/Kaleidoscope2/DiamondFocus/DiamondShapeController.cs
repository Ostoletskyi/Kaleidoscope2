using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class DiamondShapeController
    {
        private readonly Mesh[] meshes = new Mesh[DiamondFocusSettings.ShapeCount];

        public void NextShape(DiamondFocusSettings settings)
        {
            if (settings != null)
            {
                settings.CycleShape(1);
            }
        }

        public void PreviousShape(DiamondFocusSettings settings)
        {
            if (settings != null)
            {
                settings.CycleShape(-1);
            }
        }

        public void SetShape(DiamondFocusSettings settings, DiamondFocusShape shape)
        {
            if (settings != null)
            {
                settings.SetShape(shape);
            }
        }

        public string GetShapeLabel(DiamondFocusSettings settings)
        {
            return settings != null ? settings.ShapeLabel : "None";
        }

        public Mesh GetMesh(DiamondFocusShape shape)
        {
            int index = Mathf.Clamp((int)shape, 0, meshes.Length - 1);
            Mesh mesh = meshes[index];
            if (mesh != null)
            {
                return mesh;
            }

            mesh = CreateMesh(shape);
            meshes[index] = mesh;
            return mesh;
        }

        public Mesh GetPlaceholderMesh(DiamondFocusShape shape)
        {
            return GetMesh(shape);
        }

        public void Release()
        {
            for (int index = 0; index < meshes.Length; index++)
            {
                Mesh mesh = meshes[index];
                if (mesh == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(mesh);
                }
                else
                {
                    Object.DestroyImmediate(mesh);
                }

                meshes[index] = null;
            }
        }

        private static Mesh CreateMesh(DiamondFocusShape shape)
        {
            Mesh mesh = shape == DiamondFocusShape.TetrahedralCrystal
                ? CreateTetrahedralMesh()
                : CreateRingMesh(shape);

            mesh.name = "DiamondFocus_" + shape + "_3DMesh";
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTetrahedralMesh()
        {
            List<Vector3> vertices = new List<Vector3>(12);
            List<Vector2> uvs = new List<Vector2>(12);
            List<int> triangles = new List<int>(12);

            Vector3 top = new Vector3(0f, 0.58f, 0f);
            Vector3 a = new Vector3(-0.52f, -0.38f, -0.3f);
            Vector3 b = new Vector3(0.52f, -0.38f, -0.3f);
            Vector3 c = new Vector3(0f, -0.38f, 0.6f);

            AddTriangle(vertices, uvs, triangles, top, a, b);
            AddTriangle(vertices, uvs, triangles, top, b, c);
            AddTriangle(vertices, uvs, triangles, top, c, a);
            AddTriangle(vertices, uvs, triangles, a, c, b);

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        private static Mesh CreateRingMesh(DiamondFocusShape shape)
        {
            int sides;
            RingProfile[] rings;

            switch (shape)
            {
                case DiamondFocusShape.FacetedCube:
                    sides = 12;
                    rings = new[]
                    {
                        new RingProfile(0.48f, 0.36f, 0.9f, 0f),
                        new RingProfile(0.32f, 0.58f, 0.82f, Mathf.PI / 12f),
                        new RingProfile(-0.32f, 0.58f, 0.82f, -Mathf.PI / 12f),
                        new RingProfile(-0.48f, 0.36f, 0.9f, 0f)
                    };
                    break;

                case DiamondFocusShape.TwelveFacetCrystal:
                    sides = 12;
                    rings = new[]
                    {
                        new RingProfile(0.58f, 0.05f, 0f, 0f),
                        new RingProfile(0.36f, 0.36f, 0.12f, Mathf.PI / 12f),
                        new RingProfile(0.02f, 0.6f, 0.1f, 0f),
                        new RingProfile(-0.36f, 0.34f, 0.12f, Mathf.PI / 12f),
                        new RingProfile(-0.58f, 0.04f, 0f, 0f)
                    };
                    break;

                case DiamondFocusShape.HighDetailDiamond:
                    sides = 96;
                    rings = new[]
                    {
                        new RingProfile(0.48f, 0.18f, 0f, 0f),
                        new RingProfile(0.34f, 0.36f, 0f, Mathf.PI / 96f),
                        new RingProfile(0.12f, 0.62f, 0f, 0f),
                        new RingProfile(-0.03f, 0.66f, 0f, Mathf.PI / 96f),
                        new RingProfile(-0.28f, 0.42f, 0f, 0f),
                        new RingProfile(-0.62f, 0.04f, 0f, Mathf.PI / 96f)
                    };
                    break;

                default:
                    sides = 8;
                    rings = new[]
                    {
                        new RingProfile(0.5f, 0.22f, 0f, 0f),
                        new RingProfile(0.32f, 0.42f, 0f, Mathf.PI / 8f),
                        new RingProfile(0.08f, 0.62f, 0f, 0f),
                        new RingProfile(-0.04f, 0.62f, 0f, Mathf.PI / 8f),
                        new RingProfile(-0.34f, 0.32f, 0f, 0f),
                        new RingProfile(-0.62f, 0.05f, 0f, Mathf.PI / 8f)
                    };
                    break;
            }

            List<Vector3> vertices = new List<Vector3>(sides * rings.Length * 6);
            List<Vector2> uvs = new List<Vector2>(sides * rings.Length * 6);
            List<int> triangles = new List<int>(sides * rings.Length * 6);

            for (int ring = 0; ring < rings.Length - 1; ring++)
            {
                for (int side = 0; side < sides; side++)
                {
                    int next = (side + 1) % sides;
                    Vector3 a = GetRingPoint(rings[ring], side, sides);
                    Vector3 b = GetRingPoint(rings[ring + 1], side, sides);
                    Vector3 c = GetRingPoint(rings[ring + 1], next, sides);
                    Vector3 d = GetRingPoint(rings[ring], next, sides);

                    AddQuad(vertices, uvs, triangles, a, b, c, d);
                }
            }

            AddCap(vertices, uvs, triangles, rings[0], sides, true);
            AddCap(vertices, uvs, triangles, rings[rings.Length - 1], sides, false);

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        private static Vector3 GetRingPoint(RingProfile ring, int index, int sides)
        {
            float angle = (Mathf.PI * 2f * index) / sides + ring.Twist;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            float radius = ring.Radius;

            if (ring.Squareness > 0f)
            {
                float squareRadius = radius / Mathf.Max(0.0001f, Mathf.Max(Mathf.Abs(cosine), Mathf.Abs(sine)));
                radius = Mathf.Lerp(radius, squareRadius, ring.Squareness);
            }

            return new Vector3(cosine * radius, ring.Y, sine * radius);
        }

        private static void AddCap(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, RingProfile ring, int sides, bool top)
        {
            Vector3 center = new Vector3(0f, ring.Y, 0f);
            for (int side = 0; side < sides; side++)
            {
                int next = (side + 1) % sides;
                Vector3 a = GetRingPoint(ring, side, sides);
                Vector3 b = GetRingPoint(ring, next, sides);

                if (top)
                {
                    AddTriangle(vertices, uvs, triangles, center, b, a);
                }
                else
                {
                    AddTriangle(vertices, uvs, triangles, center, a, b);
                }
            }
        }

        private static void AddQuad(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddTriangle(vertices, uvs, triangles, a, b, c);
            AddTriangle(vertices, uvs, triangles, a, c, d);
        }

        private static void AddTriangle(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            int index = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            uvs.Add(MakeUv(a));
            uvs.Add(MakeUv(b));
            uvs.Add(MakeUv(c));
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        private static Vector2 MakeUv(Vector3 value)
        {
            return new Vector2(value.x * 0.5f + 0.5f, value.z * 0.5f + 0.5f);
        }

        private struct RingProfile
        {
            public readonly float Y;
            public readonly float Radius;
            public readonly float Squareness;
            public readonly float Twist;

            public RingProfile(float y, float radius, float squareness, float twist)
            {
                Y = y;
                Radius = radius;
                Squareness = Mathf.Clamp01(squareness);
                Twist = twist;
            }
        }
    }
}
