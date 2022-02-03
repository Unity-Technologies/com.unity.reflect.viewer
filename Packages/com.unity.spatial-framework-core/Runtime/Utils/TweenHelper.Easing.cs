using System;
using UnityEngine;

namespace Unity.Tweening
{
    /// <summary>
    /// Utility methods for creating simple Tweens.
    /// </summary>
    /// <remarks>This file contains special functions for processing easing.</remarks>
    public static partial class TweenHelper
    {
        const float k_Pi = Mathf.PI;

        const float k_HalfPi = Mathf.PI / 2.0f;

        /// <summary>
        /// Converts a linear, [0,1] value to the value at the same animation time in a giving easing type.
        /// </summary>
        /// <remarks>
        /// See the EasingVisualizer unity scene in the Spatial Framework package to view each ease type.
        /// </remarks>
        /// <param name="linearValue">The [0,1] linear value. If outside of that range, the value will be clamped.</param>
        /// <param name="ease">The type of easing being performed.</param>
        /// <returns>An point on an interpolated line.</returns>
        /// <exception cref="ArgumentException">Will throw an error if the ease type is of an unknown type.</exception>
        public static float CalculateEasing(float linearValue, EaseType ease)
        {
            // Clamp to functional [0,1] range
            linearValue = Mathf.Clamp(linearValue, 0.0f, 1.0f);

            switch (ease)
            {
                case EaseType.Linear:
                    return linearValue;
                case EaseType.QuadraticEaseOut:
                    return QuadraticEaseOut(linearValue);
                case EaseType.QuadraticEaseIn:
                    return QuadraticEaseIn(linearValue);
                case EaseType.QuadraticEaseInOut:
                    return QuadraticEaseInOut(linearValue);
                case EaseType.CubicEaseIn:
                    return CubicEaseIn(linearValue);
                case EaseType.CubicEaseOut:
                    return CubicEaseOut(linearValue);
                case EaseType.CubicEaseInOut:
                    return CubicEaseInOut(linearValue);
                case EaseType.QuarticEaseIn:
                    return QuarticEaseIn(linearValue);
                case EaseType.QuarticEaseOut:
                    return QuarticEaseOut(linearValue);
                case EaseType.QuarticEaseInOut:
                    return QuarticEaseInOut(linearValue);
                case EaseType.QuinticEaseIn:
                    return QuinticEaseIn(linearValue);
                case EaseType.QuinticEaseOut:
                    return QuinticEaseOut(linearValue);
                case EaseType.QuinticEaseInOut:
                    return QuinticEaseInOut(linearValue);
                case EaseType.SineEaseIn:
                    return SineEaseIn(linearValue);
                case EaseType.SineEaseOut:
                    return SineEaseOut(linearValue);
                case EaseType.SineEaseInOut:
                    return SineEaseInOut(linearValue);
                case EaseType.CircularEaseIn:
                    return CircularEaseIn(linearValue);
                case EaseType.CircularEaseOut:
                    return CircularEaseOut(linearValue);
                case EaseType.CircularEaseInOut:
                    return CircularEaseInOut(linearValue);
                case EaseType.ExponentialEaseIn:
                    return ExponentialEaseIn(linearValue);
                case EaseType.ExponentialEaseOut:
                    return ExponentialEaseOut(linearValue);
                case EaseType.ExponentialEaseInOut:
                    return ExponentialEaseInOut(linearValue);
                case EaseType.ElasticEaseIn:
                    return ElasticEaseIn(linearValue);
                case EaseType.ElasticEaseOut:
                    return ElasticEaseOut(linearValue);
                case EaseType.ElasticEaseInOut:
                    return ElasticEaseInOut(linearValue);
                case EaseType.BackEaseIn:
                    return BackEaseIn(linearValue);
                case EaseType.BackEaseOut:
                    return BackEaseOut(linearValue);
                case EaseType.BackEaseInOut:
                    return BackEaseInOut(linearValue);
                case EaseType.BounceEaseIn:
                    return BounceEaseIn(linearValue);
                case EaseType.BounceEaseOut:
                    return BounceEaseOut(linearValue);
                case EaseType.BounceEaseInOut:
                    return BounceEaseInOut(linearValue);
                default:
                    throw new ArgumentException("Unknown EaseType passed to TweenUtils", nameof(ease));
            }
        }

        static float QuadraticEaseIn(float t)
        {
            return t * t;
        }

        static float QuadraticEaseOut(float t)
        {
            return -(t * (t - 2f));
        }

        static float QuadraticEaseInOut(float t)
        {
            if (t < 0.5f)
                return 2f * t * t;

            return (-2f * t * t) + (4f * t) - 1f;
        }

        static float CubicEaseIn(float t)
        {
            return t * t * t;
        }

        static float CubicEaseOut(float t)
        {
            var f = (t - 1f);
            return f * f * f + 1f;
        }

        static float CubicEaseInOut(float t)
        {
            if (t < 0.5f)
                return 4f * t * t * t;

            var f = ((2f * t) - 2f);
            return 0.5f * f * f * f + 1f;
        }

        static float QuarticEaseIn(float t)
        {
            return t * t * t * t;
        }

        static float QuarticEaseOut(float t)
        {
            var f = (t - 1f);
            return f * f * f * (1f - t) + 1f;
        }

        static float QuarticEaseInOut(float t)
        {
            if (t < 0.5f)
                return 8f * t * t * t * t;

            var f = (t - 1f);
            return -8f * f * f * f * f + 1f;
        }

        static float QuinticEaseIn(float t)
        {
            return t * t * t * t * t;
        }

        static float QuinticEaseOut(float t)
        {
            var f = (t - 1f);
            return f * f * f * f * f + 1f;
        }

        static float QuinticEaseInOut(float t)
        {
            if (t < 0.5f)
                return 16f * t * t * t * t * t;

            var f = ((2f * t) - 2f);
            return 0.5f * f * f * f * f * f + 1f;
        }

        static float SineEaseIn(float t)
        {
            return Mathf.Sin((t - 1f) * k_HalfPi) + 1f;
        }

        static float SineEaseOut(float t)
        {
            return Mathf.Sin(t * k_HalfPi);
        }

        static float SineEaseInOut(float t)
        {
            return 0.5f * (1f - Mathf.Cos(t * k_Pi));
        }

        static float CircularEaseIn(float t)
        {
            return 1f - Mathf.Sqrt(1f - (t * t));
        }

        static float CircularEaseOut(float t)
        {
            return Mathf.Sqrt((2f - t) * t);
        }

        static float CircularEaseInOut(float t)
        {
            if (t < 0.5f)
                return 0.5f * (1f - Mathf.Sqrt(1f - 4f * (t * t)));

            return 0.5f * (Mathf.Sqrt(-((2f * t) - 3f) * ((2f * t) - 1f)) + 1f);
        }

        static float ExponentialEaseIn(float t)
        {
            return Mathf.Approximately(t, 0.0f) ? t : Mathf.Pow(2f, 10 * (t - 1));
        }

        static float ExponentialEaseOut(float t)
        {
            return Mathf.Approximately(t, 1.0f) ? t : (1f - Mathf.Pow(2f, -10f * t));
        }

        static float ExponentialEaseInOut(float t)
        {
            if (Mathf.Approximately(t, 0.0f) || Mathf.Approximately(t, 1.0f))
                return t;

            if (t < 0.5f)
                return 0.5f * Mathf.Pow(2f, (20f * t) - 10f);

            return -0.5f * Mathf.Pow(2f, (-20f * t) + 10f) + 1f;
        }

        static float ElasticEaseIn(float t)
        {
            return Mathf.Sin(13f * k_HalfPi * t) * Mathf.Pow(2f, 10f * (t - 1f));
        }

        static float ElasticEaseOut(float t)
        {
            return Mathf.Sin(-13f * k_HalfPi * (t + 1f)) * Mathf.Pow(2f, -10f * t) + 1f;
        }

        static float ElasticEaseInOut(float t)
        {
            if (t < 0.5f)
                return 0.5f * Mathf.Sin(13f * k_HalfPi * (2f * t)) * Mathf.Pow(2f, 10f * ((2f * t) - 1));

            return 0.5f * (Mathf.Sin(-13f * k_HalfPi * ((2f * t - 1f) + 1f)) * Mathf.Pow(2f, -10f * (2f * t - 1f)) + 2f);
        }

        static float BackEaseIn(float t)
        {
            return (t * t * t - t * Mathf.Sin(t * k_Pi));
        }

        static float BackEaseOut(float t)
        {
            var f = (1f - t);
            return (1f - (f * f * f - f * Mathf.Sin(f * k_Pi)));
        }

        static float BackEaseInOut(float t)
        {
            if (t < 0.5f)
            {
                var f = 2f * t;
                return 0.5f * (f * f * f - f * Mathf.Sin(f * k_Pi));
            }

            var val = (1f - (2f*t - 1f));
            return 0.5f * (1f - (val * val * val - val * Mathf.Sin(val * k_Pi))) + 0.5f;
        }

        static float BounceEaseIn(float t)
        {
            return 1f - BounceEaseOut(1f - t);
        }

        static float BounceEaseOut(float t)
        {
            const float d1 = 2.75f;

            const float firstBounce = 1f / d1;
            const float secondBounce = 2f / d1;
            const float thirdBounce = 2.5f / d1;

            if (t < firstBounce)
                return 7.5625f * t * t;

            if (t < secondBounce)
                return (9.075f * t * t) - (9.9f * t) + 3.4f;

            if (t < thirdBounce)
                return (12.066f * t * t) - (19.635f * t) + 8.898f;

            return (10.8f * t * t) - (20.52f * t) + 10.72f;
        }

        static float BounceEaseInOut(float t)
        {
            if (t < 0.5f)
            {
                return 0.5f * BounceEaseIn(t * 2f);
            }

            return 0.5f * BounceEaseOut(t * 2f - 1f) + 0.5f;
        }
    }
}
