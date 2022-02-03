using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core
{
    public enum StatusMessageType
    {
        Debug,
        Info,
        Instruction,
        Warning
    }

    public struct StatusMessageData
    {
        public StatusMessageType type;
        public string text;
    }

    public interface IStatusMessageData
    {
        public StatusMessageData statusMessageData{ get; set; }
        public bool isClearAll{ get; set; }
        public bool isInstructionMode{ get; set; }
    }

    public class MessageManagerContext : ContextBase<MessageManagerContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> { typeof(IStatusMessageData) };
    }
}
