#if !NET_DOTS
using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Unity.Properties.CodeGen.Blocks
{
    static class ReflectedProperty
    {
        public static TypeDefinition Generate(Context context, TypeReference containerType, IMemberDefinition member, string nameOverride = null)
        {
            if (null == member)
                throw new ArgumentException(nameof(member));

            var memberType = context.Module.ImportReference(Utility.GetMemberType(member).ResolveGenericParameter(containerType));
            var propertyBaseType = context.ImportReference(typeof(ReflectedMemberProperty<,>)).MakeGenericInstanceType(containerType, memberType);
            var memberName = string.IsNullOrEmpty(nameOverride) ? member.Name : nameOverride;

            var type = new TypeDefinition
            (
                @namespace: string.Empty,
                name: Utility.GetSanitizedName(memberName, string.Empty),
                attributes: TypeAttributes.Class | TypeAttributes.NestedPrivate,
                baseType: propertyBaseType
            )
            {
                Scope = containerType.Scope
            };

            var ctorMethod = CreateReflectedMemberPropertyCtorMethod(context, containerType, propertyBaseType, member, memberName);
            type.Methods.Add(ctorMethod);

            return type;
        }

        static MethodDefinition CreateReflectedMemberPropertyCtorMethod(Context context, TypeReference containerType, TypeReference baseType, IMemberDefinition member, string memberName)
        {
            // NOTE: We create our own method reference since this assembly may not reference Unity.Properties on it's own. Thus any attempt
            // to Resolve() a TypeReference from Properties will return null. So instead we create MethodReferences for methods we
            // know will exist ourselves and let the new assembly, which will now include a reference to Properties, resolve at runtime
            var basePropertyConstructor = new MethodReference(".ctor", context.ImportReference(typeof(void)), baseType)
            {
                HasThis = true,
                ExplicitThis = false,
                CallingConvention = MethodCallingConvention.Default
            };

            var parameters = basePropertyConstructor.Parameters;
            if (member is FieldDefinition)
            {
                parameters.Add(new ParameterDefinition(context.ImportReference(typeof(FieldInfo))));
            }
            else if (member is PropertyDefinition)
            {
                parameters.Add(new ParameterDefinition(context.ImportReference(typeof(PropertyInfo))));
            }
            else
            {
                throw new ArgumentException($"No constructor exists for ReflectedMemberProperty({member.GetType()})");
            }
            
            parameters.Add(new ParameterDefinition(context.ImportReference(typeof(string))));

            var method = new MethodDefinition
            (
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                context.ImportReference(typeof(void))
            );

            var il = method.Body.GetILProcessor();

            il.Emit(OpCodes.Ldarg_0); // this

            // typeof({TContainer})
            il.Emit(OpCodes.Ldtoken, context.ImportReference(member.GetResolvedDeclaringType(containerType)));
            il.Emit(OpCodes.Call, context.TypeGetTypeFromTypeHandleMethodReference.Value);

            // {FieldName}
            il.Emit(OpCodes.Ldstr, member.Name);

            var flags = BindingFlags.Instance;

            if (member.IsPublic())
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            il.Emit(OpCodes.Ldc_I4_S, (sbyte) flags);

            if (member is FieldDefinition)
            {
                // GetField
                il.Emit(OpCodes.Callvirt, context.TypeGetFieldMethodReference.Value);
            }
            else if (member is PropertyDefinition)
            {
                // GetProperty
                il.Emit(OpCodes.Callvirt, context.TypeGetPropertyMethodReference.Value);
            }
            
            il.Emit(OpCodes.Ldstr, memberName);

            // : base
            il.Emit(OpCodes.Call, context.Module.ImportReference(basePropertyConstructor));
            il.Emit(OpCodes.Ret);

            return method;
        }
    }
}
#endif