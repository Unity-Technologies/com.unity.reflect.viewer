using System;
using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    [ScriptableSettingsPath("Assets/SpatialFramework/Settings")]
    public class TemporaryUIModule : ScriptableSettings<TemporaryUIModule>, IModule, IUsesViewerScale
    {
#pragma warning disable 649
        [SerializeField, Tooltip("A prefab for labels. It must have a component that implements the interface ILabel")]
        GameObject m_LabelPrefab;

        [SerializeField, Tooltip("A prefab for line segments. It must have a component that implements the interface ILine")]
        GameObject m_LineSegmentPrefab;

        [SerializeField, Tooltip("A prefab for rectangle lines. It must have a component that implements the interface ILine")]
        GameObject m_LineRectPrefab;

        [SerializeField, Tooltip("A prefab for angles. It must have a component that implements the interface ILine")]
        GameObject m_AngleLinePrefab;

        [SerializeField, Tooltip("A prefab for guide lines. It must have a component that implements the interface ILine")]
        GameObject m_GuideLinePrefab;

        [SerializeField, Tooltip("The length for a guide line")]
        float m_GuideLineLength = 10f;

        [SerializeField, Tooltip("A prefab for guide circles. It must have a component that implements the interface ICircle")]
        GameObject m_GuideCirclePrefab;
#pragma warning restore 649

        Transform m_Parent;

        Queue<ILabel> m_LabelPool = new Queue<ILabel>(1);
        Dictionary<object, ILabel> m_ActiveLabels = new Dictionary<object, ILabel>();

        Queue<ILine> m_LineSegmentPool = new Queue<ILine>(1);
        Dictionary<object, ILine> m_ActiveLineSegments = new Dictionary<object, ILine>();

        Queue<ILine> m_LineRectPool = new Queue<ILine>(1);
        Dictionary<object, ILine> m_ActiveLineRects = new Dictionary<object, ILine>();

        Queue<ILine> m_AngleLinePool = new Queue<ILine>(1);
        Dictionary<object, ILine> m_ActiveAngleLines = new Dictionary<object, ILine>();

        Queue<ILine> m_GuideLinePool = new Queue<ILine>(1);
        Dictionary<object, ILine> m_ActiveGuideLines = new Dictionary<object, ILine>();

        Queue<ICircle> m_GuideCirclePool = new Queue<ICircle>(1);
        Dictionary<object, ICircle> m_ActiveGuideCircles = new Dictionary<object, ICircle>();

        IProvidesViewerScale IFunctionalitySubscriber<IProvidesViewerScale>.provider { get; set; }

        /// <summary>
        /// Adds a text label at a position in space
        /// </summary>
        /// <param name="id">The identifying object that is creating the label</param>
        /// <param name="text">A function that returns the current text for the label</param>
        /// <param name="position">A function that returns the current position for the label</param>
        public void AddLabel(object id, Func<string> text, Func<Vector3> position)
        {
            var label = GetPooledObject(id, m_ActiveLabels, m_LabelPool, m_LabelPrefab);
            label.getText = text;
            label.getPosition = position;
            label.active = true;
        }

        /// <summary>
        /// Removes a label created via AddLabel
        /// </summary>
        /// <param name="id">The identifying object that was used to add the label</param>
        public void RemoveLabel(object id)
        {
            var label = RecyclePooledObject(id, m_ActiveLabels, m_LabelPool);
            label.active = false;
        }

        /// <summary>
        /// Adds a line segment
        /// </summary>
        /// <param name="id">The identifying object that is creating the line segment.</param>
        /// <param name="start">Function that returns the current start point of the line segment</param>
        /// <param name="end">Function that returns the current end point of the line segment</param>
        public void AddLineSegment(object id, Func<Vector3> start, Func<Vector3> end)
        {
            var line = GetPooledObject(id, m_ActiveLineSegments, m_LineSegmentPool, m_LineSegmentPrefab);
            line.vertexCount = 2;
            line.getLinePositions = () =>
            {
                return new[] { start(), end() };
            };

            line.active = true;
        }

        /// <summary>
        /// Removes a line segment that was created via AddLineSegment
        /// </summary>
        /// <param name="id">The identifying object that was used to add the line segment</param>
        public void RemoveLineSegment(object id)
        {
            var line = RecyclePooledObject(id, m_ActiveLineSegments, m_LineSegmentPool);
            line.active = false;
        }

        /// <summary>
        /// Adds a line rectangle
        /// </summary>
        /// <param name="id">The identifying object that is creating the line rect.</param>
        /// <param name="min">Function that returns the current minimum corner of the rect</param>
        /// <param name="max">Function that returns the current maximum corner of the rect</param>
        /// <param name="rotation">Function that returns the rotation of the plane that the rectangle is on</param>
        public void AddLineRect(object id, Func<Vector3> min, Func<Vector3> max, Func<Quaternion> rotation)
        {
            var line = GetPooledObject(id, m_ActiveLineRects, m_LineRectPool, m_LineRectPrefab);
            line.vertexCount = 4;
            line.getLinePositions = () =>
            {
                var start = min();
                var end = max();
                var rot = rotation();
                var sides = Quaternion.Inverse(rot) * (end - start);
                return new[]
                {
                    start,
                    start + rot * Vector3.right * sides.x,
                    end,
                    start + rot * Vector3.up * sides.y,
                };
            };

            line.active = true;
        }

        /// <summary>
        /// Removes a line rectangle created via AddLineRect
        /// </summary>
        /// <param name="id">The identifying object that was used to add the line rectangle</param>
        public void RemoveLineRect(object id)
        {
            var line = RecyclePooledObject(id, m_ActiveLineRects, m_LineRectPool);
            line.active = false;
        }

        /// <summary>
        /// Adds an angle, i.e. '>', defined by 3 points
        /// </summary>
        /// <param name="id">The identifying object that is creating the angle</param>
        /// <param name="start">Function that returns the current start point</param>
        /// <param name="mid">Function that returns the current middle point</param>
        /// <param name="end">Function that returns the current end point</param>
        public void AddAngle(object id, Func<Vector3> start, Func<Vector3> mid, Func<Vector3> end)
        {
            var line = GetPooledObject(id, m_ActiveAngleLines, m_AngleLinePool, m_AngleLinePrefab);
            line.vertexCount = 3;
            line.getLinePositions = () =>
            {
                return new[] { start(), mid(), end() };
            };

            line.active = true;
        }

        /// <summary>
        /// Removes an angle created via AddAngle
        /// </summary>
        /// <param name="id">The identifying object that was used to add the angle</param>
        public void RemoveAngle(object id)
        {
            var line = RecyclePooledObject(id, m_ActiveAngleLines, m_AngleLinePool);
            line.active = false;
        }

        /// <summary>
        /// Adds a guide line
        /// </summary>
        /// <param name="id">The identifying object that is creating the guide line</param>
        /// <param name="center">Function that returns the center of the guide line</param>
        /// <param name="direction">Function that returns the direction of the guide line</param>
        public void AddGuideLine(object id, Func<Vector3> center, Func<Vector3> direction)
        {
            var line = GetPooledObject(id, m_ActiveGuideLines, m_GuideLinePool, m_GuideLinePrefab);
            line.vertexCount = 3;
            line.getLinePositions = () =>
            {
                var extents = direction() * m_GuideLineLength * this.GetViewerScale();
                return new[] { center() + extents, center(), center() - extents };
            };

            line.active = true;
        }

        /// <summary>
        /// Removes a guide line created via AddGuideLine
        /// </summary>
        /// <param name="id">The identifying object that was used to add the guide line</param>
        public void RemoveGuideLine(object id)
        {
            var line = RecyclePooledObject(id, m_ActiveGuideLines, m_GuideLinePool);
            line.active = false;
        }

        /// <summary>
        /// Adds a guide circle
        /// </summary>
        /// <param name="id">The identifying object that is creating the guide circle</param>
        /// <param name="center">Function that return the center point of the circle</param>
        /// <param name="start">Function that returns the starting point on the circle</param>
        /// <param name="normal">Function that returns the normal direction of the circle (direction orthogonal to the plane that the circle lies on)</param>
        public void AddGuideCircle(object id, Func<Vector3> center, Func<Vector3> start, Func<Vector3> normal)
        {
            var line = GetPooledObject(id, m_ActiveGuideCircles, m_GuideCirclePool, m_GuideCirclePrefab);
            line.getCenter = center;
            line.getNormal = normal;
            line.getRadius = () => start() - center();

            line.active = true;
        }

        /// <summary>
        /// Removes a guide circle created via AddGuideCircle
        /// </summary>
        /// <param name="id">The identifying object that was used to add the guide circle</param>
        public void RemoveGuideCircle(object id)
        {
            var line = RecyclePooledObject(id, m_ActiveGuideCircles, m_GuideCirclePool);
            line.active = false;
        }

        static T RecyclePooledObject<T>(object id, Dictionary<object, T> activeObjects, Queue<T> pool)
        {
            if (activeObjects.TryGetValue(id, out var obj))
            {
                activeObjects.Remove(id);
                pool.Enqueue(obj);
                return obj;
            }

            return default;
        }

        T GetPooledObject<T>(object id, Dictionary<object, T> usedObjects, Queue<T> pool, GameObject prefab) where T : IPooledUI
        {
            if (usedObjects.TryGetValue(id, out var obj))
            {
                return obj;
            }

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                var newGameObject = Instantiate(prefab, m_Parent);
                newGameObject.SetHideFlagsRecursively(HideFlags.DontSave);
                obj = newGameObject.GetComponent<T>();
                if (obj == null)
                {
                    Debug.LogError($"Prefab {prefab} does not have a component that implements the {typeof(T).Name} interface.");
                }
                else
                {
                    obj.active = false; // Object will be set active by the creator after other values are set
                }
            }

            usedObjects.Add(id, obj);
            return obj;
        }

        void IModule.LoadModule()
        {
            var moduleParent = ModuleLoaderCore.instance.GetModuleParent().transform;
            m_Parent = new GameObject("Temporary UI").transform;
            m_Parent.parent = moduleParent;
            m_Parent.gameObject.hideFlags = HideFlags.DontSave;
        }

        void IModule.UnloadModule()
        {
            UnityObjectUtils.Destroy(m_Parent.gameObject);
        }
    }
}
