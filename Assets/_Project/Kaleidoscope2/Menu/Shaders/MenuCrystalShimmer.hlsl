#ifndef KAELIS_MENU_CRYSTAL_SHIMMER_INCLUDED
#define KAELIS_MENU_CRYSTAL_SHIMMER_INCLUDED

fixed3 KaelisMenuSpectral(float value)
{
    fixed3 phase = fixed3(0.0, 0.34, 0.67);
    return saturate(0.5 + 0.5 * cos(6.2831853 * (value + phase)));
}

float KaelisMenuFacetLine(float2 p, float2 direction, float frequency, float width)
{
    float2 dir = KaelisMenuSafeDirection(direction, float2(0.7071, 0.7071));
    float lane = abs(frac(dot(p, dir) * frequency) - 0.5) * 2.0;
    return 1.0 - smoothstep(width, width + 0.035, lane);
}

fixed4 KaelisMenuCrystalShimmerFragment(float2 uv, float time)
{
    float intensity = saturate(_ShimmerParams.x);
    float sparkleIntensity = saturate(_ShimmerParams.y);
    float sparkleSpeed = max(0.01, _ShimmerParams.z);
    float hueShift = _ShimmerParams.w + time * 0.018 * sparkleSpeed;

    float2 p = uv - 0.5;
    p.x *= 1.32;

    float diamondDistance = abs(p.x) * 1.18 + abs(p.y) * 1.52;
    float mask = 1.0 - smoothstep(0.52, 0.74, diamondDistance);
    float innerMask = 1.0 - smoothstep(0.16, 0.64, diamondDistance);
    float rim = (1.0 - smoothstep(0.035, 0.18, abs(diamondDistance - 0.55))) * mask;
    float rimPulse = rim * (0.72 + 0.28 * sin(time * 0.58 * sparkleSpeed));

    float facetA = KaelisMenuFacetLine(p + time * 0.002, float2(0.68, 0.38), 7.0, 0.08);
    float facetB = KaelisMenuFacetLine(p - time * 0.0015, float2(-0.35, 0.93), 6.5, 0.075);
    float facetC = KaelisMenuFacetLine(p, float2(1.0, 0.0), 4.0, 0.08);
    float facets = saturate((facetA * 0.46 + facetB * 0.42 + facetC * 0.24) * mask);

    float peakA = pow(saturate(sin(dot(p, float2(28.0, 41.0)) + time * 1.9 * sparkleSpeed) * 0.5 + 0.5), 38.0);
    float peakB = pow(saturate(sin(dot(p, float2(-36.0, 24.0)) - time * 1.36 * sparkleSpeed) * 0.5 + 0.5), 46.0);
    float peakGate = saturate(facets + rim * 0.7 + innerMask * 0.24);
    float sparkle = (peakA * 0.62 + peakB * 0.48) * peakGate * sparkleIntensity * mask;

    fixed3 spectral = KaelisMenuSpectral(hueShift + diamondDistance * 0.36 + facets * 0.18);
    fixed3 premiumColor = lerp(_ShimmerTintA.rgb, _ShimmerTintB.rgb, saturate(facets * 0.65 + rimPulse * 0.25));
    fixed3 dispersion = lerp(premiumColor, spectral, saturate(0.18 + sparkle * 0.52));

    fixed3 color = premiumColor * (rimPulse * 0.42 + facets * 0.14);
    color += dispersion * (sparkle * 1.45 + rimPulse * 0.12);
    color += fixed3(1.0, 0.96, 0.86) * sparkle * 0.75;

    float alpha = saturate((rimPulse * 0.26 + facets * 0.12 + sparkle * 0.54) * intensity);
    return fixed4(color, alpha);
}

fixed4 KaelisMenuDustFragment(float2 uv, fixed4 vertexColor)
{
    float2 p = uv - 0.5;
    float distanceToCenter = length(p);
    float soft = 1.0 - smoothstep(0.12, 0.5, distanceToCenter);
    float core = pow(saturate(1.0 - distanceToCenter * 2.0), 2.6);

    fixed3 color = vertexColor.rgb * (0.62 + core * 1.35);
    float alpha = vertexColor.a * soft;
    return fixed4(color, alpha);
}

#endif
