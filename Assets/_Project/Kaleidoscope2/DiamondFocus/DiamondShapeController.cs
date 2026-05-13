using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public sealed class DiamondShapeController
    {
        private const int MorphSides = 24;
        private const int MorphRingCount = 6;

        private readonly Mesh[] meshes = new Mesh[DiamondFocusSettings.ShapeCount];
        private readonly List<Vector3> transitionVertices = new List<Vector3>(MorphSides * MorphRingCount * 6);
        private readonly List<Vector2> transitionUvs = new List<Vector2>(MorphSides * MorphRingCount * 6);
        private readonly List<int> transitionTriangles = new List<int>(MorphSides * MorphRingCount * 6);
        private Mesh transitionMesh;

        public void NextShape(DiamondFocusSettings settings)
        {
            if (settings != null)
            {
                int nextIndex = (settings.ShapeIndex + 1) % DiamondFocusSettings.ShapeCount;
                settings.BeginShapeTransition((DiamondFocusShape)nextIndex);
            }
        }

        public void PreviousShape(DiamondFocusSettings settings)
        {
            if (settings != null)
            {
                int previousIndex = settings.ShapeIndex - 1;
                if (previousIndex < 0)
                {
                    previousIndex += DiamondFocusSettings.ShapeCount;
                }

                settings.BeginShapeTransition((DiamondFocusShape)previousIndex);
            }
        }

        public void SetShape(DiamondFocusSettings settings, DiamondFocusShape shape)
        {
            if (settings != null)
            {
                settings.BeginShapeTransition(shape);
            }
        }

        public string GetShapeLabel(DiamondFocusSettings settings)
        {
            return settings != null ? settings.ShapeLabel : "None";
        }

        public void Tick(DiamondFocusSettings settings, float deltaTime)
        {
            if (settings != null)
            {
                settings.TickShapeTransition(deltaTime);
            }
        }

        public Mesh GetMesh(DiamondFocusSettings settings)
        {
            if (settings == null)
            {
                return GetMesh(DiamondFocusShape.ClassicDiamond);
            }

            if (settings.ShapeTransitionActive)
            {
                return GetTransitionMesh(
                    settings.ShapeTransitionFromShape,
                    settings.ShapeTransitionToShape,
                    settings.ShapeTransitionSmoothProgress);
            }

            return GetMesh(settings.Shape);
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
            if (transitionMesh != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(transitionMesh);
                }
                else
                {
                    Object.DestroyImmediate(transitionMesh);
                }

                transitionMesh = null;
            }

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
            Mesh mesh = CreateMorphCompatibleMesh(shape);

            mesh.name = "DiamondFocus_" + shape + "_3DMesh";
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Mesh GetTransitionMesh(DiamondFocusShape fromShape, DiamondFocusShape toShape, float progress)
        {
            if (transitionMesh == null)
            {
                transitionMesh = new Mesh
                {
                    name = "DiamondFocus_ShapeTransition_3DMesh",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            BuildMorphGeometry(transitionMesh, fromShape, toShape, Mathf.Clamp01(progress), transitionVertices, transitionUvs, transitionTriangles);
            return transitionMesh;
        }

        private static Mesh CreateMorphCompatibleMesh(DiamondFocusShape shape)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>(MorphSides * MorphRingCount * 6);
            List<Vector2> uvs = new List<Vector2>(MorphSides * MorphRingCount * 6);
            List<int> triangles = new List<int>(MorphSides * MorphRingCount * 6);
            BuildMorphGeometry(mesh, shape, shape, 1f, vertices, uvs, triangles);
            return mesh;
        }

        private static void BuildMorphGeometry(
            Mesh mesh,
            DiamondFocusShape fromShape,
            DiamondFocusShape toShape,
            float progress,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles)
        {
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();

            for (int ring = 0; ring < MorphRingCount - 1; ring++)
            {
                for (int side = 0; side < MorphSides; side++)
                {
                    int next = (side + 1) % MorphSides;
                    Vector3 a = GetMorphedPoint(fromShape, toShape, ring, side, progress);
                    Vector3 b = GetMorphedPoint(fromShape, toShape, ring + 1, side, progress);
                    Vector3 c = GetMorphedPoint(fromShape, toShape, ring + 1, next, progress);
                    Vector3 d = GetMorphedPoint(fromShape, toShape, ring, next, progress);

                    AddQuad(vertices, uvs, triangles, a, b, c, d);
                }
            }

            AddMorphCap(vertices, uvs, triangles, fromShape, toShape, 0, true, progress);
            AddMorphCap(vertices, uvs, triangles, fromShape, toShape, MorphRingCount - 1, false, progress);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private static void AddMorphCap(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            DiamondFocusShape fromShape,
            DiamondFocusShape toShape,
            int ring,
            bool top,
            float progress)
        {
            Vector3 center = GetMorphedCenter(fromShape, toShape, ring, progress);
            for (int side = 0; side < MorphSides; side++)
            {
                int next = (side + 1) % MorphSides;
                Vector3 a = GetMorphedPoint(fromShape, toShape, ring, side, progress);
                Vector3 b = GetMorphedPoint(fromShape, toShape, ring, next, progress);

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

        private static Vector3 GetMorphedPoint(DiamondFocusShape fromShape, DiamondFocusShape toShape, int ring, int side, float progress)
        {
            return Vector3.Lerp(
                GetMorphPoint(fromShape, ring, side),
                GetMorphPoint(toShape, ring, side),
                progress);
        }

        private static Vector3 GetMorphedCenter(DiamondFocusShape fromShape, DiamondFocusShape toShape, int ring, float progress)
        {
            return Vector3.Lerp(GetMorphCenter(fromShape, ring), GetMorphCenter(toShape, ring), progress);
        }

        private static Vector3 GetMorphCenter(DiamondFocusShape shape, int ring)
        {
            RingProfile profile = GetMorphProfile(shape, ring);
            return new Vector3(0f, profile.Y, 0f);
        }

        private static Vector3 GetMorphPoint(DiamondFocusShape shape, int ring, int side)
        {
            RingProfile profile = GetMorphProfile(shape, ring);
            float angle = (Mathf.PI * 2f * side) / MorphSides + profile.Twist;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            float radius = profile.Radius;

            if (profile.Squareness > 0f)
            {
                float squareRadius = radius / Mathf.Max(0.0001f, Mathf.Max(Mathf.Abs(cosine), Mathf.Abs(sine)));
                radius = Mathf.Lerp(radius, squareRadius, profile.Squareness);
            }

            radius *= GetAngularRadiusScale(shape, angle);
            return new Vector3(cosine * radius, profile.Y, sine * radius);
        }

        private static float GetAngularRadiusScale(DiamondFocusShape shape, float angle)
        {
            switch (shape)
            {
                case DiamondFocusShape.FacetedCube:
                    return 0.84f + 0.18f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 0.42f);
                case DiamondFocusShape.DiscoBall:
                    return 0.96f + 0.045f * Mathf.Cos(angle * 12f);
                case DiamondFocusShape.TetrahedralCrystal:
                    return 0.76f + 0.26f * Mathf.Cos(angle * 3f + Mathf.PI * 0.08f);
                case DiamondFocusShape.RhombicCrystal:
                    return 0.7f + 0.34f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2f)), 0.48f);
                case DiamondFocusShape.OvalRingGem:
                    return 0.82f + 0.26f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle)), 0.72f);
                default:
                    return 0.86f + 0.14f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 0.58f);
            }
        }

        private static RingProfile GetMorphProfile(DiamondFocusShape shape, int ring)
        {
            int index = Mathf.Clamp(ring, 0, MorphRingCount - 1);
            switch (shape)
            {
                case DiamondFocusShape.FacetedCube:
                    return GetProfile(index,
                        new RingProfile(0.46f, 0.48f, 0.96f, 0f),
                        new RingProfile(0.3f, 0.58f, 0.92f, Mathf.PI / 12f),
                        new RingProfile(0.1f, 0.6f, 0.98f, 0f),
                        new RingProfile(-0.1f, 0.6f, 0.98f, Mathf.PI / 12f),
                        new RingProfile(-0.3f, 0.58f, 0.92f, 0f),
                        new RingProfile(-0.46f, 0.48f, 0.96f, Mathf.PI / 12f));

                case DiamondFocusShape.DiscoBall:
                    return GetProfile(index,
                        new RingProfile(0.52f, 0.18f, 0f, 0f),
                        new RingProfile(0.34f, 0.46f, 0f, Mathf.PI / 24f),
                        new RingProfile(0.12f, 0.62f, 0f, 0f),
                        new RingProfile(-0.12f, 0.62f, 0f, Mathf.PI / 24f),
                        new RingProfile(-0.34f, 0.46f, 0f, 0f),
                        new RingProfile(-0.52f, 0.18f, 0f, Mathf.PI / 24f));

                case DiamondFocusShape.TetrahedralCrystal:
                    return GetProfile(index,
                        new RingProfile(0.58f, 0.04f, 0f, 0f),
                        new RingProfile(0.3f, 0.52f, 0.04f, Mathf.PI / 6f),
                        new RingProfile(0.02f, 0.6f, 0.04f, -Mathf.PI / 12f),
                        new RingProfile(-0.24f, 0.48f, 0.04f, Mathf.PI / 8f),
                        new RingProfile(-0.46f, 0.18f, 0.02f, -Mathf.PI / 10f),
                        new RingProfile(-0.6f, 0.04f, 0f, 0f));

                case DiamondFocusShape.RhombicCrystal:
                    return GetProfile(index,
                        new RingProfile(0.62f, 0.04f, 0f, 0f),
                        new RingProfile(0.36f, 0.34f, 0.03f, Mathf.PI / 8f),
                        new RingProfile(0.08f, 0.64f, 0.02f, 0f),
                        new RingProfile(-0.08f, 0.64f, 0.02f, Mathf.PI / 8f),
                        new RingProfile(-0.36f, 0.34f, 0.03f, 0f),
                        new RingProfile(-0.62f, 0.04f, 0f, Mathf.PI / 8f));

                case DiamondFocusShape.OvalRingGem:
                    return GetProfile(index,
                        new RingProfile(0.32f, 0.2f, 0.02f, 0f),
                        new RingProfile(0.2f, 0.48f, 0.02f, Mathf.PI / 24f),
                        new RingProfile(0.06f, 0.68f, 0.02f, 0f),
                        new RingProfile(-0.08f, 0.66f, 0.02f, Mathf.PI / 24f),
                        new RingProfile(-0.22f, 0.46f, 0.02f, 0f),
                        new RingProfile(-0.34f, 0.18f, 0.02f, Mathf.PI / 24f));

                default:
                    return GetProfile(index,
                        new RingProfile(0.52f, 0.22f, 0f, 0f),
                        new RingProfile(0.36f, 0.48f, 0.02f, Mathf.PI / 8f),
                        new RingProfile(0.1f, 0.64f, 0f, 0f),
                        new RingProfile(-0.06f, 0.64f, 0.02f, Mathf.PI / 8f),
                        new RingProfile(-0.34f, 0.32f, 0f, 0f),
                        new RingProfile(-0.64f, 0.05f, 0f, Mathf.PI / 8f));
            }
        }

        private static RingProfile GetProfile(int index, RingProfile a, RingProfile b, RingProfile c, RingProfile d, RingProfile e, RingProfile f)
        {
            switch (index)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
                case 3:
                    return d;
                case 4:
                    return e;
                default:
                    return f;
            }
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

                case DiamondFocusShape.DiscoBall:
                    sides = 24;
                    rings = new[]
                    {
                        new RingProfile(0.52f, 0.18f, 0f, 0f),
                        new RingProfile(0.34f, 0.46f, 0f, Mathf.PI / 24f),
                        new RingProfile(0.12f, 0.62f, 0f, 0f),
                        new RingProfile(-0.12f, 0.62f, 0f, Mathf.PI / 24f),
                        new RingProfile(-0.34f, 0.46f, 0f, 0f),
                        new RingProfile(-0.52f, 0.18f, 0f, Mathf.PI / 24f)
                    };
                    break;

                case DiamondFocusShape.TetrahedralCrystal:
                    sides = 6;
                    rings = new[]
                    {
                        new RingProfile(0.58f, 0.04f, 0f, 0f),
                        new RingProfile(0.3f, 0.52f, 0.04f, Mathf.PI / 6f),
                        new RingProfile(0.02f, 0.6f, 0.04f, -Mathf.PI / 12f),
                        new RingProfile(-0.24f, 0.48f, 0.04f, Mathf.PI / 8f),
                        new RingProfile(-0.46f, 0.18f, 0.02f, -Mathf.PI / 10f),
                        new RingProfile(-0.6f, 0.04f, 0f, 0f)
                    };
                    break;

                case DiamondFocusShape.RhombicCrystal:
                    sides = 8;
                    rings = new[]
                    {
                        new RingProfile(0.62f, 0.04f, 0f, 0f),
                        new RingProfile(0.36f, 0.34f, 0.03f, Mathf.PI / 8f),
                        new RingProfile(0.08f, 0.64f, 0.02f, 0f),
                        new RingProfile(-0.08f, 0.64f, 0.02f, Mathf.PI / 8f),
                        new RingProfile(-0.36f, 0.34f, 0.03f, 0f),
                        new RingProfile(-0.62f, 0.04f, 0f, Mathf.PI / 8f)
                    };
                    break;

                case DiamondFocusShape.OvalRingGem:
                    sides = 16;
                    rings = new[]
                    {
                        new RingProfile(0.32f, 0.2f, 0.02f, 0f),
                        new RingProfile(0.2f, 0.48f, 0.02f, Mathf.PI / 24f),
                        new RingProfile(0.06f, 0.68f, 0.02f, 0f),
                        new RingProfile(-0.08f, 0.66f, 0.02f, Mathf.PI / 24f),
                        new RingProfile(-0.22f, 0.46f, 0.02f, 0f),
                        new RingProfile(-0.34f, 0.18f, 0.02f, Mathf.PI / 24f)
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
