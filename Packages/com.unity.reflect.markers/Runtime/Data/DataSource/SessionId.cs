using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public interface ISessionId
    {
        public int SessionId { get; }
    }

    public class BaseSessionId : ISessionId
    {
        public int SessionId
        {
            get
            {
                if (m_SessionId == 0)
                    m_SessionId = Guid.NewGuid().GetHashCode();
                return m_SessionId;
            }
        }

        int m_SessionId = 0;
    }
}
