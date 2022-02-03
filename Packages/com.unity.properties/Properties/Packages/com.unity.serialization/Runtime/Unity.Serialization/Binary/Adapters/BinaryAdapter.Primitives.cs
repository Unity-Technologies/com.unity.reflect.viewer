#if !NET_DOTS
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        IBinaryAdapter<sbyte>, 
        IBinaryAdapter<short>, 
        IBinaryAdapter<int>,   
        IBinaryAdapter<long>,  
        IBinaryAdapter<byte>,  
        IBinaryAdapter<ushort>,
        IBinaryAdapter<uint>,  
        IBinaryAdapter<ulong>, 
        IBinaryAdapter<float>, 
        IBinaryAdapter<double>,
        IBinaryAdapter<bool>,  
        IBinaryAdapter<char>,  
        IBinaryAdapter<string>
    {
        void IBinaryAdapter<sbyte>.Serialize(BinarySerializationContext<sbyte> context, sbyte value)
            => context.Writer->Add(value);

        void IBinaryAdapter<short>.Serialize(BinarySerializationContext<short> context, short value)
            => context.Writer->Add(value);

        void IBinaryAdapter<int>.Serialize(BinarySerializationContext<int> context, int value)
            => context.Writer->Add(value);

        void IBinaryAdapter<long>.Serialize(BinarySerializationContext<long> context, long value)
            => context.Writer->Add(value);

        void IBinaryAdapter<byte>.Serialize(BinarySerializationContext<byte> context, byte value)
            => context.Writer->Add(value);

        void IBinaryAdapter<ushort>.Serialize(BinarySerializationContext<ushort> context, ushort value)
            => context.Writer->Add(value);

        void IBinaryAdapter<uint>.Serialize(BinarySerializationContext<uint> context, uint value)
            => context.Writer->Add(value);

        void IBinaryAdapter<ulong>.Serialize(BinarySerializationContext<ulong> context, ulong value)
            => context.Writer->Add(value);

        void IBinaryAdapter<float>.Serialize(BinarySerializationContext<float> context, float value)
            => context.Writer->Add(value);

        void IBinaryAdapter<double>.Serialize(BinarySerializationContext<double> context, double value)
            => context.Writer->Add(value);

        void IBinaryAdapter<bool>.Serialize(BinarySerializationContext<bool> context, bool value)
            => context.Writer->Add((byte)(value ? 1 : 0));

        void IBinaryAdapter<char>.Serialize(BinarySerializationContext<char> context, char value)
            => context.Writer->Add(value);
        
        void IBinaryAdapter<string>.Serialize(BinarySerializationContext<string> context, string value)
            => context.Writer->Add(value);

        sbyte IBinaryAdapter<sbyte>.Deserialize(BinaryDeserializationContext<sbyte> context)
            => context.Reader->ReadNext<sbyte>();

        short IBinaryAdapter<short>.Deserialize(BinaryDeserializationContext<short> context)
            => context.Reader->ReadNext<short>();

        int IBinaryAdapter<int>.Deserialize(BinaryDeserializationContext<int> context)
            => context.Reader->ReadNext<int>();

        long IBinaryAdapter<long>.Deserialize(BinaryDeserializationContext<long> context)
            => context.Reader->ReadNext<long>();

        byte IBinaryAdapter<byte>.Deserialize(BinaryDeserializationContext<byte> context)
            => context.Reader->ReadNext<byte>();

        ushort IBinaryAdapter<ushort>.Deserialize(BinaryDeserializationContext<ushort> context)
            => context.Reader->ReadNext<ushort>();

        uint IBinaryAdapter<uint>.Deserialize(BinaryDeserializationContext<uint> context)
            => context.Reader->ReadNext<uint>();

        ulong IBinaryAdapter<ulong>.Deserialize(BinaryDeserializationContext<ulong> context)
            => context.Reader->ReadNext<ulong>();

        float IBinaryAdapter<float>.Deserialize(BinaryDeserializationContext<float> context)
            => context.Reader->ReadNext<float>();

        double IBinaryAdapter<double>.Deserialize(BinaryDeserializationContext<double> context)
            => context.Reader->ReadNext<double>();

        bool IBinaryAdapter<bool>.Deserialize(BinaryDeserializationContext<bool> context)
            => context.Reader->ReadNext<byte>() == 1;

        char IBinaryAdapter<char>.Deserialize(BinaryDeserializationContext<char> context)
            => context.Reader->ReadNext<char>();

        string IBinaryAdapter<string>.Deserialize(BinaryDeserializationContext<string> context)
        {
            context.Reader->ReadNext(out string value);
            return value;
        }
    }
}
#endif