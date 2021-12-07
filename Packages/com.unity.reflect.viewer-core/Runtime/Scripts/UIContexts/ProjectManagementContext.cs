using System;
using System.Collections.Generic;
using Unity.Reflect;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IProjectDataProvider<T>
    {
        public AccessToken accessToken { get; set; }
        public T activeProject { get; set; }
        public Sprite activeProjectThumbnail { get; set; }
        public string url { get; set; }
        public string loadSceneName { get; set; }
        public string unloadSceneName { get; set; }
    }

    public class ProjectManagementContext<T> : ContextBase<ProjectManagementContext<T>>
    {
        public override List<Type> implementsInterfaces => new List<Type> { typeof(IProjectDataProvider<T>) };    }
}
