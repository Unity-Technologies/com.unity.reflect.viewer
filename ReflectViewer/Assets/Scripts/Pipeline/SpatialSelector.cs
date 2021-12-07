using System;
using System.Collections.Generic;
using Unity.Reflect.Collections;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer
{
    public class SpatialSelector : ISpatialPicker<Tuple<GameObject, RaycastHit>>, ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>, IPicker, IDisposable
    {
        struct RayData
        {
            public Ray ray;
            public float length;
        }

        public SpatialSelector()
        {
            m_WalkModeEnableGetter =  UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled));
            m_NavigationModeGetter =  UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode));
        }

        const int k_MaxResultsCount = 10;
        IUISelector<bool> m_WalkModeEnableGetter;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeGetter;
        static int k_ObjectsLayer => ~LayerMask.GetMask("Avatars"); // All layers but avatars, can change this
        static readonly RaycastHit k_BlankRaycastHit = new RaycastHit();

        readonly RaycastHit[] m_RaycastHitCache = new RaycastHit[k_MaxResultsCount];
        readonly List<ISpatialObject> m_SpatialObjects = new List<ISpatialObject>();
        internal Dictionary<int, MeshCollider> m_ColliderCache = new Dictionary<int, MeshCollider>();
        internal HashSet<int> m_SyncObjectCache = new HashSet<int>();
        bool m_IsPicking;

        RayData[] m_RayDatas;

        [Obsolete("Please use 'SpatialPickerAsync' instead.")]
        public ISpatialPicker<ISpatialObject> SpatialPicker { get; set; }
        public ISpatialPickerAsync<ISpatialObject> SpatialPickerAsync { get; set; }
        public Transform WorldRoot { get; set; }

        void PreRaycast(List<ISpatialObject> spatialObjects)
        {
            // use colliders for more precision than just the bounding boxes
            foreach (var obj in spatialObjects)
            {
                // ignore objects that haven't been loaded yet and objects whose colliders we already know of
                if (obj.LoadedObject == null)
                    continue;

                // add colliders to all children object too
                AddMeshColliderRecursively(obj.LoadedObject);
            }
        }

        void AddMeshColliderRecursively(GameObject obj)
        {
            if (!m_ColliderCache.ContainsKey(obj.GetInstanceID()) && !m_SyncObjectCache.Contains(obj.GetInstanceID()))
            {
                // only add colliders if there aren't any present, keep track of them to destroy when we're done
                if (obj.gameObject.GetComponent<MeshRenderer>() != null && obj.gameObject.GetComponent<Collider>() == null)
                {
                    m_ColliderCache.Add(obj.GetInstanceID(), obj.AddComponent<MeshCollider>());
                }
                else
                {
                    m_SyncObjectCache.Add(obj.GetInstanceID());
                }
            }

            foreach (Transform childTransform in obj.transform)
            {
                AddMeshColliderRecursively(childTransform.gameObject);
            }
        }

        protected virtual void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));

            // delay cache cleaning in VR mode, must be trigger manually
            if (m_NavigationModeGetter.GetValue() != SetNavigationModeAction.NavigationMode.VR
                && !m_WalkModeEnableGetter.GetValue())
                CleanCache();
        }

        [Obsolete("Please use 'Pick(Ray ray, Action<List<Tuple<GameObject, RaycastHit>>> callback)' instead.")]
        public void Pick(Ray ray, List<Tuple<GameObject, RaycastHit>> results, string[] flagsExcluded = null)
        {
            var transformedRay = new Ray(WorldRoot.InverseTransformPoint(ray.origin), WorldRoot.InverseTransformDirection(ray.direction));

            // narrow down the possible objects using the spatial picker
            SpatialPicker.Pick(transformedRay, m_SpatialObjects, flagsExcluded);

            PreRaycast(m_SpatialObjects);
            RaycastInternal(ray, results);
            PostRaycast(results);
        }

        public void Pick(Ray ray, Action<List<Tuple<GameObject, RaycastHit>>> callback, string[] flagsExcluded = null)
        {
            var transformedRay = new Ray(WorldRoot.InverseTransformPoint(ray.origin), WorldRoot.InverseTransformDirection(ray.direction));

            // narrow down the possible objects using the spatial picker
            SpatialPickerAsync.Pick(transformedRay, list =>
            {
                PreRaycast(list);
                var results = new List<Tuple<GameObject, RaycastHit>>();
                RaycastInternal(ray, results);
                PostRaycast(results);
                callback(results);
            }, flagsExcluded);
        }

        public void FloorPick(Ray ray, Action<List<ISpatialObject>> callback)
        {
            var transformedRay = new Ray(WorldRoot.InverseTransformPoint(ray.origin), WorldRoot.InverseTransformDirection(ray.direction));

            // narrow down the possible objects using the spatial picker
            SpatialPickerAsync.Pick(transformedRay, callback);
        }

        [Obsolete("Please use 'Pick(Ray ray, List<Tuple<GameObject, RaycastHit>> results)' instead.")]
        public void VRPick(Ray ray, List<Tuple<GameObject, RaycastHit>> results, string[] flagsExcluded = null)
        {
            Pick(ray, results, flagsExcluded);
        }

        public void CleanCache()
        {
            if (m_IsPicking)
                return;

            foreach (var col in m_ColliderCache)
            {
                Object.Destroy(col.Value);
            }

            m_SyncObjectCache.Clear();
            m_ColliderCache.Clear();
        }

        public void SetPicking(bool value)
        {
            m_IsPicking = value;
        }

        void RaycastInternal(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Clear();
            var count = Physics.RaycastNonAlloc(ray, m_RaycastHitCache, float.MaxValue, k_ObjectsLayer);
            for (var i = 0; i < count; ++i)
                results.Add(new Tuple<GameObject, RaycastHit>(m_RaycastHitCache[i].collider.gameObject, m_RaycastHitCache[i]));
        }

        void PrePickSamplePoints(Vector3[] samplePoints, int samplePointCount)
        {
            // early exit if 2 sample points or less
            if (samplePointCount <= 2)
            {
                Debug.LogError($"[{nameof(SpatialSelector)}] cannot use Raycast(Vector3[] samplePoints, [...]) " +
                    "with 2 or less sample points! Try using Raycast(Ray ray, [...]) instead!");
                return;
            }

            // init chain of rays from one sample point to the next
            if (m_RayDatas == null || m_RayDatas.Length != samplePointCount - 1)
                m_RayDatas = new RayData[samplePointCount - 1];

            for (var i = 0; i < m_RayDatas.Length; ++i)
            {
                m_RayDatas[i].ray.origin = samplePoints[i];
                var diff = samplePoints[i + 1] - samplePoints[i];
                m_RayDatas[i].ray.direction = diff;
                m_RayDatas[i].length = diff.magnitude;
            }
        }

        [Obsolete("Please use 'Pick(Vector3[] samplePoints, int samplePointCount, Action<List<Tuple<GameObject, RaycastHit>>> callback)' instead.")]
        public void Pick(Vector3[] samplePoints, int samplePointCount, List<Tuple<GameObject, RaycastHit>> results, string[] flagsExcluded = null)
        {
            PrePickSamplePoints(samplePoints, samplePointCount);

            // narrow down the possible objects using the spatial picker
            SpatialPicker.Pick(samplePoints, samplePointCount, m_SpatialObjects, flagsExcluded);

            PreRaycast(m_SpatialObjects);
            RaycastSamplePointsInternal(results);
            PostRaycast(results);
        }

        public void Pick(Vector3[] samplePoints, int samplePointCount, Action<List<Tuple<GameObject, RaycastHit>>> callback, string[] flagsExcluded = null)
        {
            PrePickSamplePoints(samplePoints, samplePointCount);

            // narrow down the possible objects using the spatial picker
            SpatialPickerAsync.Pick(samplePoints, samplePointCount, list =>
            {
                PreRaycast(list);
                var results = new List<Tuple<GameObject, RaycastHit>>();
                RaycastSamplePointsInternal(results);
                PostRaycast(results);
                callback(results);
            }, flagsExcluded);
        }

        void RaycastSamplePointsInternal(List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Clear();
            var totalDistance = 0f;
            foreach (var rayData in m_RayDatas)
            {
                var count = Physics.RaycastNonAlloc(rayData.ray, m_RaycastHitCache, rayData.length, k_ObjectsLayer);
                for (var i = 0; i < count; ++i)
                {
                    m_RaycastHitCache[i].distance += totalDistance;
                    results.Add(new Tuple<GameObject, RaycastHit>(m_RaycastHitCache[i].collider.gameObject, m_RaycastHitCache[i]));
                }
                totalDistance += rayData.length;
            }
        }

        [Obsolete("Please use 'Pick(Vector3 origin, float distance, Action<List<Tuple<GameObject, RaycastHit>>> callback)' instead.")]
        public void Pick(float distance, List<Tuple<GameObject, RaycastHit>> results, Transform origin, string[] flagsExcluded = null)
        {
            results.Clear();
            SpatialPicker.Pick(distance, m_SpatialObjects, origin, flagsExcluded);
            foreach (var spatialObject in m_SpatialObjects)
            {
                if (spatialObject.LoadedObject == null)
                    continue;

                results.Add(new Tuple<GameObject, RaycastHit>(spatialObject.LoadedObject, k_BlankRaycastHit));
            }
        }

        public void Pick(Vector3 origin, float distance, Action<List<Tuple<GameObject, RaycastHit>>> callback, string[] flagsExcluded = null)
        {
            SpatialPickerAsync.Pick(origin, distance, list =>
            {
                var results = new List<Tuple<GameObject, RaycastHit>>();
                foreach (var spatialObject in list)
                {
                    if (spatialObject.LoadedObject == null)
                        continue;

                    results.Add(new Tuple<GameObject, RaycastHit>(spatialObject.LoadedObject, k_BlankRaycastHit));
                }

                callback(results);
            }, flagsExcluded);
        }

        public void Dispose()
        {
            m_WalkModeEnableGetter?.Dispose();
            m_NavigationModeGetter?.Dispose();
        }
    }
}
