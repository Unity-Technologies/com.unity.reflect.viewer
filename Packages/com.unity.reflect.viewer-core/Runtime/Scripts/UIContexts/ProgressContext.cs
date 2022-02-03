using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IProgressDataProvider
    {
        public SetProgressStateAction.ProgressState progressState { get; set; }
        public int totalCount { get; set; }
        public int currentProgress { get; set; }
    }

    public class ProgressContext : ContextBase<ProgressContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IProgressDataProvider)};
    }
}
