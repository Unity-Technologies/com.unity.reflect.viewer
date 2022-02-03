using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    public class WalkTargetAnimation : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> m_SpritesList;

        [SerializeField, Range(0, 1)]
        float m_Time = 0;
        [SerializeField]
        float m_Speed = 1;
        [SerializeField]
        float m_DistanceThreshold = 150;

        bool m_IsAnimationStart = false;

        void Awake()
        {
            m_Time = 0;
        }

        void Update()
        {
            EnableAsset((int)(m_Time * m_SpritesList.Count) + 1);

            if (m_IsAnimationStart)
            {
                m_Time += Time.deltaTime * m_Speed;
            }
        }

        public void StartAnimation(bool isEnable)
        {
            m_Time = 1f / m_SpritesList.Count;
            m_IsAnimationStart = isEnable;

            if (!isEnable)
            {
                m_Time = 0;
                EnableAsset(0);
            }
        }

        public void DistanceAnimation(Vector2 origin, Vector2 current)
        {
            float dist = Vector2.Distance(origin, current);
            float offset = 1f / m_SpritesList.Count;
            m_Time = offset + dist / m_DistanceThreshold;
        }

        void EnableAsset(int position)
        {
            for (int i = 0; i < m_SpritesList.Count; i++)
            {
                m_SpritesList[i].SetActive(i < position);
            }
        }
    }
}
