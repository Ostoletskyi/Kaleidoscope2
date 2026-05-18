using Kaleidoscope2.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh
{
    public static class RealCrystalVolumetricMeshFactory
    {
        public const int SegmentCount = 24;
        public const float MinimumValidDepth = 0.5f;

        public static Mesh CreateMesh(CrystalShape shape)
        {
            CrystalProfile profile = ResolveProfile(shape);
            int sideCount = Mathf.Clamp(profile.SideCount, 6, SegmentCount);
            List<Vector3> vertices = new List<Vector3>(sideCount * 27);
            List<Vector2> uvs = new List<Vector2>(sideCount * 27);
            List<int> triangles = new List<int>(sideCount * 27);

            Vector3 tableCenter = new Vector3(0f, profile.TopY, 0f);
            Vector3 bottomApex = new Vector3(0f, profile.BottomY, 0f);
            Vector3[] tableRing = CreateRing(profile, profile.TableRadiusX, profile.TableRadiusZ, profile.TopY, Mathf.PI / sideCount, 0.98f);
            Vector3[] crownRing = CreateRing(profile, profile.CrownRadiusX, profile.CrownRadiusZ, profile.CrownY, 0f, profile.FacetPulse);
            Vector3[] girdleRing = CreateRing(profile, profile.GirdleRadiusX, profile.GirdleRadiusZ, profile.GirdleY, Mathf.PI / sideCount, profile.FacetPulse);
            Vector3[] pavilionRing = CreateRing(profile, profile.PavilionRadiusX, profile.PavilionRadiusZ, profile.PavilionY, 0f, profile.FacetPulse);

            for (int index = 0; index < sideCount; index++)
            {
                int next = (index + 1) % sideCount;
                AddTriangle(vertices, uvs, triangles, tableCenter, tableRing[index], tableRing[next], 0.98f);
                AddFacetFace(vertices, uvs, triangles, tableRing[index], crownRing[index], crownRing[next], tableRing[next], 0.82f);
                AddFacetFace(vertices, uvs, triangles, crownRing[index], girdleRing[index], girdleRing[next], crownRing[next], 0.58f);
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

        private static Vector3[] CreateRing(CrystalProfile profile, float radiusX, float radiusZ, float height, float phaseOffset, float facetPulse)
        {
            int sideCount = Mathf.Clamp(profile.SideCount, 6, SegmentCount);
            Vector3[] ring = new Vector3[sideCount];
            for (int index = 0; index < sideCount; index++)
            {
                float angle = (Mathf.PI * 2f * index / sideCount) + phaseOffset;
                float pulse = index % 2 == 0 ? 1f : Mathf.Clamp(facetPulse, 0.74f, 1f);
                Vector2 silhouette = ResolveSilhouetteScale(profile, angle);
                ring[index] = new Vector3(
                    Mathf.Cos(angle) * radiusX * pulse * silhouette.x,
                    height,
                    Mathf.Sin(angle) * radiusZ * pulse * silhouette.y);
            }

            return ring;
        }

        private static Vector2 ResolveSilhouetteScale(CrystalProfile profile, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float absCos = Mathf.Abs(cos);
            float absSin = Mathf.Abs(sin);
            switch (profile.Silhouette)
            {
                case CrystalSilhouette.Princess:
                    return ResolveStepCutScale(absCos, absSin, 0.66f);
                case CrystalSilhouette.Emerald:
                    return ResolveStepCutScale(absCos, absSin, 0.42f);
                case CrystalSilhouette.Marquise:
                    return new Vector2(1.04f, Mathf.Lerp(1f, 0.28f, Mathf.Pow(absCos, 2.8f)));
                case CrystalSilhouette.Pear:
                    {
                        float tip = Mathf.Clamp01((sin + 1f) * 0.5f);
                        float bulb = Mathf.Lerp(0.84f, 1.12f, 1f - tip);
                        float point = Mathf.Lerp(0.36f, 1f, Mathf.Pow(1f - tip, 0.6f));
                        return new Vector2(bulb, point);
                    }
                case CrystalSilhouette.Cushion:
                    return ResolveStepCutScale(absCos, absSin, 0.34f) * 0.96f;
                case CrystalSilhouette.Radiant:
                    return ResolveStepCutScale(absCos, absSin, 0.52f);
                case CrystalSilhouette.Octagon:
                    return ResolveStepCutScale(absCos, absSin, 0.48f);
                case CrystalSilhouette.Hexagon:
                    return new Vector2(1f, Mathf.Lerp(1f, 0.82f, Mathf.Pow(absSin, 2.2f)));
                default:
                    return Vector2.one;
            }
        }

        private static Vector2 ResolveStepCutScale(float absCos, float absSin, float strength)
        {
            float square = 1f / Mathf.Max(0.72f, Mathf.Max(absCos, absSin));
            float scale = Mathf.Lerp(1f, Mathf.Min(square, 1.22f), Mathf.Clamp01(strength));
            return new Vector2(scale, scale);
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
                case CrystalShape.BrilliantCut:
                case CrystalShape.ClassicDiamond:
                    profile = CrystalProfile.Brilliant;
                    break;
                case CrystalShape.EmeraldCut:
                    profile = CrystalProfile.Emerald;
                    break;
                case CrystalShape.PrincessCut:
                    profile = CrystalProfile.Princess;
                    break;
                case CrystalShape.MarquiseCut:
                    profile = CrystalProfile.Marquise;
                    break;
                case CrystalShape.PearCut:
                    profile = CrystalProfile.Pear;
                    break;
                case CrystalShape.CushionCut:
                    profile = CrystalProfile.Cushion;
                    break;
                case CrystalShape.RadiantCut:
                    profile = CrystalProfile.Radiant;
                    break;
                case CrystalShape.OctagonCut:
                    profile = CrystalProfile.Octagon;
                    break;
                case CrystalShape.HexagonCut:
                    profile = CrystalProfile.Hexagon;
                    break;
                case CrystalShape.FacetedCube:
                    profile = CrystalProfile.Princess;
                    profile.CrownRadiusX = 0.82f;
                    profile.CrownRadiusZ = 0.78f;
                    profile.TableRadiusX = 0.32f;
                    profile.TableRadiusZ = 0.3f;
                    profile.GirdleRadiusX = 1.12f;
                    profile.GirdleRadiusZ = 1.04f;
                    profile.PavilionRadiusX = 0.76f;
                    profile.PavilionRadiusZ = 0.7f;
                    profile.FacetPulse = 0.82f;
                    break;
                case CrystalShape.DiscoBall:
                    profile = CrystalProfile.Cushion;
                    profile.TopY = 1.02f;
                    profile.BottomY = -1.02f;
                    profile.TableRadiusX = 0.26f;
                    profile.TableRadiusZ = 0.26f;
                    profile.CrownRadiusX = 0.72f;
                    profile.CrownRadiusZ = 0.72f;
                    profile.GirdleRadiusX = 1.02f;
                    profile.GirdleRadiusZ = 1.02f;
                    profile.PavilionRadiusX = 0.72f;
                    profile.PavilionRadiusZ = 0.72f;
                    profile.FacetPulse = 0.9f;
                    break;
                case CrystalShape.TetrahedralCrystal:
                    profile = CrystalProfile.Pear;
                    profile.TableRadiusX = 0.16f;
                    profile.TableRadiusZ = 0.2f;
                    profile.CrownRadiusX = 0.48f;
                    profile.CrownRadiusZ = 0.62f;
                    profile.GirdleRadiusX = 1.08f;
                    profile.GirdleRadiusZ = 1.12f;
                    profile.PavilionRadiusX = 0.42f;
                    profile.PavilionRadiusZ = 0.52f;
                    profile.FacetPulse = 0.82f;
                    break;
                case CrystalShape.RhombicCrystal:
                    profile = CrystalProfile.Marquise;
                    profile.TopY = 1.3f;
                    profile.BottomY = -1.3f;
                    profile.TableRadiusX = 0.18f;
                    profile.TableRadiusZ = 0.24f;
                    profile.CrownRadiusX = 0.5f;
                    profile.CrownRadiusZ = 0.74f;
                    profile.GirdleRadiusX = 0.94f;
                    profile.GirdleRadiusZ = 1.2f;
                    profile.PavilionRadiusX = 0.36f;
                    profile.PavilionRadiusZ = 0.62f;
                    profile.FacetPulse = 0.84f;
                    break;
                case CrystalShape.OvalRingGem:
                    profile = CrystalProfile.Emerald;
                    profile.TopY = 0.96f;
                    profile.BottomY = -0.96f;
                    profile.TableRadiusX = 0.36f;
                    profile.TableRadiusZ = 0.24f;
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
            public int SideCount;
            public CrystalSilhouette Silhouette;
            public float TopY;
            public float CrownY;
            public float GirdleY;
            public float PavilionY;
            public float BottomY;
            public float TableRadiusX;
            public float TableRadiusZ;
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
                    return Brilliant;
                }
            }

            public static CrystalProfile Brilliant
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 24,
                        Silhouette = CrystalSilhouette.Brilliant,
                        TopY = 1.18f,
                        CrownY = 0.62f,
                        GirdleY = 0f,
                        PavilionY = -0.54f,
                        BottomY = -1.18f,
                        TableRadiusX = 0.26f,
                        TableRadiusZ = 0.24f,
                        CrownRadiusX = 0.62f,
                        CrownRadiusZ = 0.66f,
                        GirdleRadiusX = 1.12f,
                        GirdleRadiusZ = 0.98f,
                        PavilionRadiusX = 0.48f,
                        PavilionRadiusZ = 0.46f,
                        FacetPulse = 0.84f
                    };
                }
            }

            public static CrystalProfile Emerald
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 8,
                        Silhouette = CrystalSilhouette.Emerald,
                        TopY = 1.04f,
                        CrownY = 0.54f,
                        GirdleY = 0f,
                        PavilionY = -0.48f,
                        BottomY = -1.04f,
                        TableRadiusX = 0.44f,
                        TableRadiusZ = 0.26f,
                        CrownRadiusX = 0.84f,
                        CrownRadiusZ = 0.46f,
                        GirdleRadiusX = 1.28f,
                        GirdleRadiusZ = 0.72f,
                        PavilionRadiusX = 0.72f,
                        PavilionRadiusZ = 0.38f,
                        FacetPulse = 0.96f
                    };
                }
            }

            public static CrystalProfile Princess
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 8,
                        Silhouette = CrystalSilhouette.Princess,
                        TopY = 1.06f,
                        CrownY = 0.54f,
                        GirdleY = 0f,
                        PavilionY = -0.5f,
                        BottomY = -1.08f,
                        TableRadiusX = 0.34f,
                        TableRadiusZ = 0.34f,
                        CrownRadiusX = 0.72f,
                        CrownRadiusZ = 0.72f,
                        GirdleRadiusX = 1.02f,
                        GirdleRadiusZ = 1.02f,
                        PavilionRadiusX = 0.54f,
                        PavilionRadiusZ = 0.54f,
                        FacetPulse = 0.9f
                    };
                }
            }

            public static CrystalProfile Marquise
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 24,
                        Silhouette = CrystalSilhouette.Marquise,
                        TopY = 1.22f,
                        CrownY = 0.6f,
                        GirdleY = 0f,
                        PavilionY = -0.52f,
                        BottomY = -1.18f,
                        TableRadiusX = 0.28f,
                        TableRadiusZ = 0.18f,
                        CrownRadiusX = 0.78f,
                        CrownRadiusZ = 0.36f,
                        GirdleRadiusX = 1.46f,
                        GirdleRadiusZ = 0.54f,
                        PavilionRadiusX = 0.68f,
                        PavilionRadiusZ = 0.28f,
                        FacetPulse = 0.86f
                    };
                }
            }

            public static CrystalProfile Pear
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 24,
                        Silhouette = CrystalSilhouette.Pear,
                        TopY = 1.32f,
                        CrownY = 0.6f,
                        GirdleY = -0.02f,
                        PavilionY = -0.54f,
                        BottomY = -1.22f,
                        TableRadiusX = 0.2f,
                        TableRadiusZ = 0.2f,
                        CrownRadiusX = 0.52f,
                        CrownRadiusZ = 0.5f,
                        GirdleRadiusX = 1.02f,
                        GirdleRadiusZ = 0.88f,
                        PavilionRadiusX = 0.48f,
                        PavilionRadiusZ = 0.5f,
                        FacetPulse = 0.86f
                    };
                }
            }

            public static CrystalProfile Cushion
            {
                get
                {
                    return new CrystalProfile
                    {
                        SideCount = 16,
                        Silhouette = CrystalSilhouette.Cushion,
                        TopY = 1.08f,
                        CrownY = 0.54f,
                        GirdleY = 0f,
                        PavilionY = -0.48f,
                        BottomY = -1.06f,
                        TableRadiusX = 0.34f,
                        TableRadiusZ = 0.32f,
                        CrownRadiusX = 0.74f,
                        CrownRadiusZ = 0.68f,
                        GirdleRadiusX = 1.08f,
                        GirdleRadiusZ = 0.98f,
                        PavilionRadiusX = 0.56f,
                        PavilionRadiusZ = 0.5f,
                        FacetPulse = 0.88f
                    };
                }
            }

            public static CrystalProfile Radiant
            {
                get
                {
                    CrystalProfile profile = Emerald;
                    profile.Silhouette = CrystalSilhouette.Radiant;
                    profile.SideCount = 16;
                    profile.TableRadiusX = 0.38f;
                    profile.TableRadiusZ = 0.28f;
                    profile.CrownRadiusX = 0.78f;
                    profile.CrownRadiusZ = 0.56f;
                    profile.GirdleRadiusX = 1.18f;
                    profile.GirdleRadiusZ = 0.86f;
                    profile.PavilionRadiusX = 0.62f;
                    profile.PavilionRadiusZ = 0.44f;
                    profile.FacetPulse = 0.84f;
                    return profile;
                }
            }

            public static CrystalProfile Octagon
            {
                get
                {
                    CrystalProfile profile = Princess;
                    profile.Silhouette = CrystalSilhouette.Octagon;
                    profile.SideCount = 8;
                    profile.TableRadiusX = 0.3f;
                    profile.TableRadiusZ = 0.3f;
                    profile.GirdleRadiusX = 1.06f;
                    profile.GirdleRadiusZ = 1.06f;
                    profile.FacetPulse = 0.98f;
                    return profile;
                }
            }

            public static CrystalProfile Hexagon
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.Hexagon;
                    profile.SideCount = 6;
                    profile.TableRadiusX = 0.3f;
                    profile.TableRadiusZ = 0.28f;
                    profile.CrownRadiusX = 0.7f;
                    profile.CrownRadiusZ = 0.66f;
                    profile.GirdleRadiusX = 1.08f;
                    profile.GirdleRadiusZ = 1f;
                    profile.PavilionRadiusX = 0.48f;
                    profile.PavilionRadiusZ = 0.46f;
                    profile.FacetPulse = 1f;
                    return profile;
                }
            }
        }

        private enum CrystalSilhouette
        {
            Brilliant,
            Emerald,
            Princess,
            Marquise,
            Pear,
            Cushion,
            Radiant,
            Octagon,
            Hexagon
        }
    }
}
