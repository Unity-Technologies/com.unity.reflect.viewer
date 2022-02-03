using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface ICameraOptionsDataProvider
    {
        public SetCameraProjectionTypeAction.CameraProjectionType cameraProjectionType { get; set; }
        public bool enableJoysticks { get; set; }
        public JoystickPreference joystickPreference { get; set; }
        public bool enableAutoNavigationSpeed { get; set; }
        public int navigationSpeed { get; set; }
        public ICameraViewOption cameraViewOption { get; set; }
    }

    public interface ICameraViewOption
    {
        public SetCameraViewTypeAction.CameraViewType cameraViewType { get; set; }
        public int numberOfClick { get; set; }
    }

    public class CameraOptionsContext : ContextBase<CameraOptionsContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(ICameraOptionsDataProvider), typeof(ICameraViewOption)};
    }
}
