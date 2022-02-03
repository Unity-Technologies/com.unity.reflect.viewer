#if !NET_DOTS
using System;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<sbyte>,
        IJsonAdapter<short>,
        IJsonAdapter<int>,
        IJsonAdapter<long>,
        IJsonAdapter<byte>,
        IJsonAdapter<ushort>,
        IJsonAdapter<uint>,
        IJsonAdapter<ulong>,
        IJsonAdapter<float>,
        IJsonAdapter<double>,
        IJsonAdapter<bool>,
        IJsonAdapter<char>,
        IJsonAdapter<string>
    {
        void IJsonAdapter<sbyte>.Serialize(JsonSerializationContext<sbyte> context, sbyte value) => context.Writer.WriteValue(value);
        void IJsonAdapter<short>.Serialize(JsonSerializationContext<short> context, short value) => context.Writer.WriteValue(value);
        void IJsonAdapter<int>.Serialize(JsonSerializationContext<int> context, int value) => context.Writer.WriteValue(value);
        void IJsonAdapter<long>.Serialize(JsonSerializationContext<long> context, long value) => context.Writer.WriteValue(value);
        void IJsonAdapter<byte>.Serialize(JsonSerializationContext<byte> context, byte value) => context.Writer.WriteValue(value);
        void IJsonAdapter<ushort>.Serialize(JsonSerializationContext<ushort> context, ushort value) => context.Writer.WriteValue(value);
        void IJsonAdapter<uint>.Serialize(JsonSerializationContext<uint> context, uint value) => context.Writer.WriteValue(value);
        void IJsonAdapter<ulong>.Serialize(JsonSerializationContext<ulong> context, ulong value) => context.Writer.WriteValue(value);
        void IJsonAdapter<float>.Serialize(JsonSerializationContext<float> context, float value) => context.Writer.WriteValue(value);
        void IJsonAdapter<double>.Serialize(JsonSerializationContext<double> context, double value) => context.Writer.WriteValue(value);
        void IJsonAdapter<bool>.Serialize(JsonSerializationContext<bool> context, bool value) => context.Writer.WriteValueLiteral(value ? "true" : "false");
        void IJsonAdapter<char>.Serialize(JsonSerializationContext<char> context, char value) => context.Writer.WriteValue(value);
        void IJsonAdapter<string>.Serialize(JsonSerializationContext<string> context, string value) => context.Writer.WriteValue(value);
        
        sbyte IJsonAdapter<sbyte>.Deserialize(JsonDeserializationContext<sbyte> context) 
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        short IJsonAdapter<short>.Deserialize(JsonDeserializationContext<short> context) 
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        int IJsonAdapter<int>.Deserialize(JsonDeserializationContext<int> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        long IJsonAdapter<long>.Deserialize(JsonDeserializationContext<long> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        byte IJsonAdapter<byte>.Deserialize(JsonDeserializationContext<byte> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        ushort IJsonAdapter<ushort>.Deserialize(JsonDeserializationContext<ushort> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        uint IJsonAdapter<uint>.Deserialize(JsonDeserializationContext<uint> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        ulong IJsonAdapter<ulong>.Deserialize(JsonDeserializationContext<ulong> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        float IJsonAdapter<float>.Deserialize(JsonDeserializationContext<float> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        double IJsonAdapter<double>.Deserialize(JsonDeserializationContext<double> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        bool IJsonAdapter<bool>.Deserialize(JsonDeserializationContext<bool> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        char IJsonAdapter<char>.Deserialize(JsonDeserializationContext<char> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        string IJsonAdapter<string>.Deserialize(JsonDeserializationContext<string> context)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
    }
}
#endif