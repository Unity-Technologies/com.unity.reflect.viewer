using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IRoomConnectionDataProvider<T>
    {
        string userToMute { get; set; }
        public T localUser { get; set; }
        public List<T> users { get; set; }
    }

    public interface IVivoxDataProvider<T>
    {
        public T vivoxManager { get; set; }
    }

    public class RoomConnectionContext : ContextBase<RoomConnectionContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IRoomConnectionDataProvider<>)};

        public void ForceOnStateChanged()
        {
            OnStateChanged();
        }
    }
}
