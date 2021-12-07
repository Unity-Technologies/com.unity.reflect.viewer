using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface ILoginSettingDataProvider
    {
        public object environmentInfo { get; set; }
        public bool deleteCloudEnvironmentSetting { get; set; }
    }

    public class LoginSettingContext<T> : ContextBase<LoginSettingContext<T>>
    {
        public override List<Type> implementsInterfaces => new List<Type> { typeof(ILoginSettingDataProvider) };
    }
}
