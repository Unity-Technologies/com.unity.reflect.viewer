using System.Numerics;
using System.Text;
using Unity.Properties;

namespace Unity.Serialization.Json.PerformanceTests
{
#pragma warning disable 649
    [GeneratePropertyBag]
    struct MockEntityBatch
    {
        public MockEntity[] Entities;
    }

    [GeneratePropertyBag]
    struct MockEntity
    {
        public MockTranslation Translation;
        public MockRotation Rotation;
        public MockScale Scale;
    }

    [GeneratePropertyBag]
    struct MockTranslation
    {
        public Vector3 Value;
    }

    [GeneratePropertyBag]
    struct MockRotation
    {
        public Quaternion Value;
    }

    [GeneratePropertyBag]
    struct MockScale
    {
        public Vector3 Value;
    }
#pragma warning restore 649

    static class JsonTestData
    {
        const string k_MockEntityJson = @"{
    ""Translation"": {
        ""Value"": {
            ""x"": 0,
            ""y"": 0,
            ""z"": 0
        }
    },
    ""Rotation"": {
        ""Value"": {
            ""x"": 0,
            ""y"": 0,
            ""z"": 0,
            ""w"": 1
        }
    },
    ""Scale"": {
        ""Value"": {
            ""x"": 1,
            ""y"": 1,
            ""z"": 1
        }
    }
}";

        public static string GetMockEntities(int count)
        {
            var builder = new StringBuilder();
            builder.Append(@"{""Entities"": [");

            for (var i = 0; i < count; i++)
            {
                builder.Append(k_MockEntityJson);

                if (i != count - 1)
                {
                    builder.Append(",\n");
                }
            }

            builder.Append("]}");
            return builder.ToString();
        }
    }
}