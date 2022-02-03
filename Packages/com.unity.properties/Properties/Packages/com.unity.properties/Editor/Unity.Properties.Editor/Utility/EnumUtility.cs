using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Unity.Properties.Editor
{
    static class EnumUtility
    {
        public static IEnumerable<T> EnumerateFlags<T>(this T flags)
            where T : struct, Enum
        {
            foreach (T flag in Enum.GetValues(typeof(T)))
                if (flags.HasFlags(flag))
                    yield return flag;
        }

        static bool HasFlags<TEnum>(this TEnum left, TEnum right) where TEnum : struct, Enum
        {
            var fn = Converter<TEnum>.ConverterFn;
            return (fn(left) & fn(right)) != 0;
        }

        static class Converter<TEnum> where TEnum : struct, Enum
        {
            public static readonly Func<TEnum, long> ConverterFn = ConvertToLong<TEnum>();
        }

        // Taken from https://devblogs.microsoft.com/premier-developer/dissecting-new-generics-constraints-in-c-7-3/
        static Func<T, long> ConvertToLong<T>()
            where T : struct, Enum
        {
            var method = new DynamicMethod(
                name: "ConvertToLong",
                returnType: typeof(long),
                parameterTypes: new[] {typeof(T)},
                m: typeof(EnumUtility).Module,
                skipVisibility: true);

            var ilGen = method.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Conv_I8);
            ilGen.Emit(OpCodes.Ret);
            return (Func<T, long>) method.CreateDelegate(typeof(Func<T, long>));
        }
    }
}
