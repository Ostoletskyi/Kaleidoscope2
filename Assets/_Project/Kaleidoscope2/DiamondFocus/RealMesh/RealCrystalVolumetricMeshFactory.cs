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
            int sideCount = Mathf.Clamp(ResolveProfile(shape).SideCount, 6, SegmentCount);
            List<Vector3> vertices = new List<Vector3>(sideCount * 30);
            List<Vector2> uvs = new List<Vector2>(sideCount * 30);
            List<int> triangles = new List<int>(sideCount * 30);
            Mesh mesh = new Mesh
            {
                name = "Kaleidoscope2_RuntimeVolumetricCrystal_" + shape,
                hideFlags = HideFlags.HideAndDontSave
            };
            UpdateMesh(mesh, shape, vertices, uvs, triangles);
            return mesh;
        }

        public static void UpdateMesh(
            Mesh mesh,
            CrystalShape shape,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles)
        {
            BuildGeometry(mesh, shape, shape, 1f, false, vertices, uvs, triangles);
        }

        public static void UpdateMorphedMesh(
            Mesh mesh,
            CrystalShape fromShape,
            CrystalShape toShape,
            float progress,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles)
        {
            BuildGeometry(mesh, fromShape, toShape, Mathf.Clamp01(progress), true, vertices, uvs, triangles);
        }

        public static Bounds ResolveMaximumProfileBounds(CrystalShape firstShape, CrystalShape secondShape)
        {
            Bounds firstBounds = EstimateProfileBounds(ResolveProfile(firstShape), SegmentCount);
            Bounds secondBounds = EstimateProfileBounds(ResolveProfile(secondShape), SegmentCount);
            Vector3 size = Vector3.Max(firstBounds.size, secondBounds.size);
            return new Bounds(Vector3.zero, size);
        }

        private static void BuildGeometry(
            Mesh mesh,
            CrystalShape fromShape,
            CrystalShape toShape,
            float progress,
            bool forceMorphTopology,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles)
        {
            if (mesh == null)
            {
                return;
            }

            CrystalProfile fromProfile = ResolveProfile(fromShape);
            CrystalProfile toProfile = ResolveProfile(toShape);
            int sideCount = forceMorphTopology
                ? SegmentCount
                : Mathf.Clamp(toProfile.SideCount, 6, SegmentCount);

            vertices.Clear();
            uvs.Clear();
            triangles.Clear();

            Vector3 tableCenter = Vector3.Lerp(
                new Vector3(0f, fromProfile.TopY, 0f),
                new Vector3(0f, toProfile.TopY, 0f),
                progress);
            Vector3 bottomApex = Vector3.Lerp(
                new Vector3(0f, fromProfile.BottomY, 0f),
                new Vector3(0f, toProfile.BottomY, 0f),
                progress);
            Vector3[] tableRing = CreateMorphedRing(fromProfile, toProfile, sideCount, RingKind.Table, Mathf.PI / sideCount, 1f, progress);
            Vector3[] crownRing = CreateMorphedRing(fromProfile, toProfile, sideCount, RingKind.Crown, 0f, 0f, progress);
            Vector3[] girdleTopRing = CreateMorphedRing(fromProfile, toProfile, sideCount, RingKind.GirdleTop, Mathf.PI / sideCount, 1f, progress);
            Vector3[] girdleBottomRing = CreateMorphedRing(fromProfile, toProfile, sideCount, RingKind.GirdleBottom, Mathf.PI / sideCount, 1f, progress);
            Vector3[] pavilionRing = CreateMorphedRing(fromProfile, toProfile, sideCount, RingKind.Pavilion, 0f, 0f, progress);

            for (int index = 0; index < sideCount; index++)
            {
                int next = (index + 1) % sideCount;
                AddTriangle(vertices, uvs, triangles, tableCenter, tableRing[index], tableRing[next], 0.98f);
                AddFacetFace(vertices, uvs, triangles, tableRing[index], crownRing[index], crownRing[next], tableRing[next], 0.82f);
                AddFacetFace(vertices, uvs, triangles, crownRing[index], girdleTopRing[index], girdleTopRing[next], crownRing[next], 0.6f);
                AddFacetFace(vertices, uvs, triangles, girdleTopRing[index], girdleBottomRing[index], girdleBottomRing[next], girdleTopRing[next], 0.48f);
                AddFacetFace(vertices, uvs, triangles, girdleBottomRing[index], pavilionRing[index], pavilionRing[next], girdleBottomRing[next], 0.28f);
                AddTriangle(vertices, uvs, triangles, pavilionRing[index], bottomApex, pavilionRing[next], 0.05f);
            }

            CenterVerticesOnBounds(vertices);
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        private static Bounds EstimateProfileBounds(CrystalProfile profile, int sideCount)
        {
            Vector3 tableCenter = new Vector3(0f, profile.TopY, 0f);
            Vector3 bottomApex = new Vector3(0f, profile.BottomY, 0f);
            Bounds bounds = new Bounds(tableCenter, Vector3.zero);
            bounds.Encapsulate(bottomApex);
            EncapsulateRing(ref bounds, CreateRing(profile, sideCount, profile.TableRadiusX, profile.TableRadiusZ, profile.TopY, Mathf.PI / sideCount, 1f));
            EncapsulateRing(ref bounds, CreateRing(profile, sideCount, profile.CrownRadiusX, profile.CrownRadiusZ, profile.CrownY, 0f, profile.FacetAlternation));
            EncapsulateRing(ref bounds, CreateRing(profile, sideCount, profile.GirdleRadiusX, profile.GirdleRadiusZ, profile.GirdleY + profile.GirdleHalfThickness, Mathf.PI / sideCount, 1f));
            EncapsulateRing(ref bounds, CreateRing(profile, sideCount, profile.GirdleRadiusX, profile.GirdleRadiusZ, profile.GirdleY - profile.GirdleHalfThickness, Mathf.PI / sideCount, 1f));
            EncapsulateRing(ref bounds, CreateRing(profile, sideCount, profile.PavilionRadiusX, profile.PavilionRadiusZ, profile.PavilionY, 0f, profile.FacetAlternation));
            return bounds;
        }

        private static void EncapsulateRing(ref Bounds bounds, Vector3[] ring)
        {
            for (int index = 0; index < ring.Length; index++)
            {
                bounds.Encapsulate(ring[index]);
            }
        }

        private static void CenterVerticesOnBounds(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count == 0)
            {
                return;
            }

            Vector3 min = vertices[0];
            Vector3 max = vertices[0];
            for (int index = 1; index < vertices.Count; index++)
            {
                Vector3 vertex = vertices[index];
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            Vector3 center = (min + max) * 0.5f;
            for (int index = 0; index < vertices.Count; index++)
            {
                vertices[index] = vertices[index] - center;
            }
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

        private static Vector3[] CreateMorphedRing(
            CrystalProfile fromProfile,
            CrystalProfile toProfile,
            int sideCount,
            RingKind ringKind,
            float phaseOffset,
            float fixedFacetAlternation,
            float progress)
        {
            Vector3[] ring = new Vector3[sideCount];
            ResolveRing(fromProfile, ringKind, out float fromRadiusX, out float fromRadiusZ, out float fromHeight, out float fromFacetAlternation);
            ResolveRing(toProfile, ringKind, out float toRadiusX, out float toRadiusZ, out float toHeight, out float toFacetAlternation);
            if (fixedFacetAlternation > 0f)
            {
                fromFacetAlternation = fixedFacetAlternation;
                toFacetAlternation = fixedFacetAlternation;
            }

            for (int index = 0; index < sideCount; index++)
            {
                Vector3 fromPoint = CreateRingPoint(fromProfile, sideCount, index, fromRadiusX, fromRadiusZ, fromHeight, phaseOffset, fromFacetAlternation);
                Vector3 toPoint = CreateRingPoint(toProfile, sideCount, index, toRadiusX, toRadiusZ, toHeight, phaseOffset, toFacetAlternation);
                ring[index] = Vector3.Lerp(fromPoint, toPoint, progress);
            }

            return ring;
        }

        private static Vector3[] CreateRing(CrystalProfile profile, int sideCount, float radiusX, float radiusZ, float height, float phaseOffset, float facetAlternation)
        {
            Vector3[] ring = new Vector3[sideCount];
            for (int index = 0; index < sideCount; index++)
            {
                ring[index] = CreateRingPoint(profile, sideCount, index, radiusX, radiusZ, height, phaseOffset, facetAlternation);
            }

            return ring;
        }

        private static Vector3 CreateRingPoint(
            CrystalProfile profile,
            int sideCount,
            int index,
            float radiusX,
            float radiusZ,
            float height,
            float phaseOffset,
            float facetAlternation)
        {
            float angle = (Mathf.PI * 2f * index / sideCount) + phaseOffset;
            float alternatingFacetScale = index % 2 == 0 ? 1f : Mathf.Clamp(facetAlternation, 0.74f, 1f);
            Vector2 silhouette = ResolveSilhouetteScale(profile, angle);
            return new Vector3(
                Mathf.Cos(angle) * radiusX * alternatingFacetScale * silhouette.x,
                height,
                Mathf.Sin(angle) * radiusZ * alternatingFacetScale * silhouette.y);
        }

        private static void ResolveRing(
            CrystalProfile profile,
            RingKind ringKind,
            out float radiusX,
            out float radiusZ,
            out float height,
            out float facetAlternation)
        {
            switch (ringKind)
            {
                case RingKind.Crown:
                    radiusX = profile.CrownRadiusX;
                    radiusZ = profile.CrownRadiusZ;
                    height = profile.CrownY;
                    facetAlternation = profile.FacetAlternation;
                    return;
                case RingKind.GirdleTop:
                    radiusX = profile.GirdleRadiusX;
                    radiusZ = profile.GirdleRadiusZ;
                    height = profile.GirdleY + profile.GirdleHalfThickness;
                    facetAlternation = 1f;
                    return;
                case RingKind.GirdleBottom:
                    radiusX = profile.GirdleRadiusX;
                    radiusZ = profile.GirdleRadiusZ;
                    height = profile.GirdleY - profile.GirdleHalfThickness;
                    facetAlternation = 1f;
                    return;
                case RingKind.Pavilion:
                    radiusX = profile.PavilionRadiusX;
                    radiusZ = profile.PavilionRadiusZ;
                    height = profile.PavilionY;
                    facetAlternation = profile.FacetAlternation;
                    return;
                default:
                    radiusX = profile.TableRadiusX;
                    radiusZ = profile.TableRadiusZ;
                    height = profile.TopY;
                    facetAlternation = 1f;
                    return;
            }
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
                case CrystalSilhouette.Trilliant:
                    {
                        float tri = 0.72f + 0.38f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 3f)), 0.55f);
                        return new Vector2(tri, tri);
                    }
                case CrystalSilhouette.Round:
                    {
                        float scallop = 1f + 0.06f * Mathf.Cos(angle * 16f);
                        return new Vector2(scallop, scallop);
                    }
                case CrystalSilhouette.Oval:
                    return new Vector2(1.28f, 0.72f);
                case CrystalSilhouette.RadialShard:
                    {
                        float point = Mathf.Pow(Mathf.Max(0f, cos), 1.8f);
                        float tail = Mathf.Pow(Mathf.Max(0f, -cos), 0.7f);
                        return new Vector2(0.68f + point * 0.78f + tail * 0.22f, Mathf.Lerp(0.48f, 1.08f, absSin));
                    }
                case CrystalSilhouette.Mandala:
                    {
                        float rosette = 0.84f + 0.22f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 8f)), 0.45f);
                        return new Vector2(rosette, rosette);
                    }
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
                case CrystalShape.TrilliantCut:
                    profile = CrystalProfile.Trilliant;
                    break;
                case CrystalShape.RoundCut:
                    profile = CrystalProfile.Round;
                    break;
                case CrystalShape.OvalCut:
                    profile = CrystalProfile.Oval;
                    break;
                case CrystalShape.RadialShardCut:
                    profile = CrystalProfile.RadialShard;
                    break;
                case CrystalShape.MandalaCut:
                    profile = CrystalProfile.Mandala;
                    break;
                case CrystalShape.FacetedCube:
                    profile = CrystalProfile.Octagon;
                    profile.CrownRadiusX = 0.86f;
                    profile.CrownRadiusZ = 0.86f;
                    profile.TableRadiusX = 0.34f;
                    profile.TableRadiusZ = 0.34f;
                    profile.GirdleRadiusX = 1.16f;
                    profile.GirdleRadiusZ = 1.16f;
                    profile.PavilionRadiusX = 0.66f;
                    profile.PavilionRadiusZ = 0.66f;
                    profile.FacetAlternation = 1f;
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
                    profile.FacetAlternation = 0.9f;
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
                    profile.FacetAlternation = 0.82f;
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
                    profile.FacetAlternation = 0.84f;
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
                    profile.FacetAlternation = 0.94f;
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
            public float GirdleHalfThickness;
            public float FacetAlternation;

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
                        SideCount = 32,
                        Silhouette = CrystalSilhouette.Brilliant,
                        TopY = 0.94f,
                        CrownY = 0.5f,
                        GirdleY = 0f,
                        PavilionY = -0.56f,
                        BottomY = -1.34f,
                        TableRadiusX = 0.34f,
                        TableRadiusZ = 0.34f,
                        CrownRadiusX = 0.8f,
                        CrownRadiusZ = 0.8f,
                        GirdleRadiusX = 1.14f,
                        GirdleRadiusZ = 1.14f,
                        PavilionRadiusX = 0.62f,
                        PavilionRadiusZ = 0.62f,
                        GirdleHalfThickness = 0.095f,
                        FacetAlternation = 0.96f
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
                        TopY = 0.98f,
                        CrownY = 0.48f,
                        GirdleY = 0f,
                        PavilionY = -0.46f,
                        BottomY = -1.08f,
                        TableRadiusX = 0.54f,
                        TableRadiusZ = 0.3f,
                        CrownRadiusX = 0.96f,
                        CrownRadiusZ = 0.54f,
                        GirdleRadiusX = 1.36f,
                        GirdleRadiusZ = 0.82f,
                        PavilionRadiusX = 0.78f,
                        PavilionRadiusZ = 0.46f,
                        GirdleHalfThickness = 0.085f,
                        FacetAlternation = 0.99f
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
                        TopY = 0.96f,
                        CrownY = 0.48f,
                        GirdleY = 0f,
                        PavilionY = -0.46f,
                        BottomY = -1.12f,
                        TableRadiusX = 0.38f,
                        TableRadiusZ = 0.38f,
                        CrownRadiusX = 0.82f,
                        CrownRadiusZ = 0.82f,
                        GirdleRadiusX = 1.14f,
                        GirdleRadiusZ = 1.14f,
                        PavilionRadiusX = 0.64f,
                        PavilionRadiusZ = 0.64f,
                        GirdleHalfThickness = 0.09f,
                        FacetAlternation = 0.96f
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
                        TopY = 1.04f,
                        CrownY = 0.52f,
                        GirdleY = 0f,
                        PavilionY = -0.48f,
                        BottomY = -1.16f,
                        TableRadiusX = 0.32f,
                        TableRadiusZ = 0.18f,
                        CrownRadiusX = 0.94f,
                        CrownRadiusZ = 0.38f,
                        GirdleRadiusX = 1.62f,
                        GirdleRadiusZ = 0.62f,
                        PavilionRadiusX = 0.78f,
                        PavilionRadiusZ = 0.32f,
                        GirdleHalfThickness = 0.075f,
                        FacetAlternation = 0.94f
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
                        TopY = 1.08f,
                        CrownY = 0.52f,
                        GirdleY = -0.02f,
                        PavilionY = -0.5f,
                        BottomY = -1.18f,
                        TableRadiusX = 0.26f,
                        TableRadiusZ = 0.24f,
                        CrownRadiusX = 0.64f,
                        CrownRadiusZ = 0.58f,
                        GirdleRadiusX = 1.16f,
                        GirdleRadiusZ = 1.0f,
                        PavilionRadiusX = 0.6f,
                        PavilionRadiusZ = 0.54f,
                        GirdleHalfThickness = 0.075f,
                        FacetAlternation = 0.94f
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
                        TopY = 1f,
                        CrownY = 0.5f,
                        GirdleY = 0f,
                        PavilionY = -0.46f,
                        BottomY = -1.1f,
                        TableRadiusX = 0.42f,
                        TableRadiusZ = 0.38f,
                        CrownRadiusX = 0.84f,
                        CrownRadiusZ = 0.78f,
                        GirdleRadiusX = 1.18f,
                        GirdleRadiusZ = 1.08f,
                        PavilionRadiusX = 0.66f,
                        PavilionRadiusZ = 0.58f,
                        GirdleHalfThickness = 0.09f,
                        FacetAlternation = 0.96f
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
                    profile.GirdleHalfThickness = 0.07f;
                    profile.FacetAlternation = 0.95f;
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
                    profile.TableRadiusX = 0.36f;
                    profile.TableRadiusZ = 0.36f;
                    profile.CrownRadiusX = 0.86f;
                    profile.CrownRadiusZ = 0.86f;
                    profile.GirdleRadiusX = 1.18f;
                    profile.GirdleRadiusZ = 1.18f;
                    profile.PavilionRadiusX = 0.66f;
                    profile.PavilionRadiusZ = 0.66f;
                    profile.GirdleHalfThickness = 0.09f;
                    profile.FacetAlternation = 1f;
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
                    profile.TableRadiusX = 0.34f;
                    profile.TableRadiusZ = 0.32f;
                    profile.CrownRadiusX = 0.76f;
                    profile.CrownRadiusZ = 0.72f;
                    profile.GirdleRadiusX = 1.16f;
                    profile.GirdleRadiusZ = 1.08f;
                    profile.PavilionRadiusX = 0.56f;
                    profile.PavilionRadiusZ = 0.52f;
                    profile.GirdleHalfThickness = 0.085f;
                    profile.FacetAlternation = 1f;
                    return profile;
                }
            }

            public static CrystalProfile Trilliant
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.Trilliant;
                    profile.SideCount = 6;
                    profile.TopY = 1.02f;
                    profile.BottomY = -1.18f;
                    profile.TableRadiusX = 0.24f;
                    profile.TableRadiusZ = 0.24f;
                    profile.CrownRadiusX = 0.74f;
                    profile.CrownRadiusZ = 0.74f;
                    profile.GirdleRadiusX = 1.18f;
                    profile.GirdleRadiusZ = 1.18f;
                    profile.PavilionRadiusX = 0.48f;
                    profile.PavilionRadiusZ = 0.48f;
                    profile.GirdleHalfThickness = 0.08f;
                    profile.FacetAlternation = 0.9f;
                    return profile;
                }
            }

            public static CrystalProfile Round
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.Round;
                    profile.SideCount = 32;
                    profile.TopY = 1.0f;
                    profile.BottomY = -1.0f;
                    profile.TableRadiusX = 0.24f;
                    profile.TableRadiusZ = 0.24f;
                    profile.CrownRadiusX = 0.78f;
                    profile.CrownRadiusZ = 0.78f;
                    profile.GirdleRadiusX = 1.04f;
                    profile.GirdleRadiusZ = 1.04f;
                    profile.PavilionRadiusX = 0.74f;
                    profile.PavilionRadiusZ = 0.74f;
                    profile.GirdleHalfThickness = 0.12f;
                    profile.FacetAlternation = 0.88f;
                    return profile;
                }
            }

            public static CrystalProfile Oval
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.Oval;
                    profile.SideCount = 24;
                    profile.TopY = 0.98f;
                    profile.BottomY = -1.08f;
                    profile.TableRadiusX = 0.3f;
                    profile.TableRadiusZ = 0.26f;
                    profile.CrownRadiusX = 0.82f;
                    profile.CrownRadiusZ = 0.72f;
                    profile.GirdleRadiusX = 1.08f;
                    profile.GirdleRadiusZ = 1.0f;
                    profile.PavilionRadiusX = 0.62f;
                    profile.PavilionRadiusZ = 0.58f;
                    profile.GirdleHalfThickness = 0.085f;
                    profile.FacetAlternation = 0.93f;
                    return profile;
                }
            }

            public static CrystalProfile RadialShard
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.RadialShard;
                    profile.SideCount = 12;
                    profile.TopY = 1.12f;
                    profile.BottomY = -1.18f;
                    profile.TableRadiusX = 0.18f;
                    profile.TableRadiusZ = 0.2f;
                    profile.CrownRadiusX = 0.58f;
                    profile.CrownRadiusZ = 0.52f;
                    profile.GirdleRadiusX = 1.22f;
                    profile.GirdleRadiusZ = 0.92f;
                    profile.PavilionRadiusX = 0.5f;
                    profile.PavilionRadiusZ = 0.42f;
                    profile.GirdleHalfThickness = 0.075f;
                    profile.FacetAlternation = 0.84f;
                    return profile;
                }
            }

            public static CrystalProfile Mandala
            {
                get
                {
                    CrystalProfile profile = Brilliant;
                    profile.Silhouette = CrystalSilhouette.Mandala;
                    profile.SideCount = 32;
                    profile.TopY = 0.98f;
                    profile.BottomY = -1.12f;
                    profile.TableRadiusX = 0.28f;
                    profile.TableRadiusZ = 0.28f;
                    profile.CrownRadiusX = 0.76f;
                    profile.CrownRadiusZ = 0.76f;
                    profile.GirdleRadiusX = 1.12f;
                    profile.GirdleRadiusZ = 1.12f;
                    profile.PavilionRadiusX = 0.58f;
                    profile.PavilionRadiusZ = 0.58f;
                    profile.GirdleHalfThickness = 0.075f;
                    profile.FacetAlternation = 0.82f;
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
            Hexagon,
            Trilliant,
            Round,
            Oval,
            RadialShard,
            Mandala
        }

        private enum RingKind
        {
            Table,
            Crown,
            GirdleTop,
            GirdleBottom,
            Pavilion
        }
    }
}
