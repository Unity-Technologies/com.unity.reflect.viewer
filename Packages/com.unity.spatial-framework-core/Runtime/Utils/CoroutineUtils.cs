using System.Collections;
using UnityEngine;

namespace Unity.SpatialFramework.Utils
{
    /// <summary>
    /// Utility methods to stop and restart coroutines
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// Stops referenced coroutine.
        /// </summary>
        /// <param name="mb">This MonoBehaviour that contains the coroutine</param>
        /// <param name="coroutine">coroutine reference to stop running</param>
        public static void StopCoroutine(this MonoBehaviour mb, ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                mb.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        /// <summary>
        /// Starts a coroutine with enumerator
        /// </summary>
        /// <param name="mb">This MonoBehaviour that contains the coroutine</param>
        /// <param name="coroutine">coroutine reference to start running</param>
        /// <param name="routine">IEnumerator to run in the referenced coroutine</param>
        public static void RestartCoroutine(this MonoBehaviour mb, ref Coroutine coroutine, IEnumerator routine)
        {
            mb.StopCoroutine(ref coroutine);
            if (mb.gameObject.activeInHierarchy)
                coroutine = mb.StartCoroutine(routine);
        }
    }
}
