using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Unity.Properties
{
    class TypeResolver
    {
        readonly IGenericInstance m_TypeDefinitionContext;
        readonly IGenericInstance m_MethodDefinitionContext;

        static TypeResolver s_Empty;

        public static TypeResolver For(TypeReference typeReference)
        {
            return typeReference.IsGenericInstance ? new TypeResolver((GenericInstanceType) typeReference) : Empty;
        }

        public static TypeResolver Empty
        {
            get
            {
                if (s_Empty == null)
                    s_Empty = new TypeResolver();

                return s_Empty;
            }
        }

        public static TypeResolver For(TypeReference typeReference, MethodReference methodReference)
        {
            return new TypeResolver(typeReference as GenericInstanceType, methodReference as GenericInstanceMethod);
        }

        TypeResolver()
        {
        }

        public TypeResolver(GenericInstanceType typeDefinitionContext)
        {
            m_TypeDefinitionContext = typeDefinitionContext;
        }

        public TypeResolver(GenericInstanceMethod methodDefinitionContext)
        {
            m_MethodDefinitionContext = methodDefinitionContext;
        }

        public TypeResolver(GenericInstanceType typeDefinitionContext, GenericInstanceMethod methodDefinitionContext)
        {
            m_TypeDefinitionContext = typeDefinitionContext;
            m_MethodDefinitionContext = methodDefinitionContext;
        }

        public MethodReference Resolve(MethodReference method)
        {
            return Resolve(method, false);
        }

        public MethodReference Resolve(MethodReference method, bool resolveGenericParameters)
        {
            var methodReference = method;
            if (IsDummy())
                return methodReference;

            var declaringType = Resolve(method.DeclaringType);

            if (method is GenericInstanceMethod genericInstanceMethod)
            {
                methodReference = new MethodReference(method.Name, method.ReturnType, declaringType);

                foreach (var p in method.Parameters)
                    methodReference.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));

                foreach (var gp in genericInstanceMethod.ElementMethod.GenericParameters)
                    methodReference.GenericParameters.Add(new GenericParameter(gp.Name, methodReference));

                methodReference.HasThis = method.HasThis;

                var m = new GenericInstanceMethod(methodReference);
                foreach (var ga in genericInstanceMethod.GenericArguments)
                {
                    m.GenericArguments.Add(Resolve(ga));
                }

                methodReference = m;
            }
            else
            {
                if (resolveGenericParameters && method.HasGenericParameters)
                {
                    var newGenericInstanceMethod = new GenericInstanceMethod(method);
                    foreach (var gp in method.GenericParameters)
                        newGenericInstanceMethod.GenericArguments.Add(Resolve(gp));
                    return newGenericInstanceMethod;
                }

                methodReference = new MethodReference(method.Name, method.ReturnType, declaringType);

                foreach (var gp in method.GenericParameters)
                    methodReference.GenericParameters.Add(new GenericParameter(gp.Name, methodReference));

                foreach (var p in method.Parameters)
                    methodReference.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));

                methodReference.HasThis = method.HasThis;
                methodReference.MetadataToken = method.MetadataToken;
            }


            return methodReference;
        }

        public FieldReference Resolve(FieldReference field)
        {
            var declaringType = Resolve(field.DeclaringType);

            if (declaringType == field.DeclaringType)
                return field;

            return new FieldReference(field.Name, field.FieldType, declaringType);
        }

        public TypeReference ResolveReturnType(MethodReference method)
        {
            return Resolve(GenericParameterResolver.ResolveReturnTypeIfNeeded(method));
        }

        public TypeReference ResolveReturnType(CallSite callSite)
        {
            return Resolve(callSite.ReturnType);
        }

        public TypeReference ResolveParameterType(MethodReference method, ParameterReference parameter)
        {
            return Resolve(GenericParameterResolver.ResolveParameterTypeIfNeeded(method, parameter));
        }

        public TypeReference ResolveVariableType(MethodReference method, VariableReference variable)
        {
            return Resolve(GenericParameterResolver.ResolveVariableTypeIfNeeded(method, variable));
        }

        public TypeReference ResolveFieldType(FieldReference field)
        {
            return Resolve(GenericParameterResolver.ResolveFieldTypeIfNeeded(field));
        }

        public TypeReference Resolve(TypeReference typeReference)
        {
            return Resolve(typeReference, true);
        }

        public TypeReference Resolve(TypeReference typeReference, bool resolveGenericParameters)
        {
            if (IsDummy())
                return typeReference;

            if (m_TypeDefinitionContext != null && m_TypeDefinitionContext.GenericArguments.Contains(typeReference))
                return typeReference;
            if (m_MethodDefinitionContext != null && m_MethodDefinitionContext.GenericArguments.Contains(typeReference))
                return typeReference;

            switch (typeReference)
            {
                case GenericParameter genericParameter when m_TypeDefinitionContext != null && m_TypeDefinitionContext.GenericArguments.Contains(genericParameter):
                    return genericParameter;
                case GenericParameter genericParameter when m_MethodDefinitionContext != null && m_MethodDefinitionContext.GenericArguments.Contains(genericParameter):
                    return genericParameter;
                case GenericParameter genericParameter:
                    return ResolveGenericParameter(genericParameter);
                case ArrayType arrayType:
                    return new ArrayType(Resolve(arrayType.ElementType), arrayType.Rank);
                case PointerType pointerType:
                    return new PointerType(Resolve(pointerType.ElementType));
                case ByReferenceType byReferenceType:
                    return new ByReferenceType(Resolve(byReferenceType.ElementType));
                case PinnedType pinnedType:
                    return new PinnedType(Resolve(pinnedType.ElementType));
                case GenericInstanceType genericInstanceType:
                {
                    var newGenericInstanceType = new GenericInstanceType(genericInstanceType.ElementType);
                    foreach (var genericArgument in genericInstanceType.GenericArguments)
                        newGenericInstanceType.GenericArguments.Add(Resolve(genericArgument));
                    newGenericInstanceType.MetadataToken = genericInstanceType.MetadataToken;
                    return newGenericInstanceType;
                }
                case RequiredModifierType requiredModType:
                    return new RequiredModifierType(requiredModType.ModifierType, Resolve(requiredModType.ElementType, resolveGenericParameters));
                case OptionalModifierType optionalModType:
                    return Resolve(optionalModType.ElementType, resolveGenericParameters);
            }

            if (resolveGenericParameters)
            {
                if (typeReference is TypeDefinition typeDefinition && typeDefinition.HasGenericParameters)
                {
                    var newGenericInstanceType = new GenericInstanceType(typeDefinition);
                    foreach (var gp in typeDefinition.GenericParameters)
                        newGenericInstanceType.GenericArguments.Add(Resolve(gp));
                    return newGenericInstanceType;
                }
            }

            if (typeReference is TypeSpecification)
                throw new NotSupportedException($"The type {typeReference.FullName} cannot be resolved correctly.");

            return typeReference;
        }

        internal TypeResolver Nested(GenericInstanceMethod genericInstanceMethod)
        {
            return new TypeResolver(m_TypeDefinitionContext as GenericInstanceType, genericInstanceMethod);
        }

        TypeReference ResolveGenericParameter(GenericParameter genericParameter)
        {
            if (genericParameter.Owner == null)
                return HandleOwnerlessInvalidILCode(genericParameter);

            if (!(genericParameter.Owner is MemberReference memberReference))
                throw new NotSupportedException();

            return genericParameter.Type == GenericParameterType.Type
                ? m_TypeDefinitionContext.GenericArguments[genericParameter.Position]
                : (m_MethodDefinitionContext != null ? m_MethodDefinitionContext.GenericArguments[genericParameter.Position] : genericParameter);
        }

        TypeReference HandleOwnerlessInvalidILCode(GenericParameter genericParameter)
        {
            // NOTE(Josh): If owner is null and we have a method parameter, then we'll assume that the method parameter
            // is actually a type parameter, and we'll use the type parameter from the corresponding position. I think
            // this assumption is valid, but if you're visiting this code then I might have been proven wrong.
            if (genericParameter.Type == GenericParameterType.Method && (m_TypeDefinitionContext != null && genericParameter.Position < m_TypeDefinitionContext.GenericArguments.Count))
                return m_TypeDefinitionContext.GenericArguments[genericParameter.Position];

            // NOTE(gab): Owner cannot be null, but sometimes the Mono compiler generates invalid IL and we
            // end up in this situation.
            // When we do, we assume that the runtime doesn't care about the resolved type of the GenericParameter,
            // thus we return a reference to System.Object.
            return genericParameter.Module.TypeSystem.Object;
        }

        bool IsDummy()
        {
            return m_TypeDefinitionContext == null && m_MethodDefinitionContext == null;
        }
    }

    class GenericParameterResolver
    {
        internal static TypeReference ResolveReturnTypeIfNeeded(MethodReference methodReference)
        {
            if (methodReference.DeclaringType.IsArray && methodReference.Name == "Get")
                return methodReference.ReturnType;

            var genericInstanceMethod = methodReference as GenericInstanceMethod;
            var declaringGenericInstanceType = methodReference.DeclaringType as GenericInstanceType;

            if (genericInstanceMethod == null && declaringGenericInstanceType == null)
                return methodReference.ReturnType;

            return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, methodReference.ReturnType);
        }

        internal static TypeReference ResolveThisTypeIfNeeded(MethodReference methodReference)
        {
            var thisType = methodReference.DeclaringType;
            if (methodReference.DeclaringType.IsArray && (methodReference.Name == "Get" || methodReference.Name == "Set"))
                return thisType;

            var genericInstanceMethod = methodReference as GenericInstanceMethod;
            var declaringGenericInstanceType = methodReference.DeclaringType as GenericInstanceType;

            if ((genericInstanceMethod == null && declaringGenericInstanceType == null) || declaringGenericInstanceType == thisType)
                return thisType;

            return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, thisType);
        }

        internal static TypeReference ResolveFieldTypeIfNeeded(FieldReference fieldReference)
        {
            return ResolveIfNeeded(null, fieldReference.DeclaringType as GenericInstanceType, fieldReference.FieldType);
        }

        internal static TypeReference ResolveParameterTypeIfNeeded(MethodReference method, ParameterReference parameter)
        {
            var genericInstanceMethod = method as GenericInstanceMethod;
            var declaringGenericInstanceType = method.DeclaringType as GenericInstanceType;

            if (genericInstanceMethod == null && declaringGenericInstanceType == null)
                return parameter.ParameterType;

            return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, parameter.ParameterType);
        }

        internal static TypeReference ResolveVariableTypeIfNeeded(MethodReference method, VariableReference variable)
        {
            var genericInstanceMethod = method as GenericInstanceMethod;
            var declaringGenericInstanceType = method.DeclaringType as GenericInstanceType;

            if (genericInstanceMethod == null && declaringGenericInstanceType == null)
                return variable.VariableType;

            return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, variable.VariableType);
        }

        static TypeReference ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance declaringGenericInstanceType, TypeReference parameterType)
        {
            if (parameterType is ByReferenceType byRefType)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, byRefType);

            if (parameterType is ArrayType arrayType)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, arrayType);

            if (parameterType is GenericInstanceType genericInstanceType)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, genericInstanceType);

            if (parameterType is GenericParameter genericParameter)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, genericParameter);

            if (parameterType is PointerType pointerType)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, pointerType);

            if (parameterType is RequiredModifierType requiredModifierType && requiredModifierType.ContainsGenericParameters())
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, requiredModifierType);

            if (parameterType is OptionalModifierType optionalModifierType && optionalModifierType.ContainsGenericParameters())
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, optionalModifierType.ElementType);

            if (parameterType is PinnedType pinnedType)
                return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, pinnedType.ElementType);

            if (parameterType.ContainsGenericParameters())
                throw new Exception("Unexpected generic parameter.");

            return parameterType;
        }

        static TypeReference ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, GenericParameter genericParameterElement)
        {
            return (genericParameterElement.MetadataType == MetadataType.MVar)
                ? (genericInstanceMethod != null ? genericInstanceMethod.GenericArguments[genericParameterElement.Position] : genericParameterElement)
                : genericInstanceType.GenericArguments[genericParameterElement.Position];
        }

        static ArrayType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, ArrayType arrayType)
        {
            return new ArrayType(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, arrayType.ElementType), arrayType.Rank);
        }

        static ByReferenceType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, ByReferenceType byReferenceType)
        {
            return new ByReferenceType(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, byReferenceType.ElementType));
        }

        static PointerType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, PointerType pointerType)
        {
            return new PointerType(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, pointerType.ElementType));
        }

        static RequiredModifierType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, RequiredModifierType requiredModifierType)
        {
            return new RequiredModifierType(requiredModifierType.ModifierType, ResolveIfNeeded(genericInstanceMethod, genericInstanceType, requiredModifierType.ElementType));
        }

        static GenericInstanceType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, GenericInstanceType genericInstanceType1)
        {
            if (!genericInstanceType1.ContainsGenericParameters())
                return genericInstanceType1;

            var newGenericInstance = new GenericInstanceType(genericInstanceType1.ElementType);

            foreach (var genericArgument in genericInstanceType1.GenericArguments)
            {
                if (!genericArgument.IsGenericParameter)
                {
                    newGenericInstance.GenericArguments.Add(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, genericArgument));
                    continue;
                }

                var genParam = (GenericParameter) genericArgument;

                switch (genParam.Type)
                {
                    case GenericParameterType.Type:
                    {
                        if (genericInstanceType == null)
                            throw new NotSupportedException();

                        newGenericInstance.GenericArguments.Add(genericInstanceType.GenericArguments[genParam.Position]);
                    }
                        break;

                    case GenericParameterType.Method:
                    {
                        if (genericInstanceMethod == null)
                            newGenericInstance.GenericArguments.Add(genParam);
                        else
                            newGenericInstance.GenericArguments.Add(genericInstanceMethod.GenericArguments[genParam.Position]);
                    }
                        break;
                }
            }

            return newGenericInstance;
        }
    }

    static class Extensions
    {
        public static bool ContainsGenericParameters(this TypeReference typeReference)
        {
            if (typeReference is GenericParameter)
                return true;

            if (typeReference is ArrayType arrayType)
                return arrayType.ElementType.ContainsGenericParameters();

            if (typeReference is PointerType pointerType)
                return pointerType.ElementType.ContainsGenericParameters();

            if (typeReference is ByReferenceType byRefType)
                return byRefType.ElementType.ContainsGenericParameters();

            if (typeReference is SentinelType sentinelType)
                return sentinelType.ElementType.ContainsGenericParameters();

            if (typeReference is PinnedType pinnedType)
                return pinnedType.ElementType.ContainsGenericParameters();

            if (typeReference is RequiredModifierType requiredModifierType)
                return requiredModifierType.ElementType.ContainsGenericParameters();

            if (typeReference is OptionalModifierType optionalModifierType)
                return optionalModifierType.ElementType.ContainsGenericParameters();

            if (typeReference is GenericInstanceType genericInstance)
                return genericInstance.GenericArguments.Any(ContainsGenericParameters);

            if (typeReference is TypeSpecification)
                throw new NotSupportedException();

            return false;
        }
    }
}