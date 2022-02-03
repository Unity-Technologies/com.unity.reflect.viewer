using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class AnchorPoint : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Transform m_Beam;
#pragma warning restore CS0649

        public float beamHeight
        {
            get
            {
                return m_Beam.localScale.y;
            }
            set
            {
                var scale = m_Beam.localScale;
                scale.y = value;
                m_Beam.localScale = scale;
                var position = m_Beam.localPosition;
                position.y = value;
                m_Beam.localPosition = position;
            }
        }
    }
}
