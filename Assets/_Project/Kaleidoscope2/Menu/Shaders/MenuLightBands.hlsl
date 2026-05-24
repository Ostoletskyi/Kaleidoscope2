#ifndef KAELIS_MENU_LIGHT_BANDS_INCLUDED
#define KAELIS_MENU_LIGHT_BANDS_INCLUDED

float2 KaelisMenuSafeDirection(float2 direction, float2 fallback)
{
    float lengthSq = dot(direction, direction);
    return lengthSq > 0.0001 ? direction * rsqrt(lengthSq) : fallback;
}

float KaelisMenuHash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float KaelisMenuNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = KaelisMenuHash21(i);
    float b = KaelisMenuHash21(i + float2(1.0, 0.0));
    float c = KaelisMenuHash21(i + float2(0.0, 1.0));
    float d = KaelisMenuHash21(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float KaelisMenuFbm(float2 p)
{
    float total = 0.0;
    float amplitude = 0.5;

    [unroll]
    for (int index = 0; index < 4; index++)
    {
        total += KaelisMenuNoise(p) * amplitude;
        p = p * 2.03 + 17.31;
        amplitude *= 0.5;
    }

    return total;
}

float KaelisMenuSingleBand(float2 uv, float2 direction, float speed, float width, float softness, float time, float phaseOffset)
{
    float2 dir = KaelisMenuSafeDirection(direction, float2(0.82, 0.57));
    float projection = dot(uv - 0.5, dir);
    float phase = frac(time * max(0.0, speed) + phaseOffset);
    float center = lerp(-1.05, 1.05, phase);
    float distanceToCenter = abs(projection - center);
    return 1.0 - smoothstep(width, width + max(0.001, softness), distanceToCenter);
}

float KaelisMenuCausticLine(float2 uv, float2 direction, float frequency, float speed, float time)
{
    float2 dir = KaelisMenuSafeDirection(direction, float2(0.6, 0.8));
    float causticWave = sin(dot(uv, dir) * frequency + time * speed);
    causticWave += sin(dot(uv, dir.yx * float2(-1.0, 1.0)) * (frequency * 0.53) - time * speed * 0.71) * 0.45;
    return pow(saturate(0.52 + causticWave * 0.28), 5.0);
}

fixed4 KaelisMenuAtmosphereFragment(float2 uv, float time)
{
    float noiseStrength = saturate(_AtmosphereParams.y);
    float2 causticDrift = _CausticParams.xy * time;
    float distortionA = KaelisMenuFbm(uv * 3.2 + causticDrift);
    float distortionB = KaelisMenuFbm(uv * 5.1 - causticDrift.yx * 0.72 + 11.37);
    float2 distortedUv = uv + float2(distortionA - 0.5, distortionB - 0.5) * noiseStrength;

    float narrow = KaelisMenuSingleBand(
        distortedUv,
        _BandDirection.xy,
        _NarrowBandParams.x,
        _NarrowBandParams.w,
        _NarrowBandParams.z,
        time,
        0.08);

    float wide = KaelisMenuSingleBand(
        distortedUv,
        _BandDirection.zw,
        _WideBandParams.x,
        _WideBandParams.w,
        _WideBandParams.z,
        time,
        0.62);

    float causticA = KaelisMenuCausticLine(distortedUv + causticDrift * 0.38, float2(0.77, 0.33), 34.0, 0.34, time);
    float causticB = KaelisMenuCausticLine(distortedUv - causticDrift * 0.31, float2(-0.42, 0.91), 26.0, 0.23, time);
    float caustic = saturate(causticA * 0.64 + causticB * 0.52);
    float breath = lerp(1.0, 0.82 + 0.18 * sin(time * 0.31), saturate(_AtmosphereParams.z));

    float narrowAmount = narrow * saturate(_NarrowBandParams.y);
    float wideAmount = wide * saturate(_WideBandParams.y);
    float causticAmount = caustic * saturate(_AtmosphereParams.x) * breath;

    fixed3 color = 0.0;
    color += _NarrowBandColor.rgb * narrowAmount;
    color += _WideBandColor.rgb * wideAmount;
    color += lerp(_NarrowBandColor.rgb, _WideBandColor.rgb, 0.38) * causticAmount;

    float alpha = saturate((narrowAmount + wideAmount + causticAmount) * saturate(_AtmosphereParams.w));
    return fixed4(color, alpha);
}

#endif
