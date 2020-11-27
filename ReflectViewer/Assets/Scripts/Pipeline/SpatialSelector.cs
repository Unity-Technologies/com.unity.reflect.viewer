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

        readonly RaycastHit[] m_RaycastHitCache = new RaycastHit[k_MaxResultsCount];
        readonly List<ISpatialObject> m_SpatialObjects = new List<ISpatialObject>();
        internal readonly List<Collider> m_Colliders = new List<Collider>();

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
            // only add colliders if there aren't any present, keep track of them to destroy when we're done
            if (obj.GetComponent<MeshRenderer>() != null && obj.GetComponent<Collider>() == null )
            {
                m_Colliders.Add(obj.AddComponent<MeshCollider>());
            }
            foreach (Transform childTransform  in obj.transform)
            {
                AddMeshColliderRecursively(childTransform.gameObject);
            }
        }

        protected virtual void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));

            foreach (var c in m_Colliders)
                Object.Destroy(c);

            m_Colliders.Clear();
        }

        public void Pick(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            var transformedRay = new Ray(WorldRoot.InverseTransformPoint(ray.origin), WorldRoot.InverseTransformDirection(ray.direction));
            // narrow down the possible objects using the spatial picker
            SpatialPicker.Pick(transformedRay, m_SpatialObjects);

            PreRaycast();
            RaycastInternal(ray, results);
            PostRaycast(results);
        }

        void RaycastInternal(Ray ray, List<Tuple<GameObject, RaycastHit>> results)
        {
            results.Clear();
            var count = Physics.RaycastNonAlloc(ray, m_RaycastHitCache);
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
                var count = Physics.RaycastNonAlloc(rayData.ray, m_RaycastHitCache, rayData.length);
                for (var i = 0; i < count; ++i)
                {
                    m_RaycastHitCache[i].distance += totalDistance;
                    results.Add(new Tuple<GameObject, RaycastHit>(m_RaycastHitCache[i].collider.gameObject, m_RaycastHitCache[i]));
                }
                totalDistance += rayData.length;
            }
        }
    }
}
