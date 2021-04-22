#include "UnityCG.cginc"

#ifndef HOLOCP_INCLUDED
#define HOLOCP_INCLUDED

#pragma target 5.0
#pragma only_renderers d3d11

fixed3 mod(fixed3 x, fixed3 y)
{
	return x - y * floor(x / y);
}

fixed r(fixed n)
{
	return frac(abs(sin(n*55.753)*367.34));
}

fixed r(fixed2 n)
{
	return r(dot(n, fixed2(2.46, -1.21)));
}

fixed hash1(fixed2 p)
{
	fixed n = dot(p, fixed2(127.1, 311.7));
	return frac(sin(n));
}

half cubicPulse(fixed c, fixed w, fixed x)
{
	x = abs(x - c);
	if (x > w)
		return 0;
	x /= w;
	return 1 - x * x * (3 - 2 * x);
}

half2 cubicPulse(half c, half2 w, half x)
{
    x = abs(x - c);
    if (x > w.x)
        return 0;
    half2 x2 = x / w;
    if (x > w.y)
        return half2(1 - x2.x * x2.x * (3 - 2 * x2.x), 0.);

    return max(1 - x2 * x2 * (3 - 2 * x2), 0.);
}

fixed4 _NearPlaneTransitionDistance;

inline fixed ComputeNearPlaneTransition(fixed4 vertex, fixed fadeEnd, fixed fadeRange)
{
    fixed distToCamera = -UnityObjectToViewPos(vertex).z;
    return smoothstep(fadeEnd, fadeEnd + fadeRange, distToCamera);
}

fixed4 getTransition(fixed3 world, fixed3 center, fixed3 direction, fixed transitionOffset, fixed transitionWidth, fixed detailedTransitionWidth)
{
    fixed4 transition = 1;
#if SPHERICAL_PULSE				
				transition.xy = cubicPulse(transitionOffset, fixed2(transitionWidth, transitionWidth * detailedTransitionWidth), distance(center, world));
#endif
#if LINEAR_PULSE
				transition.xy = cubicPulse(transitionOffset, fixed2(transitionWidth, transitionWidth * detailedTransitionWidth), dot(world, normalize(direction.xyz)));
#endif
#if SPHERICAL_WIPE				
				transition.xy = smoothstep(transitionOffset + fixed2(transitionWidth, transitionWidth * detailedTransitionWidth) + 0.001, transitionOffset - fixed2(transitionWidth, transitionWidth * detailedTransitionWidth), distance(center, world));
#endif
#if LINEAR_WIPE
				transition.xy = smoothstep(transitionOffset + fixed2(transitionWidth, transitionWidth * detailedTransitionWidth) + 0.001, transitionOffset - fixed2(transitionWidth, transitionWidth * detailedTransitionWidth), dot(world, normalize(direction.xyz)));
#endif
    return transition;
}

fixed4 getPulse(fixed3 world, fixed3 center, fixed transitionOffset, fixed transitionWidth, fixed detailedTransitionWidth)
{
    fixed4 transition = 1;
    transition.xy = cubicPulse(transitionOffset, fixed2(transitionWidth, transitionWidth * detailedTransitionWidth), distance(center, world));
    return transition;
}

#endif // HOLOCP_INCLUDED
