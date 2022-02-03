#if !NET_DOTS
using System;
using System.Globalization;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<Guid>,
        IJsonAdapter<DateTime>,
        IJsonAdapter<TimeSpan>,
        IJsonAdapter<Version>
    {
        void IJsonAdapter<Guid>.Serialize(JsonSerializationContext<Guid> context, Guid value)
            => context.Writer.WriteValue(value.ToString("N", CultureInfo.InvariantCulture));

        Guid IJsonAdapter<Guid>.Deserialize(JsonDeserializationContext<Guid> context)
            => Guid.TryParseExact(context.SerializedValue.ToString(), "N", out var value) ? value : default;

        void IJsonAdapter<DateTime>.Serialize(JsonSerializationContext<DateTime> context, DateTime value)
            => context.Writer.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));

        DateTime IJsonAdapter<DateTime>.Deserialize(JsonDeserializationContext<DateTime> context)
            => DateTime.TryParseExact(context.SerializedValue.ToString(), "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : default;

        void IJsonAdapter<TimeSpan>.Serialize(JsonSerializationContext<TimeSpan> context, TimeSpan value)
            => context.Writer.WriteValue(value.ToString("c", CultureInfo.InvariantCulture));

        TimeSpan IJsonAdapter<TimeSpan>.Deserialize(JsonDeserializationContext<TimeSpan> context)
            => TimeSpan.TryParseExact(context.SerializedValue.ToString(), "c", CultureInfo.InvariantCulture, out var value) ? value : default;
        
        void IJsonAdapter<Version>.Serialize(JsonSerializationContext<Version> context, Version value)
            => context.Writer.WriteValue(value.ToString());

        Version IJsonAdapter<Version>.Deserialize(JsonDeserializationContext<Version> context)
            => new Version(context.SerializedValue.ToString());
    }
}
#endif