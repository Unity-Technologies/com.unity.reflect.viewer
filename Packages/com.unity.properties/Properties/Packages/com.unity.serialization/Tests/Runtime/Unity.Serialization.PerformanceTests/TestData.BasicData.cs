using System;
using Unity.Properties;

namespace Unity.Serialization.PerformanceTests
{
    [GeneratePropertyBag, Serializable]
    struct BasicDataBatch
    {
        public BasicData[] batch;
    }
    
    [GeneratePropertyBag, Serializable]
    public struct BasicData
    {
        public string Value;
    }
}