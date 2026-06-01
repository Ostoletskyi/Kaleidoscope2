using UnityEngine;
using UnityEngine.UI;

namespace Kaleidoscope2.Menu.FX
{
    [DisallowMultipleComponent]
    public sealed class MenuDefocusedLensParticleController : MaskableGraphic
    {
        private const int MinimumParticles = 10;
        private const int MaximumParticles = 36;

        private struct LensParticle
        {
            public Vector2 Position;
            public Vector2 Drift;
            public Vector2 HalfSize;
            public Color Color;
            public float Age;
            public float Lifetime;
            public float Phase;
            public float SpeedScale;
        }

        private LensParticle[] particles = new LensParticle[0];
        private System.Random random;
        private float density = 0.62f;
        private float speed = 0.32f;
        private float colorVariation = 0.55f;

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
            newSpeed = Mathf.Clamp(newSpeed, 0f, 1.5f);
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
            float pixelsPerSecond = 5f + speed * 18f;

            for (int index = 0; index < particles.Length; index++)
            {
                LensParticle particle = particles[index];
                particle.Age += deltaTime;
                particle.Position += particle.Drift * pixelsPerSecond * particle.SpeedScale * deltaTime;

                float margin = Mathf.Max(particle.HalfSize.x, particle.HalfSize.y) * 4f;
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
                LensParticle particle = particles[index];
                float normalizedAge = Mathf.Clamp01(particle.Age / Mathf.Max(0.001f, particle.Lifetime));
                float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalizedAge * 4f));
                float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - normalizedAge) * 2.5f));
                float pulse = 0.82f + Mathf.Sin(time * 0.11f + particle.Phase) * 0.10f;

                Color color = particle.Color;
                color.a *= fadeIn * fadeOut * pulse;

                Vector2 halfSize = particle.HalfSize * (0.94f + Mathf.Sin(time * 0.07f + particle.Phase) * 0.045f);
                int vertexIndex = vh.currentVertCount;
                Vector3 min = new Vector3(particle.Position.x - halfSize.x, particle.Position.y - halfSize.y, 0f);
                Vector3 max = new Vector3(particle.Position.x + halfSize.x, particle.Position.y + halfSize.y, 0f);

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
            particles = new LensParticle[count];
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
                LensParticle particle = particles[index];
                particle.Color = CreateParticleColor();
                particles[index] = particle;
            }
        }

        private void RespawnParticle(ref LensParticle particle, Rect rect, bool distributeAcrossRect)
        {
            EnsureRandom();

            float radius = NextRange(18f, 74f);
            particle.HalfSize = new Vector2(radius * NextRange(0.82f, 1.55f), radius * NextRange(0.54f, 1.05f));
            particle.Lifetime = NextRange(28f, 62f);
            particle.Age = distributeAcrossRect ? NextRange(0f, particle.Lifetime) : 0f;
            particle.Phase = NextRange(0f, Mathf.PI * 2f);
            particle.SpeedScale = NextRange(0.20f, 0.52f);

            float x = NextRange(rect.xMin, rect.xMax);
            float y = distributeAcrossRect
                ? NextRange(rect.yMin, rect.yMax)
                : rect.yMin - Mathf.Max(particle.HalfSize.x, particle.HalfSize.y) * NextRange(1.2f, 3.5f);

            particle.Position = new Vector2(x, y);
            particle.Drift = new Vector2(NextRange(-0.35f, 0.42f), NextRange(0.24f, 0.68f)).normalized;
            particle.Color = CreateParticleColor();
        }

        private Color CreateParticleColor()
        {
            Color cyan = new Color(0.56f, 0.95f, 1f, 1f);
            Color pearl = new Color(0.88f, 0.98f, 1f, 1f);
            Color gold = new Color(1f, 0.78f, 0.42f, 1f);
            Color color = Color.Lerp(cyan, pearl, NextRange(0.15f, 0.82f));
            color = Color.Lerp(color, gold, Next01() * 0.30f * colorVariation);
            color.a = NextRange(0.018f, 0.060f);
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
                random = new System.Random(9127);
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
