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
}
