using System;
using System.Collections;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{

    /// <summary>
    ///     A generic follow object camera
    /// </summary>
    [RequireComponent(typeof(FreeFlyCamera))]
    public class FollowObjectCamera : MonoBehaviour
    {
        GameObject m_ObjectToFollow = null;
        FreeFlyCamera m_Camera;

        bool m_IsFollowing;

        void Awake()
        {
            m_Camera = GetComponent<FreeFlyCamera>();
            UIStateManager.stateChanged += OnUIStateChanged;
        }

        void OnUIStateChanged(UIStateData stateData)
        {
            m_ObjectToFollow = stateData.toolState.followUserTool.userObject;
            if(m_IsFollowing)
            {
                m_IsFollowing = m_ObjectToFollow != null;
            }
            else if (m_ObjectToFollow != null)
            {
                m_IsFollowing = true;
                StartCoroutine(FollowObjectUpdate());
            }
        }

        IEnumerator FollowObjectUpdate()
        {
            while (m_IsFollowing)
            {
                if (IsObjectValid(m_ObjectToFollow))
                {
                    m_Camera.TransformTo(m_ObjectToFollow.transform);
                    yield return null;
                }
                else
                {
                    m_IsFollowing = false;
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.FollowUser, null));
                }
            }
        }

        bool IsObjectValid(GameObject obj)
        {
            return m_ObjectToFollow != null;
        }
    }
}
