using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TMPro;

namespace Unity.TouchFramework
{
    public class DialControl : MonoBehaviour
    {
        [Serializable]
        public class SelectedValueChangedEvent : UnityEvent<float> {}

        SelectedValueChangedEvent m_OnSelectedValueChanged = new SelectedValueChangedEvent();
        public SelectedValueChangedEvent onSelectedValueChanged => m_OnSelectedValueChanged;

        enum MarkedEntryType
        {
            Increments,
            Manual
        }

        public enum NumberType
        {
            Int,
            Float
        }

        static DefaultScaler s_DefaultScaler = new DefaultScaler();
        IScaler m_Scaler = s_DefaultScaler;

        public IScaler scaler
        {
            set => m_Scaler = value;
        }

        static DefaultConverter s_DefaultConverter = new DefaultConverter();
        ILabelConverter m_LabelConverter = s_DefaultConverter;

        public ILabelConverter labelConverter
        {
            set => m_LabelConverter = value;
        }

#pragma warning disable CS0649
        [SerializeField]
        Graphic m_DialGraphic;
        [SerializeField]
        MarkedEntryType m_MarkedEntryType = MarkedEntryType.Manual;
        [SerializeField]
        NumberType m_DisplayNumberType = NumberType.Int;
        [SerializeField]
        float m_MarkedEntryFontSize = 12;
        [SerializeField]
        Orientation m_Orientation;
        [SerializeField]
        int m_MarkedEntrySegmentCount = 5;
        [SerializeField]
        float m_MinimumValue;
        [SerializeField]
        float m_MaximumValue = 10f;
        [SerializeField]
        List<float> m_MarkedEntries = new List<float>();
        [SerializeField]
        GameObject m_MarkedEntryPrefab;
        [SerializeField]
        TextMeshProUGUI m_SelectedEntryLabel;
        [SerializeField]
        float m_AngularRange = 90f;
        [SerializeField]
        float m_DeadZoneRadius = 20;
        [SerializeField]
        Image m_ScaleGraphic;
        [SerializeField]
        float m_MarkedEntryRadius = 165f;
        [SerializeField]
        bool m_TapToJumpToEntry;
        [SerializeField, Tooltip("Approximate number of graduations to be distributed along the dial.")]
        float m_ScaleDensityHint;
        [SerializeField, Tooltip("Allow selection of entries only, excluding intermediate values.")]
        bool m_RestrictSelectionToEntries;
        [SerializeField, Tooltip("Angle in degrees within which we may snap to an entry.")]
        float m_AngularSnapThreshold; // Snapping threshold is expressed as an angle since it is tied to user motion.
        [SerializeField, Tooltip("Snap to entry on pointer up.")]
        bool m_SnapOnPointerUp;
        [SerializeField, Tooltip("Snap to entry while dragging according to angular velocity.")]
        bool m_SnapOnPointerDrag;
        [SerializeField, Tooltip("Energy threshold (affected by angular velocity) determining whether or not we snap to an entry while dragging.")]
        float m_SnapKineticEnergyThreshold; // Degrees per second.
        [SerializeField, Tooltip("Angular motion threshold beyond which snap mode is exited regardless of velocity.")]
        float m_ExitSnapAngularThreshold;
        [SerializeField]
        Vector2 m_ScaleRadiusMinMax;
        [SerializeField]
        Color m_ScaleColor;
        [SerializeField]
        float m_ScaleAntialiasing;
#pragma warning restore CS0649

        // WARNING: this error margin should not be too small, or the heuristic could never converge
        const float k_relativeAngleErrorMargin = 0.15f;

        // Angular range is exposed since it may be required by custom scalers.
        public float angularRange => m_AngularRange;

        RectTransform m_Rect;
        Vector2 m_CenterPoint;

        bool m_Snapped;
        float m_DialAngle;
        float m_LastDialAngle;
        float m_LastSnapDialAngle;
        float m_LastDragDistance;
        float m_SelectedValue;
        float m_LastSelectedValue;
        bool m_EntriesNeedUpdate;
        float m_CachedAngleForLabelAlphaUpdate;
        CircleRaycastFilter m_Filter;
        CircularGraduation m_CircularGraduation = new CircularGraduation();

        struct EntryObject
        {
            public GameObject gameObject;
            public TextMeshProUGUI textField;
        }

        // Entries management.
        Stack<GameObject> m_EntryObjectPool = new Stack<GameObject>();
        List<EntryObject> m_ActiveEntryObjects = new List<EntryObject>();
        List<float> m_IncrementEntries = new List<float>();
        List<float> m_CachedCurrentEntries; // Optimization, so that snapping code does not trigger current entries update.

        // Snap-On-Drag behavior.
        float m_AngularVelocity;
        float m_LastMotionTime; // Used to compute angular velocity.
        float m_SnapKineticEnergy; // Energy level letting us determine whether we should snap or not.
        float m_LastSnapDirection;

        /// <summary>
        /// The currently selected value
        /// </summary>
        public float selectedValue
        {
            get => m_SelectedValue;
            set
            {
                m_SelectedValue = Mathf.Clamp(value, m_MinimumValue, m_MaximumValue);
                m_SelectedEntryLabel.text = m_DisplayNumberType == NumberType.Int ? m_LabelConverter.ConvertSelectedValLabel(m_SelectedValue, true) : m_LabelConverter.ConvertSelectedValLabel(m_SelectedValue, false);
                UpdateRectRotation();
            }
        }

        /// <summary>
        /// The minimum value of the dial. Setting is only valid when the <see cref="MarkedEntryType"/> is Incremental.
        /// </summary>
        public float minimumValue
        {
            get => m_MinimumValue;
            set
            {
                if (m_MarkedEntryType == MarkedEntryType.Manual)
                {
                    Debug.LogWarning("DialControl uses Manual mode, minimum value is ignored.");
                    return;
                }

                // Note: no early return, it is client code's responsibility to update range when appropriate
                // Once invoked, m_MinimumValue must reflect passed value
                m_EntriesNeedUpdate |= m_MinimumValue != value; // Updating entries is not free, only when needed.
                m_MinimumValue = value;

                // Bounds have changed, make sure the selected value is within those.
                selectedValue = m_SelectedValue;
            }
        }

        /// <summary>
        /// The maximum value of the dial. Setting is only valid when the <see cref="MarkedEntryType"/> is Incremental.
        /// </summary>
        public float maximumValue
        {
            get => m_MaximumValue;
            set
            {
                if (m_MarkedEntryType == MarkedEntryType.Manual)
                {
                    Debug.LogWarning("DialControl uses Manual mode, maximum value is ignored.");
                    return;
                }

                // Note: no early return, it is client code's responsibility to update range when appropriate
                // Once invoked, m_MaximumValue must reflect passed value
                m_EntriesNeedUpdate |= m_MaximumValue != value; // Updating entries is not free, only when needed.
                m_MaximumValue = value;

                // Bounds have changed, make sure the selected value is within those.
                selectedValue = m_SelectedValue;
            }
        }

        /// <summary>
        /// Set the marked entries
        /// </summary>
        /// <remarks>
        /// This will set the marked entry values regardless of the <see cref="MarkedEntryType"/> but will only be
        /// represented on the dial when the <see cref="MarkedEntryType"/> is set to Manual.
        /// </remarks>
        /// <param name="entries">A sorted array of entry values.</param>
        public void SetMarkedEntries(float[] entries)
        {
            if (m_MarkedEntryType != MarkedEntryType.Manual)
                throw new InvalidOperationException("Entries can only be set in manual mode.");

            m_MarkedEntries.Clear();
            m_MarkedEntries.AddRange(entries);
            m_EntriesNeedUpdate = true;
        }

        void UpdateRectRotation()
        {
            if (m_Rect != null)
            {
                var angle = m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, m_SelectedValue);
                m_Rect.localEulerAngles = (m_Orientation == Orientation.Left ? Vector3.back : Vector3.forward) * angle;
            }
        }

        void Awake()
        {
            m_Rect = m_DialGraphic.rectTransform;
            m_Filter = m_DialGraphic.GetComponent<CircleRaycastFilter>();
            Assert.IsNotNull(m_Filter, "Missing CircleRaycastFilter.");

            var selectTransform = transform.Find("Selector Background") as RectTransform;
            Assert.IsNotNull(selectTransform, "Failed to retrieve Selector Background transform.");
            selectTransform.localPosition = (m_Orientation == Orientation.Left ? Vector3.left : Vector3.right) * m_MarkedEntryRadius;
            var selectorLayout = selectTransform.GetComponent<HorizontalLayoutGroup>();
            selectorLayout.childAlignment = m_Orientation == Orientation.Left ? TextAnchor.UpperLeft : TextAnchor.UpperRight;

            var arrowTransform = transform.Find("Arrow") as RectTransform;
            Assert.IsNotNull(arrowTransform, "Failed to retrieve Arrow transform.");
            var width = (transform as RectTransform).rect.width;
            arrowTransform.localPosition = (m_Orientation == Orientation.Left ? Vector3.left : Vector3.right) * width * 0.5f;
            arrowTransform.localEulerAngles = new Vector3(0, 0, m_Orientation == Orientation.Left ? 0 : 180);

            // Setup scale.
            var rotation = m_ScaleGraphic.transform.rotation.eulerAngles;
            rotation.z = m_AngularRange + (m_Orientation == Orientation.Left ? -90 : 90);
            m_ScaleGraphic.transform.rotation = Quaternion.Euler(rotation);

            // List of marked entries is not expected to change, sort it once and for all.
            m_MarkedEntries.Sort();
            m_EntriesNeedUpdate = true;

            UpdateRectRotation();
        }

        void UpdateGraduations(List<float> entries)
        {
            var parms = new CircularGraduation.Parameters
            {
                radiusMinMax = m_ScaleRadiusMinMax,
                antialiasing = m_ScaleAntialiasing,
                color = m_ScaleColor,
                angularRange = m_AngularRange,
                orientation = m_Orientation,
                scaleDensityHint = m_ScaleDensityHint,
                entryLineWidth = 4,
                lineWidth = 2
            };
            var range = new Vector2(m_MinimumValue, m_MaximumValue);
            m_ScaleGraphic.material = m_CircularGraduation.Update(parms, m_Scaler, range, entries);
        }

        // TODO: should those event be cleared?
        void Start()
        {
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(m_DialGraphic.gameObject, OnEndDrag, EventTriggerType.EndDrag);
        }

        void OnEnable()
        {
            selectedValue = m_SelectedValue; // Force UI sync.
        }

        void OnDestroy()
        {
            m_ActiveEntryObjects.Clear();
            m_EntryObjectPool.Clear();
            m_CircularGraduation.Dispose();
        }

        void Update()
        {
            var labelsAlphaNeedUpdate = false;
            if (m_EntriesNeedUpdate || m_Scaler.isDirty)
            {
                m_EntriesNeedUpdate = false;
                UpdateEntries();
                labelsAlphaNeedUpdate = true;
                m_Scaler.MarkClean();
            }

            var angle = m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, m_SelectedValue);
            labelsAlphaNeedUpdate |= Mathf.Abs(Mathf.DeltaAngle(angle, m_CachedAngleForLabelAlphaUpdate)) > 10e-3;
            if (labelsAlphaNeedUpdate)
            {
                m_CachedAngleForLabelAlphaUpdate = angle;
                UpdateLabelsAlpha();
            }

            m_Filter.worldRadius = m_Rect.rect.width * 0.5f * m_Rect.lossyScale.x;
            m_Filter.worldCenter = m_Rect.position;
        }

        // Dissipate kinetic energy (snap-on-drag behavior).
        void FixedUpdate()
        {
            if (!m_Snapped)
                m_SnapKineticEnergy *= 0.9f;
        }

        #region Entries

        // Returns a collection of values depending on current mode.
        // Encapsulates mode-dependent code.
        List<float> GetCurrentEntries()
        {
            if (m_MarkedEntryType == MarkedEntryType.Manual)
            {
                if (m_MarkedEntries.Count < 1)
                    throw new IndexOutOfRangeException("DialControl: entries list is empty.");
                return m_MarkedEntries;
            }

            m_IncrementEntries.Clear();
            var angleIncrement = (2 * m_AngularRange) / (m_MarkedEntrySegmentCount + 1);
            m_IncrementEntries.Add(m_MinimumValue);
            for (var i = 1; i <= m_MarkedEntrySegmentCount; i++)
            {
                var angle = m_AngularRange - (angleIncrement * i);
                m_IncrementEntries.Add(m_Scaler.AngleToValue(minimumValue, maximumValue , angularRange, angle));
            }
            m_IncrementEntries.Add(m_MaximumValue);

            for (var i = 1; i < m_IncrementEntries.Count - 1; i++)
            {
                var gap = Mathf.Abs(m_IncrementEntries[i + 1] - m_IncrementEntries[i - 1]);
                m_IncrementEntries[i] = RoundNumberHeuristic(m_IncrementEntries[i], gap);
            }
            return m_IncrementEntries;
        }

        float RoundNumberHeuristic(float value, float gap)
        {
            var logScale = Mathf.Log(gap, 10);
            var exponent = Mathf.Floor(logScale);
            var multiplier = (logScale - exponent) > Mathf.Log(5, 10) ? 5 : 1;
            var roundingReference = Mathf.Pow(10, exponent) * multiplier;
            var originalAngle = m_Scaler.ValueToAngle(minimumValue, maximumValue, m_AngularRange, value);
            var errorMargin = k_relativeAngleErrorMargin * (2 * m_AngularRange) / (m_MarkedEntrySegmentCount + 1);
            float newAngle;
            float roundedValue;
            // This tries to round to a multiple of 10^x or a multiple of 5*(10^x)
            // It decrements x until the angle error is within the margin
            do
            {
                roundingReference /= (multiplier == 1 ? 2 : 5);
                roundedValue = RoundToReference(value, roundingReference);
                newAngle = m_Scaler.ValueToAngle(minimumValue, maximumValue, m_AngularRange, roundedValue);
            }
            while (!(Mathf.Abs(originalAngle - newAngle) < errorMargin));
            return roundedValue;
        }

        static float RoundToReference(float value, float reference)
        {
            return Mathf.Round(value / reference) * reference;
        }

        // Returns a configured entry prefab. Fetched from pool or instantiated if pool was empty.
        GameObject AllocEntryObject(Transform parent, float value, float radius, float fontSize, bool interactable)
        {
            var entry = m_EntryObjectPool.Count > 0 ? m_EntryObjectPool.Pop() : Instantiate(m_MarkedEntryPrefab);
            entry.hideFlags = HideFlags.DontSave;

            var button = entry.GetComponentInChildren<Button>();
            var buttonRectTransform = button.GetComponent<RectTransform>();
            var image = button.GetComponent<Image>();
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            var angle = m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, value);

            entry.transform.SetParent(parent);
            entry.transform.localPosition = Vector3.zero;
            entry.transform.localScale = Vector3.one;

            var rectTransform = entry.GetComponent<RectTransform>();
            if (m_Orientation == Orientation.Left)
            {
                rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
                buttonRectTransform.anchoredPosition = Vector2.left * radius;
                text.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                rectTransform.localEulerAngles = new Vector3(0f, 0f, -angle);
                buttonRectTransform.anchoredPosition = Vector2.right * radius;
                text.alignment = TextAlignmentOptions.Right;
            }

            text.text = m_LabelConverter.ConvertTickLabels(value);
            text.fontSize = fontSize;

            button.interactable = interactable;
            image.raycastTarget = interactable;
            if (interactable)
                button.onClick.AddListener(() => selectedValue = value);

            return entry;
        }

        void ClearActiveEntries()
        {
            foreach (var entry in m_ActiveEntryObjects)
            {
                var button = entry.gameObject.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                entry.gameObject.SetActive(false);
                m_EntryObjectPool.Push(entry.gameObject);
            }

            m_ActiveEntryObjects.Clear();
        }

        void UpdateEntries()
        {
            ClearActiveEntries();
            m_CachedCurrentEntries = GetCurrentEntries();

            // In manual mode, update range.
            if (m_MarkedEntryType == MarkedEntryType.Manual)
            {
                m_MinimumValue = m_CachedCurrentEntries[0];
                m_MaximumValue = m_CachedCurrentEntries[m_CachedCurrentEntries.Count - 1];
                // Bounds may have changed, make sure the selected value is within those.
                selectedValue = m_SelectedValue;
            }

            foreach (var val in m_CachedCurrentEntries)
            {
                var go = AllocEntryObject(
                    m_Rect, val, m_MarkedEntryRadius, m_MarkedEntryFontSize, m_TapToJumpToEntry);
                go.SetActive(true);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(text);
                m_ActiveEntryObjects.Add(new EntryObject()
                {
                    gameObject = go,
                    textField = text
                });
            }

            UpdateGraduations(m_CachedCurrentEntries);
        }

        #endregion

        #region Event Handlers

        void OnBeginDrag(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;
            m_CenterPoint = RectTransformUtility.WorldToScreenPoint(pointerEventData.pressEventCamera, m_Rect.position);
            m_LastSelectedValue = selectedValue;
            m_LastDialAngle = AngleFromPointerPosition(pointerEventData.position);
            m_LastSnapDialAngle = m_LastDialAngle;
            m_LastMotionTime = Time.time;
            m_Snapped = false;
            m_AngularVelocity = 0;
            m_LastSnapDirection = 0;
            m_SnapKineticEnergy = 0;
        }

        void OnDrag(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;
            var pointerPosition = pointerEventData.position;
            var time = Time.time;
            var deltaTime = Mathf.Max(Mathf.Epsilon, time - m_LastMotionTime);
            var motionAngle = AngleFromPointerPosition(pointerEventData.position);

            // Dead zone.
            if (Vector2.Distance(pointerPosition, m_CenterPoint) < m_DeadZoneRadius)
            {
                m_LastDialAngle = motionAngle;
                m_LastMotionTime = time;
                return;
            }

            // Change in angle according to user motion.
            var deltaAngle = (motionAngle - m_LastDialAngle) * Mathf.Sign(pointerPosition.x - m_CenterPoint.x);

            // Translate a change in angle to a change in value.
            var newSelectedAngle = m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, selectedValue) + deltaAngle;
            var newSelectedValue = m_Scaler.AngleToValue(m_MinimumValue, m_MaximumValue, m_AngularRange, newSelectedAngle);

            if (m_SnapOnPointerDrag)
            {
                var instantAngularVelocity = deltaAngle / deltaTime;
                m_AngularVelocity = Mathf.Lerp(m_AngularVelocity, instantAngularVelocity, 0.1f);
                m_SnapKineticEnergy += m_AngularVelocity * m_AngularVelocity * 10e-3f; // Scaling simply to make threshold more readable.

                if (m_Snapped)
                {
                    // There are 3 ways to exit snap mode.
                    // 1) Build up enough energy
                    var accumulatedEnoughEnergyToExitSnap = m_SnapKineticEnergy > m_SnapKineticEnergyThreshold;

                    // 2) Moving back to the side of the entry we were on before snapping.
                    var currentSnapDirection = Mathf.Sign(newSelectedValue - selectedValue);
                    var movingBackwards = currentSnapDirection * m_LastSnapDirection < 0;

                    // 3) Move by an angle large enough (regardless of velocity)
                    var crossAngularMotionThreshold = Mathf.Abs(Mathf.DeltaAngle(m_LastSnapDialAngle, motionAngle)) > m_ExitSnapAngularThreshold;

                    // Energy boost in case we exit dial mode other than by building up energy.
                    if (movingBackwards || crossAngularMotionThreshold)
                        m_SnapKineticEnergy = m_SnapKineticEnergyThreshold * 1.2f;

                    m_Snapped = !accumulatedEnoughEnergyToExitSnap && !movingBackwards && !crossAngularMotionThreshold;
                }
                else
                {
                    // Try enter snap mode. First, has our kinetic energy fell below the threshold?
                    if (m_SnapKineticEnergy < m_SnapKineticEnergyThreshold)
                    {
                        // Then, enter snap mode if there is a value to snap to and we just crossed it.
                        var closestEntry = TrySnapToClosestEntry(newSelectedValue, m_AngularSnapThreshold, out var snappedToClosestEntry);
                        var crossing = IsWithinRange(closestEntry, selectedValue, newSelectedValue);
                        if (snappedToClosestEntry && crossing)
                        {
                            m_Snapped = true;
                            m_SnapKineticEnergy = 0;
                            m_LastSnapDirection = Mathf.Sign(newSelectedValue - selectedValue);
                            m_LastSnapDialAngle = motionAngle;
                            selectedValue = closestEntry;
                        }
                    }

                    if (!m_Snapped)
                        selectedValue = newSelectedValue;
                }
            }
            else selectedValue = newSelectedValue;

            if (selectedValue != m_LastSelectedValue)
                m_OnSelectedValueChanged.Invoke(selectedValue);

            m_LastDialAngle = motionAngle;
            m_LastMotionTime = time;
            m_LastSelectedValue = selectedValue;
        }

        void OnEndDrag(BaseEventData eventData)
        {
            if (m_RestrictSelectionToEntries)
                selectedValue = FindClosestEntry(selectedValue);
            else if (m_SnapOnPointerUp)
                selectedValue = TrySnapToClosestEntry(selectedValue, m_AngularSnapThreshold, out var snapped); // TODO last arg is not used.
        }

        #endregion

        #region Utils

        void UpdateLabelsAlpha()
        {
            var baseRotation = m_Rect.localEulerAngles.z;
            foreach (var entry in m_ActiveEntryObjects)
            {
                var rotation = Mathf.DeltaAngle(0, baseRotation + entry.gameObject.transform.localEulerAngles.z);
                var color = entry.textField.color;
                color.a = Utilities.Smoothstep(5, 10, Mathf.Abs(rotation)); // TODO: have font-size influence those bounds.
                entry.textField.color = color;
            }
        }

        float AngleFromPointerPosition(Vector2 position)
        {
            return Vector2.Angle(m_Orientation == Orientation.Left ?  Vector2.up : Vector2.down, position - m_CenterPoint);
        }

        float FindClosestEntry(float value)
        {
            Assert.IsTrue(m_CachedCurrentEntries.Count > 0);
            var bestCandidate = 0f;
            var minDist = float.MaxValue;
            for (var i = 0; i != m_CachedCurrentEntries.Count; ++i)
            {
                var dist = Mathf.Abs(m_CachedCurrentEntries[i] - value);
                if (dist < minDist)
                {
                    bestCandidate = m_CachedCurrentEntries[i];
                    minDist = dist;
                }
            }
            return bestCandidate;
        }

        float TrySnapToClosestEntry(float value, float angularSnapThreshold, out bool snapped)
        {
            var boundA = m_Scaler.AngleToValue(m_MinimumValue, m_MaximumValue, m_AngularRange, m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, value) - angularSnapThreshold);
            var boundB = m_Scaler.AngleToValue(m_MinimumValue, m_MaximumValue, m_AngularRange, m_Scaler.ValueToAngle(m_MinimumValue, m_MaximumValue, m_AngularRange, value) + angularSnapThreshold);
            var closestEntry = FindClosestEntry(selectedValue);

            if (IsWithinRange(closestEntry, boundA, boundB))
            {
                snapped = true;
                return closestEntry;
            }

            snapped = false;
            return value;
        }

        static bool IsWithinRange(float value, float boundA, float boundB)
        {
            var minRangeValue = Mathf.Min(boundA, boundB);
            var maxRangeValue = Mathf.Max(boundA, boundB);
            return value >= minRangeValue && value <= maxRangeValue;
        }

        #endregion
    }
}
