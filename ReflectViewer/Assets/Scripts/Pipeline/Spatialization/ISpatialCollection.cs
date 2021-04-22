using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public interface ISpatialCollection<TObject> : IDisposable
    {
        int objectCount { get; }
        int depth { get; }
        Bounds bounds { get; }

        void Search<T>(Func<TObject, bool> predicate,
            Func<TObject, float> prioritizer,
            int maxResultsCount,
            ICollection<T> results) where T : TObject;

        void Add(TObject obj);
        void Remove(TObject obj);

        // TODO: this should be more generic (not necessarily a tree)
        void DrawDebug(Gradient nodeGradient, Gradient objectGradient, float maxPriority, int maxDepth);
    }

    public interface ISpatialObject : IDisposable
    {
        Vector3 min { get; }
        Vector3 max { get; }
        Vector3 center { get; }
        float priority { get; set; }
        bool isVisible { get; set; }
        GameObject loadedObject { get; set; }
    }

    public interface ISpatialPicker<T>
    {
        void Pick(Ray ray, List<T> results);
        void VRPick(Ray ray, List<T> results);
        void Pick(Vector3[] samplePoints, int samplePointCount, List<T> results);
        void Pick(float distance, List<T> results, Transform origin);
    }
}
