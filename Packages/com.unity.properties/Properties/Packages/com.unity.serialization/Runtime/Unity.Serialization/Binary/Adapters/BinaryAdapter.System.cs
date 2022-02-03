#if !NET_DOTS
using System;
using System.Globalization;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        IBinaryAdapter<Guid>,
        IBinaryAdapter<DateTime>,
        IBinaryAdapter<TimeSpan>,
        IBinaryAdapter<Version>
    {
        void IBinaryAdapter<Guid>.Serialize(BinarySerializationContext<Guid> context, Guid value)
            => context.Writer->Add(value.ToString("N", CultureInfo.InvariantCulture));

        Guid IBinaryAdapter<Guid>.Deserialize(BinaryDeserializationContext<Guid> context)
        {
            context.Reader->ReadNext(out string str);
            return Guid.TryParseExact(str, "N", out var value) ? value : default;
        }

        void IBinaryAdapter<DateTime>.Serialize(BinarySerializationContext<DateTime> context, DateTime value)
            => context.Writer->Add(value.ToString("o", CultureInfo.InvariantCulture));

        DateTime IBinaryAdapter<DateTime>.Deserialize(BinaryDeserializationContext<DateTime> context)
        {
            context.Reader->ReadNext(out string str);
            return DateTime.TryParseExact(str, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : default;
        }
        
        void IBinaryAdapter<TimeSpan>.Serialize(BinarySerializationContext<TimeSpan> context, TimeSpan value)
            => context.Writer->Add(value.ToString("c", CultureInfo.InvariantCulture));

        TimeSpan IBinaryAdapter<TimeSpan>.Deserialize(BinaryDeserializationContext<TimeSpan> context)
        {
            context.Reader->ReadNext(out string str);
            return TimeSpan.TryParseExact(str, "c", CultureInfo.InvariantCulture, out var value) ? value : default;
        }

        void IBinaryAdapter<Version>.Serialize(BinarySerializationContext<Version> context, Version value)
            => context.Writer->Add(value.ToString());

        Version IBinaryAdapter<Version>.Deserialize(BinaryDeserializationContext<Version> context)
        {
            context.Reader->ReadNext(out string str);
            return new Version(str);
        }
    }
}
#endif