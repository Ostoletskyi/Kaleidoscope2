using UnityEngine;

namespace Kaleidoscope2.Core
{
    public enum PremiumCrystalShapeType
    {
        Sphere = 0,
        Cube = 1,
        Octahedron = 2,
        Hexahedron = 3,
        VolumetricRhombus = 4,
        Cone = 5,
        PlateDisc = 6,
        Icosahedron = 7,
        Dodecahedron = 8,
        DoublePyramid = 9,
        CrystalLens = 10,
        StarPrism = 11
    }

    public static class PremiumCrystalShapeLibrary
    {
        public const int Count = 12;
        public const PremiumCrystalShapeType FallbackShape = PremiumCrystalShapeType.Cube;

        public static PremiumCrystalShapeType Normalize(PremiumCrystalShapeType value)
        {
            int index = (int)value;
            return index >= 0 && index < Count ? value : FallbackShape;
        }

        public static PremiumCrystalShapeType Cycle(PremiumCrystalShapeType value, int direction)
        {
            int step = direction > 0 ? 1 : direction < 0 ? -1 : 0;
            int next = ((int)Normalize(value) + step) % Count;
            if (next < 0)
            {
                next += Count;
            }

            return (PremiumCrystalShapeType)next;
        }

        public static string GetLabel(PremiumCrystalShapeType value)
        {
            switch (Normalize(value))
            {
                case PremiumCrystalShapeType.Sphere:
                    return "Sphere";
                case PremiumCrystalShapeType.Cube:
                    return "Cube";
                case PremiumCrystalShapeType.Octahedron:
                    return "Octahedron";
                case PremiumCrystalShapeType.Hexahedron:
                    return "Hexahedron";
                case PremiumCrystalShapeType.VolumetricRhombus:
                    return "Volumetric Rhombus";
                case PremiumCrystalShapeType.Cone:
                    return "Cone";
                case PremiumCrystalShapeType.PlateDisc:
                    return "Plate / Disc";
                case PremiumCrystalShapeType.Icosahedron:
                    return "Icosahedron";
                case PremiumCrystalShapeType.Dodecahedron:
                    return "Dodecahedron";
                case PremiumCrystalShapeType.DoublePyramid:
                    return "Double Pyramid";
                case PremiumCrystalShapeType.CrystalLens:
                    return "Crystal Lens";
                case PremiumCrystalShapeType.StarPrism:
                    return "Star Prism";
                default:
                    return "Cube";
            }
        }

        public static CrystalShape ToCrystalShape(PremiumCrystalShapeType value)
        {
            switch (Normalize(value))
            {
                case PremiumCrystalShapeType.Sphere:
                    return CrystalShape.PremiumSphere;
                case PremiumCrystalShapeType.Octahedron:
                    return CrystalShape.PremiumOctahedron;
                case PremiumCrystalShapeType.Hexahedron:
                    return CrystalShape.PremiumHexahedron;
                case PremiumCrystalShapeType.VolumetricRhombus:
                    return CrystalShape.PremiumRhombus;
                case PremiumCrystalShapeType.Cone:
                    return CrystalShape.PremiumCone;
                case PremiumCrystalShapeType.PlateDisc:
                    return CrystalShape.PremiumPlate;
                case PremiumCrystalShapeType.Icosahedron:
                    return CrystalShape.PremiumIcosahedron;
                case PremiumCrystalShapeType.Dodecahedron:
                    return CrystalShape.PremiumDodecahedron;
                case PremiumCrystalShapeType.DoublePyramid:
                    return CrystalShape.PremiumBipyramid;
                case PremiumCrystalShapeType.CrystalLens:
                    return CrystalShape.PremiumLens;
                case PremiumCrystalShapeType.StarPrism:
                    return CrystalShape.PremiumStarPrism;
                default:
                    return CrystalShape.PremiumCube;
            }
        }

        public static PremiumCrystalShapeType FromLegacyShape(DiamondFocusShape value)
        {
            switch (value)
            {
                case DiamondFocusShape.FacetedCube:
                    return PremiumCrystalShapeType.Cube;
                case DiamondFocusShape.DiscoBall:
                    return PremiumCrystalShapeType.Sphere;
                case DiamondFocusShape.TetrahedralCrystal:
                    return PremiumCrystalShapeType.Octahedron;
                case DiamondFocusShape.RhombicCrystal:
                    return PremiumCrystalShapeType.VolumetricRhombus;
                case DiamondFocusShape.OvalRingGem:
                    return PremiumCrystalShapeType.CrystalLens;
                case DiamondFocusShape.RadialShardCrystal:
                case DiamondFocusShape.StarDiamond:
                    return PremiumCrystalShapeType.StarPrism;
                case DiamondFocusShape.MandalaCrystal:
                    return PremiumCrystalShapeType.Dodecahedron;
                case DiamondFocusShape.PolygonCrystal:
                    return PremiumCrystalShapeType.Hexahedron;
                default:
                    return PremiumCrystalShapeType.VolumetricRhombus;
            }
        }
    }
}
