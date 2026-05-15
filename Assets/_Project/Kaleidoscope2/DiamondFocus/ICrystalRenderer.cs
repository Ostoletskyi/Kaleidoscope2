using Kaleidoscope2.Core;
using UnityEngine;

namespace Kaleidoscope2.DiamondFocus
{
    public interface ICrystalRenderer
    {
        void Initialize(CrystalSharedSettings settings);
        void SetSourceTexture(RenderTexture texture);
        void SetShape(CrystalShape shape);
        void SetMaterialMode(CrystalMaterialMode mode);
        void SetRotation(Vector3 rotation);
        void SetIntensity(float value);
        void SetVisible(bool visible);
        void Render();
        void Shutdown();
    }
}
