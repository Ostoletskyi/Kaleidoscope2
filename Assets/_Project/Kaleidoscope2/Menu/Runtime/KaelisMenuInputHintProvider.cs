namespace Kaleidoscope2.Menu
{
    internal enum KaelisMenuInputHintKind
    {
        Select,
        Toggle,
        Slider,
        Action,
        Geometry,
        Language,
        Reserved
    }

    internal static class KaelisMenuInputHintProvider
    {
        public static string Get(KaelisMenuInputHintKind kind)
        {
            switch (kind)
            {
                case KaelisMenuInputHintKind.Toggle:
                    return "Enter / Space / Click to toggle";
                case KaelisMenuInputHintKind.Slider:
                    return "Drag to adjust";
                case KaelisMenuInputHintKind.Action:
                    return "Enter / Click";
                case KaelisMenuInputHintKind.Geometry:
                    return "+ / - : Smooth Crystal Geometry\nNum Del : Cycle Selected Control Class";
                case KaelisMenuInputHintKind.Language:
                    return "Enter / Click to change language";
                case KaelisMenuInputHintKind.Reserved:
                    return "Click to view reserved action";
                default:
                    return "Enter / Click to select";
            }
        }
    }
}
