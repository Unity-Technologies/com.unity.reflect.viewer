using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class FollowUserAction: ActionBase
    {
        public struct FollowUserData
        {
            public string matchmakerId;
            public GameObject visualRepresentationGameObject;
        }

        public object Data { get; }

        FollowUserAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            var prefPropertyName = nameof(IFollowUserDataProvider.userId);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<string>(ref boxed, prefPropertyName);
                var shouldFollowThisUser = !string.IsNullOrEmpty(((FollowUserData)viewerActionData).matchmakerId) && oldValue != ((FollowUserData)viewerActionData).matchmakerId;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, shouldFollowThisUser ? ((FollowUserData)viewerActionData).matchmakerId : "", oldValue);

                prefPropertyName = nameof(IFollowUserDataProvider.userObject);
                if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
                {
                    var oldValue2 = PropertyContainer.GetValue<GameObject>(ref boxed, prefPropertyName);

                    hasChanged |= SetPropertyValue(ref stateData, prefPropertyName,
                        shouldFollowThisUser ? ((FollowUserData)viewerActionData).visualRepresentationGameObject : null, oldValue2);
                }

                prefPropertyName = nameof(IFollowUserDataProvider.isFollowing);
                if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
                {
                    hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IFollowUserDataProvider.isFollowing), shouldFollowThisUser);
                }
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new FollowUserAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == FollowUserContext.current;
        }
    }
}
