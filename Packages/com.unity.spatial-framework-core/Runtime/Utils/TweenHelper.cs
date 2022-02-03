using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Tweening
{
    /// <summary>
    /// Utility methods for creating simple Tweens
    /// </summary>
    public static partial class TweenHelper
    {
        /// <summary>
        /// Easing type that can be used to modify how a value is interpolated.  For descriptions of each type see <see href="https://easings.net/">Easing Cheat Sheet</see>.
        /// </summary>
        public enum EaseType
        {
            Linear,
            QuadraticEaseIn,
            QuadraticEaseOut,
            QuadraticEaseInOut,
            CubicEaseIn,
            CubicEaseOut,
            CubicEaseInOut,
            QuarticEaseIn,
            QuarticEaseOut,
            QuarticEaseInOut,
            QuinticEaseIn,
            QuinticEaseOut,
            QuinticEaseInOut,
            SineEaseIn,
            SineEaseOut,
            SineEaseInOut,
            CircularEaseIn,
            CircularEaseOut,
            CircularEaseInOut,
            ExponentialEaseIn,
            ExponentialEaseOut,
            ExponentialEaseInOut,
            ElasticEaseIn,
            ElasticEaseOut,
            ElasticEaseInOut,
            BackEaseIn,
            BackEaseOut,
            BackEaseInOut,
            BounceEaseIn,
            BounceEaseOut,
            BounceEaseInOut
        };

        /// <summary>
        /// Creates an IEnumerator that performs a scale tween on the specified <see cref="Transform"/>. The starting point is the current scale.
        /// </summary>
        /// <remarks>If the <see cref="Transform"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Transform"/> results in undefined behaviour.</remarks>
        /// <param name="transform">The Transform to scale.</param>
        /// <param name="targetScale">The desired scale</param>
        /// <param name="duration">The length of time, in seconds, to take to scale to the target scale.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenScale(this Transform transform, Vector3 targetScale, float duration, EaseType easeType = EaseType.Linear)
        {
            var startScale = transform.localScale;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (transform == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// Creates an IEnumerator that changes the color of an <see cref="Image"/> over time. The starting point is the current color.
        /// </summary>
        /// <remarks>Uses Color.Lerp to interpolate. If the <see cref="Image"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Image"/> results in undefined behaviour.</remarks>
        /// <param name="image">The image to recolor.</param>
        /// <param name="targetColor">The desired end color.</param>
        /// <param name="duration">The length of time, in seconds, to take to reach the target destination.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenColor(this Image image, Color targetColor, float duration, EaseType easeType = EaseType.Linear)
        {
            var startColor = image.color;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (image == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                image.color = Color.LerpUnclamped(startColor, targetColor, t);
                yield return null;
            }

            image.color = targetColor;
        }

        /// <summary>
        /// Creates an IEnumerator that rotates a <see cref="Transform"/> in it's local space over a set duration. The starting point is the current rotation.
        /// </summary>
        /// <remarks>Uses Quaternion.Slerp to interpolate. If the <see cref="Transform"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Transform"/> results in undefined behaviour.</remarks>
        /// <param name="transform">The Transform to translate.</param>
        /// <param name="targetRotation">The desired end rotation in local space.</param>
        /// <param name="duration">The length of time, in seconds, to take to reach the target destination.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenRotate(this Transform transform, Quaternion targetRotation, float duration, EaseType easeType = EaseType.Linear)
        {
            var start = transform.localRotation;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (transform == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                transform.localRotation = Quaternion.SlerpUnclamped(start, targetRotation, t);
                yield return null;
            }

            transform.localRotation = targetRotation;
        }

        /// <summary>
        /// Creates an IEnumerator that moves a <see cref="Transform"/> in it's local space over a set duration. The starting point is the current local position.
        /// </summary>
        /// <remarks>If the <see cref="Transform"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Transform"/> results in undefined behaviour.</remarks>
        /// <param name="transform">The Transform to translate.</param>
        /// <param name="targetPosition">The desired end location in local space.</param>
        /// <param name="duration">The length of time, in seconds, to take to reach the target destination.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenMove(this Transform transform, Vector3 targetPosition, float duration, EaseType easeType = EaseType.Linear)
        {
            var start = transform.localPosition;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (transform == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                var localPosition = Vector3.LerpUnclamped(start, targetPosition, t);
                transform.localPosition = localPosition;
                yield return null;
            }

            transform.localPosition = targetPosition;
        }

        /// <summary>
        /// Creates an IEnumerator that moves a <see cref="Transform"/> along the X axis in it's local space over a set duration. The starting point is the current local position.
        /// </summary>
        /// <remarks>If the <see cref="Transform"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Transform"/> results in undefined behaviour.</remarks>
        /// <param name="transform">The Transform to translate.</param>
        /// <param name="targetPosition">The desired end position along the X-axis in local space.</param>
        /// <param name="duration">The length of time, in seconds, to take to reach the target destination.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenMoveX(this Transform transform, float targetPosition, float duration, EaseType easeType = EaseType.Linear)
        {
            var localPosition = transform.localPosition;
            var startX = localPosition.x;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (transform == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                // Refresh local position so as to not squash movements in other axes.
                localPosition = transform.localPosition;
                localPosition.x = Mathf.LerpUnclamped(startX, targetPosition, t);
                transform.localPosition = localPosition;
                yield return null;
            }

            localPosition.x = targetPosition;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Creates an IEnumerator that moves a <see cref="Transform"/> along the Z axis in it's local space over a set duration. The starting point is the current local position.
        /// </summary>
        /// <remarks>If the <see cref="Transform"/> is destroyed, the tween will silently quit. Multiple tweens with the same <see cref="Transform"/> results in undefined behaviour.</remarks>
        /// <param name="transform">The Transform to translate.</param>
        /// <param name="targetPosition">The desired end position along the Z-axis in local space.</param>
        /// <param name="duration">The length of time, in seconds, to take to reach the target destination.</param>
        /// <param name="easeType">The type of easing to use. Defaults to <see cref="EaseType.Linear"/></param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator TweenMoveZ(this Transform transform, float targetPosition, float duration, EaseType easeType = EaseType.Linear)
        {
            var localPosition = transform.localPosition;
            var startZ = localPosition.z;
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Early out if object is destroyed.
                if (transform == null)
                    yield break;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                // Refresh local position so as to not squash movements in other axes.
                localPosition = transform.localPosition;
                localPosition.z = Mathf.LerpUnclamped(startZ, targetPosition, t);
                transform.localPosition = localPosition;
                yield return null;
            }

            localPosition.z = targetPosition;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// A simple coroutine that will call a callback every frame over a set duration that returns an interpolated value that can be used for simple animations.
        /// </summary>
        /// <param name="callback">The callback to call every frame.</param>
        /// <param name="duration">The duration to run this interpolation for.</param>
        /// <param name="easeType">The type of easing on the value passed to the callback.  Defaults to Linear (0 to 1 at a constant rate).</param>
        /// <returns>Returns an IEnumerator that can be passed to <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static IEnumerator Interpolate(Action<float> callback, float duration, EaseType easeType = EaseType.Linear)
        {
            var startTime = Time.realtimeSinceStartup;
            var elapsed = 0f;
            while (elapsed <= duration)
            {
                yield return null;

                elapsed = Time.realtimeSinceStartup - startTime;
                var t = elapsed / duration;
                t = CalculateEasing(t, easeType);

                callback?.Invoke(t);
            }
        }
    }
}
