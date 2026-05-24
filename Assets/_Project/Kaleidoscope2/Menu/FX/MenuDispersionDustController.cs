using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class MenuDispersionDustController : MaskableGraphic
    {
        private const int MinimumParticles = 8;
        private const int MaximumParticles = 140;

        private struct DustParticle
        {
            public Vector2 Position;
            public Vector2 Drift;
            public Color Color;
            public float Size;
            public float Age;
            public float Lifetime;
            public float Phase;
        }

        private DustParticle[] particles = new DustParticle[0];
        private System.Random random;
        private float density = 0.46f;
        private float speed = 0.55f;
        private float colorVariation = 0.62f;

        public override Texture mainTexture
        {
            get { return s_WhiteTexture; }
        }

        internal void AssignMaterial(Material runtimeMaterial)
        {
            material = runtimeMaterial;
            SetMaterialDirty();
        }

        internal void Configure(float newDensity, float newSpeed, float newColorVariation)
        {
            newDensity = Mathf.Clamp01(newDensity);
            newSpeed = Mathf.Clamp(newSpeed, 0f, 2f);
            newColorVariation = Mathf.Clamp01(newColorVariation);

            bool countChanged = GetParticleCount(newDensity) != particles.Length;
            bool colorsChanged = !Mathf.Approximately(colorVariation, newColorVariation);

            density = newDensity;
            speed = newSpeed;
            colorVariation = newColorVariation;

            if (countChanged)
            {
                RebuildParticles(true);
            }
            else if (colorsChanged)
            {
                RecolorParticles();
                SetVerticesDirty();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            EnsureRandom();
            RebuildParticles(true);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            RebuildParticles(true);
        }

        private void Update()
        {
            if (particles.Length == 0)
            {
                return;
            }

            Rect rect = GetSafeRect();
            float deltaTime = Application.isPlaying ? Time.unscaledDeltaTime : 0.016f;
            float pixelsPerSecond = 9f + speed * 26f;

            for (int index = 0; index < particles.Length; index++)
            {
                DustParticle particle = particles[index];
                particle.Age += deltaTime;
                particle.Position += particle.Drift * pixelsPerSecond * deltaTime;

                float margin = particle.Size * 4f;
                bool expired = particle.Age >= particle.Lifetime;
                bool outside =
                    particle.Position.y > rect.yMax + margin ||
                    particle.Position.x < rect.xMin - margin ||
                    particle.Position.x > rect.xMax + margin;

                if (expired || outside)
                {
                    RespawnParticle(ref particle, rect, false);
                }

                particles[index] = particle;
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (particles.Length == 0)
            {
                return;
            }

            float time = Application.isPlaying ? Time.unscaledTime : 0f;

            for (int index = 0; index < particles.Length; index++)
            {
                DustParticle particle = particles[index];
                float normalizedAge = Mathf.Clamp01(particle.Age / Mathf.Max(0.001f, particle.Lifetime));
                float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalizedAge * 5f));
                float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - normalizedAge) * 3.2f));
                float alpha = fadeIn * fadeOut * (0.78f + Mathf.Sin(time * 0.34f + particle.Phase) * 0.08f);

                Color color = particle.Color;
                color.a *= Mathf.Clamp01(alpha);

                float halfSize = particle.Size * (0.76f + Mathf.Sin(time * 0.21f + particle.Phase) * 0.08f);
                int vertexIndex = vh.currentVertCount;

                Vector3 min = new Vector3(particle.Position.x - halfSize, particle.Position.y - halfSize, 0f);
                Vector3 max = new Vector3(particle.Position.x + halfSize, particle.Position.y + halfSize, 0f);

                vh.AddVert(new Vector3(min.x, min.y, 0f), color, new Vector2(0f, 0f));
                vh.AddVert(new Vector3(min.x, max.y, 0f), color, new Vector2(0f, 1f));
                vh.AddVert(new Vector3(max.x, max.y, 0f), color, new Vector2(1f, 1f));
                vh.AddVert(new Vector3(max.x, min.y, 0f), color, new Vector2(1f, 0f));
                vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
            }
        }

        private void RebuildParticles(bool distributeAcrossRect)
        {
            int count = GetParticleCount(density);
            particles = new DustParticle[count];
            EnsureRandom();

            Rect rect = GetSafeRect();
            for (int index = 0; index < particles.Length; index++)
            {
                RespawnParticle(ref particles[index], rect, distributeAcrossRect);
            }

            SetVerticesDirty();
        }

        private void RecolorParticles()
        {
            EnsureRandom();
            for (int index = 0; index < particles.Length; index++)
            {
                DustParticle particle = particles[index];
                particle.Color = CreateParticleColor();
                particles[index] = particle;
            }
        }

        private void RespawnParticle(ref DustParticle particle, Rect rect, bool distributeAcrossRect)
        {
            EnsureRandom();

            particle.Size = NextRange(1.4f, 5.4f);
            particle.Lifetime = NextRange(9f, 24f);
            particle.Age = distributeAcrossRect ? NextRange(0f, particle.Lifetime) : 0f;
            particle.Phase = NextRange(0f, Mathf.PI * 2f);

            float x = NextRange(rect.xMin, rect.xMax);
            float y = distributeAcrossRect
                ? NextRange(rect.yMin, rect.yMax)
                : rect.yMin - particle.Size * NextRange(2f, 8f);

            particle.Position = new Vector2(x, y);

            Vector2 drift = new Vector2(NextRange(-0.42f, 0.46f), NextRange(0.58f, 1.0f));
            particle.Drift = drift.normalized;
            particle.Color = CreateParticleColor();
        }

        private Color CreateParticleColor()
        {
            Color cyan = new Color(0.55f, 0.96f, 1f, 1f);
            Color gold = new Color(1f, 0.74f, 0.33f, 1f);
            Color rose = new Color(1f, 0.48f, 0.72f, 1f);
            Color color = Color.Lerp(cyan, gold, Next01() * 0.85f);
            color = Color.Lerp(color, rose, Next01() * 0.22f * colorVariation);
            color.a = NextRange(0.10f, 0.28f) * Mathf.Lerp(0.65f, 1.15f, colorVariation);
            return color;
        }

        private Rect GetSafeRect()
        {
            Rect rect = rectTransform.rect;
            if (rect.width < 1f || rect.height < 1f)
            {
                return new Rect(-960f, -540f, 1920f, 1080f);
            }

            return rect;
        }

        private int GetParticleCount(float targetDensity)
        {
            if (targetDensity <= 0.001f)
            {
                return 0;
            }

            return Mathf.RoundToInt(Mathf.Lerp(MinimumParticles, MaximumParticles, targetDensity));
        }

        private void EnsureRandom()
        {
            if (random == null)
            {
                random = new System.Random(8419);
            }
        }

        private float Next01()
        {
            EnsureRandom();
            return (float)random.NextDouble();
        }

        private float NextRange(float min, float max)
        {
            return Mathf.Lerp(min, max, Next01());
        }
    }
}
