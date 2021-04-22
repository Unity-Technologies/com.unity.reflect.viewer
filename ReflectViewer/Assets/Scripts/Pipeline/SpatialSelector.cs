using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.Viewer
{
    public class SpatialSelector : ISpatialPicker<Tuple<GameObject, RaycastHit>>
    {
        struct RayData
        {
            public Ray ray;
            public float length;
        }

        const int k_MaxResultsCount = 10;

        static int k_ObjectsLayer => ~LayerMask.GetMask("Avatars"); // All layers but avatars, can change this

        readonly RaycastHit[] m_RaycastHitCache = new RaycastHit[k_MaxResultsCount];
        readonly List<ISpatialObject> m_SpatialObjects = new List<ISpatialObject>();
        internal Dictionary<int, MeshCollider> m_ColliderCache = new Dictionary<int, MeshCollider>();

        RayData[] m_RayDatas;

        public ISpatialPicker<ISpatialObject> SpatialPicker { get; set; }
        public Transform WorldRoot { get; set; }

        void PreRaycast()
        {
            // use colliders for more precision than just the bounding boxes
            foreach (var obj in m_SpatialObjects)
            {
                // ignore objects that haven't been loaded yet and objects whose colliders we already know of
                if (obj.loadedObject == null)
                    continue;

                // add colliders to all children object too
                AddMeshColliderRecursively(obj.loadedObject);
            }
        }

        void AddMeshColliderRecursively(GameObject obj)
        {
            if (!m_ColliderCache.ContainsKey(obj.GetInstanceID()))
            {
                // only add colliders if there aren't any present, keep track of them to destroy when we're done
                if (obj.GetComponent<MeshRenderer>() != null && obj.GetComponent<Collider>() == null)
                {
                    m_ColliderCache.Add(obj.GetInstanceID(), obj.AddComponent<MeshCollider>());
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

            foreach (var c in m_ColliderCache.Values)
                Object.Destroy(c);

            m_ColliderCache.Clear();
        }

        public void Pick(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            PrePicking(ray, results);
            PostRaycast(results);
        }

        void PrePicking(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            var transformedRay = new Ray(WorldRoot.InverseTransformPoint(ray.origin), WorldRoot.InverseTransformDirection(ray.direction));

            // narrow down the possible objects using the spatial picker
            SpatialPicker.Pick(transformedRay, m_SpatialObjects);

            PreRaycast();
            RaycastInternal(ray, results);
        }

        public void CleanCache()
        {
            foreach (var col in m_ColliderCache)
            {
                Object.Destroy(col.Value);
            }

            m_ColliderCache.Clear();
        }

        public void VRPick(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            PrePicking(ray, results);
            results.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));
        }

        void RaycastInternal(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Clear();
            var count = Physics.RaycastNonAlloc(ray, m_RaycastHitCache, float.MaxValue, k_ObjectsLayer);
            for (var i = 0; i < count; ++i)
                results.Add(new Tuple<GameObject, RaycastHit>(m_RaycastHitCache[i].collider.gameObject, m_RaycastHitCache[i]));
        }

        public void Pick(Vector3[] samplePoints, int samplePointCount, List<Tuple<GameObject, RaycastHit>> results)
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

            // narrow down the possible objects using the spatial picker
            SpatialPicker.Pick(samplePoints, samplePointCount, m_SpatialObjects);

            PreRaycast();
            RaycastSamplePointsInternal(results);
            PostRaycast(results);
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

        public void Pick(float distance, List<Tuple<GameObject, RaycastHit>> results, Transform origin)
        {
            results.Clear();
            SpatialPicker.Pick(distance, m_SpatialObjects, origin);
            foreach (var spatialObject in m_SpatialObjects)
            {
                if (spatialObject.loadedObject == null)
                    continue;

                results.Add(new Tuple<GameObject, RaycastHit>(spatialObject.loadedObject, new RaycastHit()));
            }
        }
    }
}
