using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public enum SkyboxType
    {
        Light = 0,
        Dark = 1,
        Custom = 2
    }

    public enum WeatherType
    {
        HeavyRain = 0,
        Sunny = 1,
    }

    public interface ISceneOptionData<T>
    {
        public  bool enableLightData { get; set; }
        public bool enableTexture { get; set; }
        public bool enableStatsInfo { get; set; }
        public bool filterHlods { get; set; }
        public T skyboxData { get; set; }
        public float temperature { get; set; }
        public bool enableClimateSimulation { get; set; }
        public bool enableDebugOption { get; set; }
        public WeatherType weatherType { get; set; }
        public Actions.SetOrbitTypeAction.OrbitType touchOrbitType { get; set; }
    }

    public class SceneOptionContext : ContextBase<SceneOptionContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
