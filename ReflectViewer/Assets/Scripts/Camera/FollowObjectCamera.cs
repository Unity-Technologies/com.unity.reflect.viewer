using System;
using System.Collections;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    ///     A generic follow object camera
    /// </summary>
    [RequireComponent(typeof(FreeFlyCamera))]
    public class FollowObjectCamera : MonoBehaviour
    {
        FreeFlyCamera m_Camera;
        IUISelector<bool> m_IsFollowingGetter;
        IUISelector<GameObject> m_UserObjectGetter;

        float m_PosElasticity;
        float m_RotElasticity;

        void Awake()
        {
            m_Camera = GetComponent<FreeFlyCamera>();
            m_IsFollowingGetter = UISelectorFactory.createSelector<bool>(FollowUserContext.current, nameof(IFollowUserDataProvider.isFollowing), OnUserObjectChanged);
            m_UserObjectGetter = UISelectorFactory.createSelector<GameObject>(FollowUserContext.current, nameof(IFollowUserDataProvider.userObject));

            m_PosElasticity = m_Camera.settings.positionElasticity;
            m_RotElasticity = m_Camera.settings.rotationElasticity;
        }

        void OnDestroy()
        {
            m_IsFollowingGetter?.Dispose();
            m_UserObjectGetter?.Dispose();
            m_Camera.settings.positionElasticity = m_PosElasticity;
            m_Camera.settings.rotationElasticity = m_RotElasticity;
        }

        void OnUserObjectChanged(bool newData)
        {
            if (newData && m_UserObjectGetter.GetValue() != null)
            {
                m_Camera.settings.positionElasticity = 0.2f;
                m_Camera.settings.rotationElasticity = 0.2f;
                StartCoroutine(FollowObjectUpdate());
            }
            else
            {
                m_Camera.settings.positionElasticity = m_PosElasticity;
                m_Camera.settings.rotationElasticity = m_RotElasticity;
            }
        }

        IEnumerator FollowObjectUpdate()
        {
            while (m_IsFollowingGetter.GetValue())
            {
                if (IsObjectValid(m_UserObjectGetter.GetValue()))
                {
                    m_Camera.TransformTo(m_UserObjectGetter.GetValue().transform);
                    yield return null;
                }
                else
                {
                    var followUserData = new FollowUserAction.FollowUserData();
                    followUserData.matchmakerId = "";
                    followUserData.visualRepresentationGameObject = null;
                    Dispatcher.Dispatch(FollowUserAction.From(followUserData));
                }
            }
        }

        bool IsObjectValid(GameObject obj)
        {
            return obj != null;
        }
    }
}
