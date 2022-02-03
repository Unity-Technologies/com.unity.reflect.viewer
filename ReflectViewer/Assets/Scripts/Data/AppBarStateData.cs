using System;
using Unity.Properties;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    public enum ButtonType
    {
        ProjectList=0,
        Refresh,
        Help,
        Sync,
        Settings,
        Follow,
        LinkSharing,
        Audio,
        Profile,
        PrivateMode,
        LogOff
    }

    public class ButtonVisibility : IButtonVisibility
    {
        public int type { get; set; }
        public bool visible { get; set; }
    }

    public class ButtonInteractable : IButtonInteractable
    {
        public int type { get; set; }
        public bool interactable { get; set; }
    }

    [Serializable, GeneratePropertyBag]
    public struct AppBarStateData : IEquatable<AppBarStateData>, IAppBarDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IButtonVisibility buttonVisibility { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IButtonInteractable buttonInteractable { get; set; }

        public bool Equals(AppBarStateData other)
        {
            return buttonVisibility == other.buttonVisibility &&
                buttonInteractable == other.buttonInteractable;
        }

        public override bool Equals(object obj)
        {
            return obj is AppBarStateData other && Equals(other);
        }

        public static bool operator ==(AppBarStateData a, AppBarStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AppBarStateData a, AppBarStateData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = buttonVisibility.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonInteractable.GetHashCode();

                return hashCode;
            }
        }
    }
}
