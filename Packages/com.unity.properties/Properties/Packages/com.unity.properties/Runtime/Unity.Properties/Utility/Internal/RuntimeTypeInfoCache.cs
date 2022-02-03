using System;
using System.Reflection;

namespace Unity.Properties.Internal
{
    struct RuntimeTypeInfoCache
    {
        public static bool IsContainerType(Type type)
        {
            return !(type.IsPrimitive || type.IsPointer || type.IsEnum || type == typeof(string));
        }
    }

    /// <summary>
    /// Helper class to avoid paying the cost of runtime type lookups.
    ///
    /// This is also used to abstract underlying type info in the runtime (e.g. RuntimeTypeHandle vs StaticTypeReg)
    /// </summary>
    struct RuntimeTypeInfoCache<T>
    {
        public static readonly bool IsValueType;
        public static readonly bool IsPrimitive;
        public static readonly bool IsInterface;
        public static readonly bool IsAbstract;
        public static readonly bool IsArray;
        public static readonly bool IsMultidimensionalArray;
        public static readonly bool IsEnum;
        
#if !NET_DOTS
        public static readonly bool IsEnumFlags;
        public static readonly bool IsNullable;
#endif

        public static readonly bool IsObjectType;
        public static readonly bool IsStringType;
        public static readonly bool IsContainerType;

        public static readonly bool CanBeNull;
        public static readonly bool IsNullableOrEnum;
        public static readonly bool IsPrimitiveOrString;
        public static readonly bool IsAbstractOrInterface;

#if !UNITY_DOTSPLAYER
        public static readonly bool IsUnityObject;
        public static readonly bool IsLazyLoadReference;
#endif

        static RuntimeTypeInfoCache()
        {
            var type = typeof(T);
            IsValueType = type.IsValueType;
            IsPrimitive = type.IsPrimitive;
            IsInterface = type.IsInterface;
            IsAbstract = type.IsAbstract;
            IsArray = type.IsArray;
            IsEnum = type.IsEnum;

#if !NET_DOTS
            IsEnumFlags = IsEnum && null != type.GetCustomAttribute<FlagsAttribute>();
            IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;
            IsMultidimensionalArray = IsArray && typeof(T).GetArrayRank() != 1;
#endif
            IsObjectType = type == typeof(object);
            IsStringType = type == typeof(string);
            IsContainerType = RuntimeTypeInfoCache.IsContainerType(type);

            CanBeNull = !IsValueType;
            IsNullableOrEnum = IsEnum;
            IsPrimitiveOrString = IsPrimitive || IsStringType;
            IsAbstractOrInterface = IsAbstract || IsInterface;

#if !NET_DOTS
            CanBeNull |= IsNullable;
            IsNullableOrEnum |= IsNullable;
#endif

#if !UNITY_DOTSPLAYER
            IsLazyLoadReference = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UnityEngine.LazyLoadReference<>);
            IsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(type);
#endif
        }
    }
}