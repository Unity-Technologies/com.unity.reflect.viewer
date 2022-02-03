using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.TouchFramework
{

    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public readonly TransformData identity => new TransformData
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,
            scale = Vector3.one
        };

        public TransformData(Transform transform, bool worldspace = true)
        {
            if (worldspace)
            {
                position = transform.position;
                rotation = transform.rotation;
                scale = transform.lossyScale;
            }
            else
            {
                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;
            }
        }

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override string ToString()
        {
            return $"({position}, {rotation}, {scale})";
        }
    }

    public class TransformPropertyValue : MonoBehaviour, IPropertyValue<TransformData>
    {
        [SerializeField]
        Vector3NumericInputFieldPropertyValue m_PositionInput;
        [SerializeField]
        Vector3NumericInputFieldPropertyValue m_RotationInput;
        [SerializeField]
        Vector3NumericInputFieldPropertyValue m_ScaleInput;

        TransformData m_Value;

        public Type type => typeof(TransformData);

        public object objectValue
        {
            get => value;
            set
            {
                m_Value = (TransformData)value;
                UpdateChildProperties();
            }
        }

        public TransformData value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                UpdateChildProperties();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        void UpdateChildProperties()
        {
            m_PositionInput.objectValue = m_Value.position;
            m_RotationInput.objectValue = m_Value.rotation.eulerAngles;
            m_ScaleInput.objectValue = m_Value.scale;
        }

        void Start()
        {
            // bubble up child property updates
            m_PositionInput.AddListener(HandlePositionUpdate);
            m_RotationInput.AddListener(HandleRotationInput);
            m_ScaleInput.AddListener(HandleScaleInput);
        }

        void OnDestroy()
        {
            m_PositionInput.RemoveListener(HandlePositionUpdate);
            m_RotationInput.RemoveListener(HandleRotationInput);
            m_ScaleInput.RemoveListener(HandleScaleInput);
        }

        void HandlePositionUpdate()
        {
            m_Value.position = m_PositionInput.value;
            m_OnValueChanged?.Invoke(m_Value);
        }

        void HandleRotationInput()
        {
            m_Value.rotation = Quaternion.Euler(m_RotationInput.value);
            m_OnValueChanged?.Invoke(m_Value);
        }

        void HandleScaleInput()
        {
            m_Value.scale = m_ScaleInput.value;
            m_OnValueChanged?.Invoke(m_Value);
        }

        #region events


        UnityEvent<TransformData> m_OnValueChanged = new UnityEvent<TransformData>();
        Dictionary<Action, UnityAction<TransformData>> m_Handlers = new Dictionary<Action, UnityAction<TransformData>>();

        /// <summary>
        /// Trigger event when value changes
        /// </summary>
        /// <param name="eventFunc"></param>
        public void AddListener(Action eventFunc)
        {
            m_Handlers[eventFunc] = (newValue) =>
            {
                eventFunc();
            };
            m_OnValueChanged.AddListener(m_Handlers[eventFunc]);
        }

        /// <summary>
        /// Remove value change event
        /// </summary>
        /// <param name="eventFunc"></param>
        public void RemoveListener(Action eventFunc)
        {
            if (!m_Handlers.ContainsKey(eventFunc))
                return;
            m_OnValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }

        #endregion

    }
}
