#if !NET_DOTS
using System;
using System.Globalization;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Represents the method that will handle converting an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="value">The source value to be converted.</param>
    /// <typeparam name="TSource">The source type to convert from.</typeparam>
    /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
    public delegate TDestination ConvertDelegate<TSource, out TDestination>(ref TSource value);

    /// <summary>
    /// Helper class to handle type conversion during properties API calls.
    /// </summary>
    public static class TypeConversion
    {
        struct Converter<TSource, TDestination>
        {
            public static ConvertDelegate<TSource, TDestination> Convert;
        }

        static TypeConversion()
        {
            PrimitiveConverters.Register();
        }

        /// <summary>
        /// Registers a new converter from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="convert"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        public static void Register<TSource, TDestination>(ConvertDelegate<TSource, TDestination> convert)
        {
            Converter<TSource, TDestination>.Convert = convert;
        }

        /// <summary>
        /// Converts the specified value from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="value">The source value to convert.</param>
        /// <typeparam name="TSource">The source type to convert from.</typeparam>
        /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
        /// <returns>The value converted to the <typeparamref name="TDestination"/> type.</returns>
        /// <exception cref="InvalidOperationException">No converter is registered for the given types.</exception>
        public static TDestination Convert<TSource, TDestination>(ref TSource value)
        {
            if (!TryConvert<TSource, TDestination>(ref value, out var destination))
            {
                throw new InvalidOperationException($"TypeConversion no converter has been registered for SrcType=[{typeof(TSource)}] to DstType=[{typeof(TDestination)}]");
            }

            return destination;
        }

        /// <summary>
        /// Converts the specified value from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="source">The source value to convert.</param>
        /// <param name="destination">When this method returns, contains the converted destination value if the conversion succeeded; otherwise, default.</param>
        /// <typeparam name="TSource">The source type to convert from.</typeparam>
        /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
        ///<returns><see langword="true"/> if the conversion succeeded; otherwise, <see langword="false"/>.</returns>
        public static bool TryConvert<TSource, TDestination>(ref TSource source, out TDestination destination)
        {
            if (null != Converter<TSource, TDestination>.Convert)
            {
                destination = Converter<TSource, TDestination>.Convert(ref source);
                return true;
            }

            if (RuntimeTypeInfoCache<TDestination>.IsNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(TDestination));

                if (underlyingType.IsEnum)
                {
                    underlyingType = Enum.GetUnderlyingType(underlyingType);
                }
                
                destination = (TDestination) System.Convert.ChangeType(source, underlyingType);
                return true;
            }

#if !UNITY_DOTSPLAYER
            if (RuntimeTypeInfoCache<TDestination>.IsUnityObject)
            {
                if (TryConvertToUnityEngineObject(source, out destination))
                {
                    return true;
                }
            }
#endif
            if (RuntimeTypeInfoCache<TDestination>.IsEnum)
            {
                if (TryConvertToEnum(source, out destination))
                {
                    return true;
                }
            }

            // Could be boxing :(
            if (source is TDestination assignable)
            {
                destination = assignable;
                return true;
            }

            if (typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                destination = (TDestination) (object) source;
                return true;
            }

            destination = default;
            return false;
        }
        
#if !UNITY_DOTSPLAYER
        static bool TryConvertToUnityEngineObject<TSource, TDestination>(TSource source, out TDestination destination)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(typeof(TDestination)))
            {
                destination = default;
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TSource)) && null == source)
            {
                destination = default;
                return true;
            }

#if UNITY_EDITOR
            var sourceType = typeof(TSource);

            if ((sourceType.IsClass && null != source) || sourceType.IsValueType)
            {
                var str = source.ToString();

                if (UnityEditor.GlobalObjectId.TryParse(str, out var id))
                {
                    var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    destination = (TDestination) (object) obj;
                    return true;
                }

                if (str == new UnityEditor.GlobalObjectId().ToString())
                {
                    destination = (TDestination) (object) null;
                    return true;
                }
            }

#endif
            destination = default;
            return false;
        }
#endif

        static bool TryConvertToEnum<TSource, TDestination>(TSource source, out TDestination destination)
        {
            if (!typeof(TDestination).IsEnum)
            {
                destination = default;
                return false;
            }

            if (typeof(TSource) == typeof(string))
            {
                try
                {
                    destination = (TDestination) Enum.Parse(typeof(TDestination), (string) (object) source);
                }
                catch (ArgumentException)
                {
                    destination = default;
                    return false;
                }

                return true;
            }

            if (typeof(TSource).IsAssignableFrom(typeof(TDestination)))
            {
                destination = (TDestination) Enum.ToObject(typeof(TDestination), source);
                return true;
            }

            var sourceTypeCode = Type.GetTypeCode(typeof(TSource));
            var destinationTypeCode = Type.GetTypeCode(typeof(TDestination));
            
            // Enums are tricky, and we need to handle narrowing conversion manually. Might as well do all possible valid use-cases.
            switch (sourceTypeCode)
            {
                case TypeCode.UInt64:
                    var uLongValue = Convert<TSource, ulong>(ref source);
                    switch (destinationTypeCode)
                    { 
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<ulong, int>(ref uLongValue);
                            break;
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<ulong, byte>(ref uLongValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<ulong, short>(ref uLongValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<ulong, long>(ref uLongValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<ulong, sbyte>(ref uLongValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<ulong, ushort>(ref uLongValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<ulong, uint>(ref uLongValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<TSource, ulong>(ref source);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int32:
                    var intValue = Convert<TSource, int>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Int32:
                            destination = (TDestination) (object) intValue;
                            break;
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<int, byte>(ref intValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<int, short>(ref intValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<int, long>(ref intValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<int, sbyte>(ref intValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<int, ushort>(ref intValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<int, uint>(ref intValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<int, ulong>(ref intValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Byte:
                    var byteValue = Convert<TSource, byte>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) byteValue;
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<byte, short>(ref byteValue);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<byte, int>(ref byteValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<byte, long>(ref byteValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<byte, sbyte>(ref byteValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<byte, ushort>(ref byteValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<byte, uint>(ref byteValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<byte, ulong>(ref byteValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.SByte:
                    var sByteValue = Convert<TSource, sbyte>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<sbyte, byte>(ref sByteValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<sbyte, short>(ref sByteValue);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<sbyte, int>(ref sByteValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<sbyte, long>(ref sByteValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) sByteValue;
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<sbyte, ushort>(ref sByteValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<sbyte, uint>(ref sByteValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<sbyte, ulong>(ref sByteValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int16:
                    var shortValue = Convert<TSource, short>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<short, byte>(ref shortValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) shortValue;
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<short, int>(ref shortValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<short, long>(ref shortValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<short, sbyte>(ref shortValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<short, ushort>(ref shortValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<short, uint>(ref shortValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<short, ulong>(ref shortValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.UInt16:
                    var uShortValue = Convert<TSource, ushort>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<ushort, byte>(ref uShortValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<ushort, short>(ref uShortValue);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<ushort, int>(ref uShortValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<ushort, long>(ref uShortValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<ushort, sbyte>(ref uShortValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) uShortValue;
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<ushort, uint>(ref uShortValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<ushort, ulong>(ref uShortValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.UInt32:
                    var uintValue = Convert<TSource, uint>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<uint, byte>(ref uintValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<uint, short>(ref uintValue);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<uint, int>(ref uintValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<uint, long>(ref uintValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<uint, sbyte>(ref uintValue);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<uint, ushort>(ref uintValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) uintValue;
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<uint, ulong>(ref uintValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int64:
                    var longValue = Convert<TSource, long>(ref source);
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<long, byte>(ref longValue);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<long, short>(ref longValue);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<long, int>(ref longValue);
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<long, long>(ref longValue);
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) longValue;
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<long, ushort>(ref longValue);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<long, uint>(ref longValue);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<long, ulong>(ref longValue);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                default:
                    destination = default;
                    return false;
            }

            return true;
        }
        
        static class PrimitiveConverters
        {
            public static void Register()
            {
                // signed integral types
                RegisterInt8Converters();
                RegisterInt16Converters();
                RegisterInt32Converters();
                RegisterInt64Converters();

                // unsigned integral types
                RegisterUInt8Converters();
                RegisterUInt16Converters();
                RegisterUInt32Converters();
                RegisterUInt64Converters();

                // floating point types
                RegisterFloat32Converters();
                RegisterFloat64Converters();

                // .net types
                RegisterBooleanConverters();
                RegisterCharConverters();
                RegisterStringConverters();
                RegisterObjectConverters();

                // Unity vector types
                RegisterVectorConverters();
                
                // support System.Guid by default
                TypeConversion.Register<string, Guid>((ref string g) => new Guid(g));
            }

            static void RegisterInt8Converters()
            {
                Converter<sbyte, char>.Convert = (ref sbyte v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<sbyte, bool>.Convert = (ref sbyte v) => v != 0;
                Converter<sbyte, sbyte>.Convert = (ref sbyte v) => (sbyte) v;
                Converter<sbyte, short>.Convert = (ref sbyte v) => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<sbyte, int>.Convert = (ref sbyte v) => (int) v;
                Converter<sbyte, long>.Convert = (ref sbyte v) => (long) v;
                Converter<sbyte, byte>.Convert = (ref sbyte v) => (byte) v;
                Converter<sbyte, ushort>.Convert = (ref sbyte v) => (ushort) v;
                Converter<sbyte, uint>.Convert = (ref sbyte v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<sbyte, ulong>.Convert = (ref sbyte v) => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<sbyte, float>.Convert = (ref sbyte v) => (float) v;
                Converter<sbyte, double>.Convert = (ref sbyte v) => (double) v;
                Converter<sbyte, object>.Convert = (ref sbyte v) => (object) v;
            }

            static void RegisterInt16Converters()
            {
                Converter<short, char>.Convert = (ref short v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<short, bool>.Convert = (ref short v) => v != 0;
                Converter<short, sbyte>.Convert = (ref short v) =>  (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<short, short>.Convert = (ref short v) => (short) v;
                Converter<short, int>.Convert = (ref short v) => (int) v;
                Converter<short, long>.Convert = (ref short v) => (long) v;
                Converter<short, byte>.Convert = (ref short v) => (byte) v;
                Converter<short, ushort>.Convert = (ref short v) => (ushort) v;
                Converter<short, uint>.Convert = (ref short v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<short, ulong>.Convert = (ref short v) =>  (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<short, float>.Convert = (ref short v) => (float) v;
                Converter<short, double>.Convert = (ref short v) => (double) v;
                Converter<short, object>.Convert = (ref short v) => (object) v;
            }

            static void RegisterInt32Converters()
            {
                Converter<int, char>.Convert = (ref int v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<int, bool>.Convert = (ref int v) => v != 0;
                Converter<int, sbyte>.Convert = (ref int v) => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<int, short>.Convert = (ref int v) => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<int, int>.Convert = (ref int v) => (int) v;
                Converter<int, long>.Convert = (ref int v) => (long) v;
                Converter<int, byte>.Convert = (ref int v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<int, ushort>.Convert = (ref int v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<int, uint>.Convert = (ref int v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<int, ulong>.Convert = (ref int v) => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<int, float>.Convert = (ref int v) => (float) v;
                Converter<int, double>.Convert = (ref int v) => (double) v;
                Converter<int, object>.Convert = (ref int v) => (object) v;
            }

            static void RegisterInt64Converters()
            {
                Converter<long, char>.Convert = (ref long v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<long, bool>.Convert = (ref long v) => v != 0;
                Converter<long, sbyte>.Convert = (ref long v) => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<long, short>.Convert = (ref long v) => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<long, int>.Convert = (ref long v) => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<long, long>.Convert = (ref long v) => (long) v;
                Converter<long, byte>.Convert = (ref long v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<long, ushort>.Convert = (ref long v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<long, uint>.Convert = (ref long v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<long, ulong>.Convert = (ref long v) => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<long, float>.Convert = (ref long v) => (float) v;
                Converter<long, double>.Convert = (ref long v) => (double) v;
                Converter<long, object>.Convert = (ref long v) => (object) v;
            }

            static void RegisterUInt8Converters()
            {
                Converter<byte, char>.Convert = (ref byte v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<byte, bool>.Convert = (ref byte v) => v != 0;
                Converter<byte, sbyte>.Convert = (ref byte v) => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<byte, short>.Convert = (ref byte v) => (short) v;
                Converter<byte, int>.Convert = (ref byte v) => (int) v;
                Converter<byte, long>.Convert = (ref byte v) => (long) v;
                Converter<byte, byte>.Convert = (ref byte v) => (byte) v;
                Converter<byte, ushort>.Convert = (ref byte v) => (ushort) v;
                Converter<byte, uint>.Convert = (ref byte v) => (uint) v;
                Converter<byte, ulong>.Convert = (ref byte v) => (ulong) v;
                Converter<byte, float>.Convert = (ref byte v) => (float) v;
                Converter<byte, double>.Convert = (ref byte v) => (double) v;
                Converter<byte, object>.Convert = (ref byte v) => (object) v;
            }

            static void RegisterUInt16Converters()
            {
                Converter<ushort, char>.Convert = (ref ushort v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<ushort, bool>.Convert = (ref ushort v) => v != 0;
                Converter<ushort, sbyte>.Convert = (ref ushort v) => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<ushort, short>.Convert = (ref ushort v) => (short) Clamp(v, 0, short.MaxValue);
                Converter<ushort, int>.Convert = (ref ushort v) => (int) v;
                Converter<ushort, long>.Convert = (ref ushort v) => (long) v;
                Converter<ushort, byte>.Convert = (ref ushort v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<ushort, ushort>.Convert = (ref ushort v) => (ushort) v;
                Converter<ushort, uint>.Convert = (ref ushort v) => (uint) v;
                Converter<ushort, ulong>.Convert = (ref ushort v) => (ulong) v;
                Converter<ushort, float>.Convert = (ref ushort v) => (float) v;
                Converter<ushort, double>.Convert = (ref ushort v) => (double) v;
                Converter<ushort, object>.Convert = (ref ushort v) => (object) v;
            }

            static void RegisterUInt32Converters()
            {
                Converter<uint, char>.Convert = (ref uint v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<uint, bool>.Convert = (ref uint v) => v != 0;
                Converter<uint, sbyte>.Convert = (ref uint v) => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<uint, short>.Convert = (ref uint v) => (short) Clamp(v, 0, short.MaxValue);
                Converter<uint, int>.Convert = (ref uint v) => (int) Clamp(v, 0, int.MaxValue);
                Converter<uint, long>.Convert = (ref uint v) => (long) Clamp(v, 0, long.MaxValue);
                Converter<uint, byte>.Convert = (ref uint v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<uint, ushort>.Convert = (ref uint v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<uint, uint>.Convert = (ref uint v) => (uint) v;
                Converter<uint, ulong>.Convert = (ref uint v) => (ulong) v;
                Converter<uint, float>.Convert = (ref uint v) => (float) v;
                Converter<uint, double>.Convert = (ref uint v) => (double) v;
                Converter<uint, object>.Convert = (ref uint v) => (object) v;
            }

            static void RegisterUInt64Converters()
            {
                Converter<ulong, char>.Convert = (ref ulong v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<ulong, bool>.Convert = (ref ulong v) => v != 0;
                Converter<ulong, sbyte>.Convert = (ref ulong v) => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<ulong, short>.Convert = (ref ulong v) => (short) Clamp(v, 0, short.MaxValue);
                Converter<ulong, int>.Convert = (ref ulong v) => (int) Clamp(v, 0, int.MaxValue);
                Converter<ulong, long>.Convert = (ref ulong v) => (long) Clamp(v, 0, long.MaxValue);
                Converter<ulong, byte>.Convert = (ref ulong v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<ulong, ushort>.Convert = (ref ulong v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<ulong, uint>.Convert = (ref ulong v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<ulong, ulong>.Convert = (ref ulong v) => (ulong) v;
                Converter<ulong, float>.Convert = (ref ulong v) => (float) v;
                Converter<ulong, double>.Convert = (ref ulong v) => (double) v;
                Converter<ulong, object>.Convert = (ref ulong v) => (object) v;
                Converter<ulong, string>.Convert = (ref ulong v) => v.ToString();
            }

            static void RegisterFloat32Converters()
            {
                Converter<float, char>.Convert = (ref float v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<float, bool>.Convert = (ref float v) => Math.Abs(v) > float.Epsilon;
                Converter<float, sbyte>.Convert = (ref float v) => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<float, short>.Convert = (ref float v) => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<float, int>.Convert = (ref float v) => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<float, long>.Convert = (ref float v) => (long) Clamp(v, long.MinValue, long.MaxValue);
                Converter<float, byte>.Convert = (ref float v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<float, ushort>.Convert = (ref float v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<float, uint>.Convert = (ref float v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<float, ulong>.Convert = (ref float v) => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<float, float>.Convert = (ref float v) => (float) v;
                Converter<float, double>.Convert = (ref float v) => (double) v;
                Converter<float, object>.Convert = (ref float v) => (object) v;
            }

            static void RegisterFloat64Converters()
            {
                Converter<double, char>.Convert = (ref double v) => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<double, bool>.Convert = (ref double v) => Math.Abs(v) > double.Epsilon;
                Converter<double, sbyte>.Convert = (ref double v) => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<double, short>.Convert = (ref double v) => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<double, int>.Convert = (ref double v) => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<double, long>.Convert = (ref double v) => (long) Clamp(v, long.MinValue, long.MaxValue);
                Converter<double, byte>.Convert = (ref double v) => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<double, ushort>.Convert = (ref double v) => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<double, uint>.Convert = (ref double v) => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<double, ulong>.Convert = (ref double v) => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<double, float>.Convert = (ref double v) => (float) Clamp(v, float.MinValue, float.MaxValue);
                Converter<double, double>.Convert = (ref double v) => (double) v;
                Converter<double, object>.Convert = (ref double v) => (object) v;
            }

            static void RegisterBooleanConverters()
            {
                Converter<bool, char>.Convert = (ref bool v) => v ? (char) 1 : (char) 0;
                Converter<bool, bool>.Convert = (ref bool v) => v;
                Converter<bool, sbyte>.Convert = (ref bool v) => v ? (sbyte) 1 : (sbyte) 0;
                Converter<bool, short>.Convert = (ref bool v) => v ? (short) 1 : (short) 0;
                Converter<bool, int>.Convert = (ref bool v) => v ? (int) 1 : (int) 0;
                Converter<bool, long>.Convert = (ref bool v) => v ? (long) 1 : (long) 0;
                Converter<bool, byte>.Convert = (ref bool v) => v ? (byte) 1 : (byte) 0;
                Converter<bool, ushort>.Convert = (ref bool v) => v ? (ushort) 1 : (ushort) 0;
                Converter<bool, uint>.Convert = (ref bool v) => v ? (uint) 1 : (uint) 0;
                Converter<bool, ulong>.Convert = (ref bool v) => v ? (ulong) 1 : (ulong) 0;
                Converter<bool, float>.Convert = (ref bool v) => v ? (float) 1 : (float) 0;
                Converter<bool, double>.Convert = (ref bool v) => v ? (double) 1 : (double) 0;
                Converter<bool, object>.Convert = (ref bool v) => (object) v;
            }
            
            static void RegisterVectorConverters()
            {
#if !UNITY_DOTSPLAYER
                Converter<UnityEngine.Vector2, UnityEngine.Vector2Int>.Convert = (ref UnityEngine.Vector2 v) => new UnityEngine.Vector2Int((int)v.x, (int)v.y);
                Converter<UnityEngine.Vector3, UnityEngine.Vector3Int>.Convert = (ref UnityEngine.Vector3 v) => new UnityEngine.Vector3Int((int)v.x, (int)v.y, (int)v.z);
                Converter<UnityEngine.Vector2Int, UnityEngine.Vector2>.Convert = (ref UnityEngine.Vector2Int v) => v;
                Converter<UnityEngine.Vector3Int, UnityEngine.Vector3>.Convert = (ref UnityEngine.Vector3Int v) => v;
#endif
            }

            static void RegisterCharConverters()
            {
                Converter<string, char>.Convert = (ref string v) =>
                {
                    if (v.Length != 1)
                    {
                        throw new Exception("Not a valid char");
                    }

                    return v[0];
                };
                Converter<char, char>.Convert = (ref char v) => v;
                Converter<char, bool>.Convert = (ref char v) => v != (char) 0;
                Converter<char, sbyte>.Convert = (ref char v) => (sbyte) v;
                Converter<char, short>.Convert = (ref char v) => (short) v;
                Converter<char, int>.Convert = (ref char v) => (int) v;
                Converter<char, long>.Convert = (ref char v) => (long) v;
                Converter<char, byte>.Convert = (ref char v) => (byte) v;
                Converter<char, ushort>.Convert = (ref char v) => (ushort) v;
                Converter<char, uint>.Convert = (ref char v) => (uint) v;
                Converter<char, ulong>.Convert = (ref char v) => (ulong) v;
                Converter<char, float>.Convert = (ref char v) => (float) v;
                Converter<char, double>.Convert = (ref char v) => (double) v;
                Converter<char, object>.Convert = (ref char v) => (object) v;
                Converter<char, string>.Convert = (ref char v) => v.ToString();
            }

            static void RegisterStringConverters()
            {
                Converter<string, string>.Convert = (ref string v) => v;
                Converter<string, char>.Convert = (ref string v) => !string.IsNullOrEmpty(v) ? v[0] : '\0';
                Converter<char, string>.Convert = (ref char v) => v.ToString();
                Converter<string, bool>.Convert = (ref string v) =>
                {
                    if (bool.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, bool>(ref fromDouble)
                        : default;
                };
                Converter<bool, string>.Convert = (ref bool v) => v.ToString();
                Converter<string, sbyte>.Convert = (ref string v) =>
                {
                    if (sbyte.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, sbyte>(ref fromDouble)
                        : default;
                };
                Converter<sbyte, string>.Convert = (ref sbyte v) => v.ToString();
                Converter<string, short>.Convert = (ref string v) =>
                {
                    if (short.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, short>(ref fromDouble)
                        : default;
                };
                Converter<short, string>.Convert = (ref short v) => v.ToString();
                Converter<string, int>.Convert = (ref string v) =>
                {
                    if (int.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, int>(ref fromDouble)
                        : default;
                };
                Converter<int, string>.Convert = (ref int v) => v.ToString();
                Converter<string, long>.Convert = (ref string v) =>
                {
                    if (long.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, long>(ref fromDouble)
                        : default;
                };
                Converter<long, string>.Convert = (ref long v) => v.ToString();
                Converter<string, byte>.Convert = (ref string v) =>
                {
                    if (byte.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, byte>(ref fromDouble)
                        : default;
                };
                Converter<byte, string>.Convert = (ref byte v) => v.ToString();
                Converter<string, ushort>.Convert = (ref string v) =>
                {
                    if (ushort.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, ushort>(ref fromDouble)
                        : default;
                };
                Converter<ushort, string>.Convert = (ref ushort v) => v.ToString();
                Converter<string, uint>.Convert = (ref string v) =>
                {
                    if (uint.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, uint>(ref fromDouble)
                        : default;
                };
                Converter<uint, string>.Convert = (ref uint v) => v.ToString();
                Converter<string, ulong>.Convert = (ref string v) =>
                {
                    if (ulong.TryParse(v, out var r))
                    {
                        return r;
                    }

                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, ulong>(ref fromDouble)
                        : default;
                };
                Converter<ulong, string>.Convert = (ref ulong v) => v.ToString();
                Converter<string, float>.Convert = (ref string v) =>
                {
                    if (float.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, float>(ref fromDouble)
                        : default;
                };
                Converter<float, string>.Convert = (ref float v) => v.ToString(CultureInfo.InvariantCulture);
                Converter<string, double>.Convert = (ref string v) =>
                {
                    double.TryParse(v, out var r);
                    return r;
                };
                Converter<double, string>.Convert = (ref double v) => v.ToString(CultureInfo.InvariantCulture);
                Converter<string, object>.Convert = (ref string v) => v;
            }

            static void RegisterObjectConverters()
            {
                Converter<object, char>.Convert = (ref object v) => v is char value ? value : default;
                Converter<object, bool>.Convert = (ref object v) => v is bool value ? value : default;
                Converter<object, sbyte>.Convert = (ref object v) => v is sbyte value ? value : default;
                Converter<object, short>.Convert = (ref object v) => v is short value ? value : default;
                Converter<object, int>.Convert = (ref object v) => v is int value ? value : default;
                Converter<object, long>.Convert = (ref object v) => v is long value ? value : default;
                Converter<object, byte>.Convert = (ref object v) => v is byte value ? value : default;
                Converter<object, ushort>.Convert = (ref object v) => v is ushort value ? value : default;
                Converter<object, uint>.Convert = (ref object v) => v is uint value ? value : default;
                Converter<object, ulong>.Convert = (ref object v) => v is ulong value ? value : default;
                Converter<object, float>.Convert = (ref object v) => v is float value ? value : default;
                Converter<object, double>.Convert = (ref object v) => v is double value ? value : default;
                Converter<object, object>.Convert = (ref object v) => v;
            } 
            
            static double Clamp(double value, double min, double max)
            {
                if (value < min)
                    value = min;
                else if (value > max)
                    value = max;
                return value;
            }
        }
    }
}
#endif