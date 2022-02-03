using System;
using System.Collections.Generic;
#if !NET_DOTS
using System.Linq;
#endif

namespace Unity.Properties
{
    public enum ConstructionType
    {
        /// <summary>
        /// The type construction will be done using <see cref="Activator"/>.
        /// </summary>
        Activator,
        
        /// <summary>
        /// The type construction will be done via a method override in <see cref="PropertyBag{TContainer}"/>
        /// </summary>
        PropertyBagOverride,
        
        /// <summary>
        /// Not type construction should be performed for this type.
        /// </summary>
        NotConstructable
    }

    interface IConstructor
    {
        /// <summary>
        /// Returns <see langword="true"/> if the type can be constructed.
        /// </summary>
        ConstructionType ConstructionType { get; }
    }
    
    /// <summary>
    /// The <see cref="IConstructor{T}"/> provides a type construction implementation for a given <typeparamref name="T"/> type. This is an internal interface.
    /// </summary>
    /// <typeparam name="T">The type to be constructed.</typeparam>
    interface IConstructor<out T> : IConstructor
    {
        /// <summary>
        /// Construct an instance of <typeparamref name="T"/> and returns it.
        /// </summary>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        T Construct();
    }    
    
    /// <summary>
    /// The <see cref="IConstructorWithCount{T}"/> provides type construction for a collection <typeparamref name="T"/> type with a count. This is an internal interface.
    /// </summary>
    /// <typeparam name="T">The type to be constructed.</typeparam>
    interface IConstructorWithCount<out T> : IConstructor
    {
        /// <summary>
        /// Construct an instance of <typeparamref name="T"/> and returns it.
        /// </summary>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        T ConstructWithCount(int count);
    }
    
    /// <summary>
    /// Helper class to create new instances for given types.
    /// </summary>
    public static class TypeConstruction
    {
        /// <summary>
        /// Represents the method that will handle constructing a specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type this delegate constructs.</typeparam>
        public delegate T ConstructorMethod<out T>();

        interface ITypeConstructor
        {
            /// <summary>
            /// Returns <see langword="true"/> if the type can be constructed.
            /// </summary>
            bool CanBeConstructed { get; }

            /// <summary>
            /// Construct an instance of the underlying type.
            /// </summary>
            /// <returns>A new instance of concrete type.</returns>
            object Construct();
        }

        interface ITypeConstructor<T> : ITypeConstructor
        {
            /// <summary>
            /// Construct an instance of <typeparamref name="T"/> and returns it.
            /// </summary>
            /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
            new T Construct();

            /// <summary>
            /// Sets an explicit construction method for the <see cref="T"/> type.
            /// </summary>
            /// <param name="constructor">The construction method.</param>
            /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
            void SetExplicitConstructor(ConstructorMethod<T> constructor);
            
            /// <summary>
            /// Un-sets the explicit construction method for the <see cref="T"/> type.
            /// </summary>
            /// <remarks>
            /// An explicit construction method can only be unset if it was previously set with the same instance.
            /// </remarks>
            /// <param name="constructor">The construction method.</param>
            /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
            void UnsetExplicitConstructor(ConstructorMethod<T> constructor);
        }

        class TypeConstructor<T> : ITypeConstructor<T>
        {
            /// <summary>
            /// An explicit user defined constructor for <typeparamref name="T"/>.
            /// </summary>
            ConstructorMethod<T> m_ExplicitConstructor;
            
            /// <summary>
            /// An implicit constructor relying on <see cref="Activator.CreateInstance{T}"/>
            /// </summary>
            ConstructorMethod<T> m_ImplicitConstructor;

            /// <summary>
            /// An explicit constructor provided by an interface implementation. This is used to provide type construction through property bags.
            /// </summary>
            IConstructor<T> m_OverrideConstructor;

            /// <inheritdoc/>
            public bool CanBeConstructed
            {
                get
                {
                    if (null != m_ExplicitConstructor) 
                        return true;
                    
                    if (null != m_OverrideConstructor)
                    {
                        if (m_OverrideConstructor.ConstructionType == ConstructionType.NotConstructable)
                            return false;

                        if (m_OverrideConstructor.ConstructionType == ConstructionType.PropertyBagOverride)
                            return true;
                    }

                    return null != m_ImplicitConstructor;
                }
            }

            public TypeConstructor()
            {
                // Try to get a construction provider through the property bag.
                m_OverrideConstructor = Internal.PropertyBagStore.GetPropertyBag<T>() as IConstructor<T>;
                
#if !NET_DOTS
                SetImplicitConstructor();
#endif
            }

#if !NET_DOTS
            void SetImplicitConstructor()
            {
                var type = typeof(T);
                
                if (type.IsValueType)
                {
                    m_ImplicitConstructor = CreateValueTypeInstance;
                    return;
                }

                if (type.IsAbstract)
                {
                    return;
                }

#if !UNITY_DOTSPLAYER
                if (typeof(UnityEngine.ScriptableObject).IsAssignableFrom(type))
                {
                    m_ImplicitConstructor = CreateScriptableObjectInstance;
                    return;
                }
#endif

                if (null != type.GetConstructor(Array.Empty<Type>()))
                {
                    m_ImplicitConstructor = CreateClassInstance;
                }
            }
            
            static T CreateValueTypeInstance()
            {
                return default;
            }

#if !UNITY_DOTSPLAYER
            static T CreateScriptableObjectInstance()
            {
                return (T) (object) UnityEngine.ScriptableObject.CreateInstance(typeof(T));
            }
#endif
            
            static T CreateClassInstance()
            {
                return Activator.CreateInstance<T>();
            }
#endif

            /// <inheritdoc/>
            public void SetExplicitConstructor(ConstructorMethod<T> constructor)
            {
                if (null != m_ExplicitConstructor)
                    throw new InvalidOperationException();
                
                m_ExplicitConstructor = constructor;
            }

            /// <inheritdoc/>
            public void UnsetExplicitConstructor(ConstructorMethod<T> constructor)
            {
                if (constructor != m_ExplicitConstructor)
                    throw new InvalidOperationException();
                
                m_ExplicitConstructor = null;
            }
            
            /// <inheritdoc/>
            T ITypeConstructor<T>.Construct()
            {
                // First try an explicit constructor set by users.
                if (null != m_ExplicitConstructor)
                    return m_ExplicitConstructor.Invoke();

                // Try custom constructor provided by the property bag.
                if (null != m_OverrideConstructor)
                {
                    if (m_OverrideConstructor.ConstructionType == ConstructionType.NotConstructable)
                    {
#if !NET_DOTS
                        throw new InvalidOperationException($"The type '{typeof(T).Name}' is not constructable.");
#else
                        throw new InvalidOperationException($"The type is not constructable.");
#endif
                    }
                    
                    if (m_OverrideConstructor.ConstructionType == ConstructionType.PropertyBagOverride)
                    {
                        return m_OverrideConstructor.Construct();
                    }
                }
                    
                // Use the implicit construction provided by Activator.
                if (null != m_ImplicitConstructor)
                    return m_ImplicitConstructor.Invoke();
                
#if !NET_DOTS
                throw new InvalidOperationException($"The type '{typeof(T).Name}' is not constructable.");
#else
                throw new InvalidOperationException($"The type is not constructable.");
#endif
            }
            
            /// <inheritdoc/>
            object ITypeConstructor.Construct() => ((ITypeConstructor<T>) this).Construct();
        }

        /// <summary>
        /// The <see cref="NonConstructable"/> class can be used when we can't fully resolve a <see cref="TypeConstructor{T}"/> for a given type.
        /// This can happen if a given type has no property bag and we don't have a strong type to work with.
        /// </summary>
        class NonConstructable : ITypeConstructor
        {
            public bool CanBeConstructed => false;
            public object Construct() => throw new InvalidOperationException($"The type is not constructable.");
        }
        
        /// <summary>
        /// The <see cref="Cache{T}"/> represents a strongly typed reference to a type constructor.
        /// </summary>
        /// <typeparam name="T">The type the constructor can initialize.</typeparam>
        /// <remarks>
        /// Any types in this set are also present in the <see cref="TypeConstruction.s_TypeConstructors"/> set.
        /// </remarks>
        struct Cache<T>
        {
            /// <summary>
            /// Reference to the strongly typed <see cref="ITypeConstructor{TType}"/> for this type. This allows direct access without any dictionary lookups.
            /// </summary>
            public static ITypeConstructor<T> TypeConstructor;
        }
        
        /// <summary>
        /// Provides untyped references to the <see cref="ITypeConstructor{TType}"/> implementations. 
        /// </summary>
        /// <remarks>
        /// Any types in this set are also present in the <see cref="Cache{T}"/>.
        /// </remarks>
        static readonly Dictionary<Type, ITypeConstructor> s_TypeConstructors = new Dictionary<Type, ITypeConstructor>();
        
#if !NET_DOTS
        static readonly System.Reflection.MethodInfo s_CreateTypeConstructor;
#endif
    
        static TypeConstruction()
        {
            SetExplicitConstructionMethod(() => string.Empty);
#if !NET_DOTS
            s_CreateTypeConstructor = typeof(TypeConstruction).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .First(x => x.Name == nameof(CreateTypeConstructor) && x.IsGenericMethod);;
#endif
        }

        /// <summary>
        /// The <see cref="TypeConstructorVisitor"/> is used to 
        /// </summary>
        class TypeConstructorVisitor : ITypeVisitor
        {
            public ITypeConstructor TypeConstructor;

            public void Visit<TContainer>()
                => TypeConstructor = CreateTypeConstructor<TContainer>();
        }
        
        /// <summary>
        /// Creates a new strongly typed <see cref="TypeConstructor{TType}"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// This method will attempt to use properties to get the strongly typed reference. If no property bag exists it will fallback to a reflection based approach.
        /// </remarks>
        /// <param name="type">The type to create a constructor for.</param>
        /// <returns>A <see cref="TypeConstructor{TType}"/> for the specified type.</returns>
        static ITypeConstructor CreateTypeConstructor(Type type)
        {
            var properties = Internal.PropertyBagStore.GetPropertyBag(type);
            
            // Attempt to use properties double dispatch to call the strongly typed create method. This avoids expensive reflection calls and allows support for NET_DOTS.
            if (null != properties)
            {
                var visitor = new TypeConstructorVisitor();
                properties.Accept(visitor);
                return visitor.TypeConstructor;
            }
            
#if !NET_DOTS
            if (type.ContainsGenericParameters)
            {
                var constructor = new NonConstructable();
                s_TypeConstructors[type] = constructor;
                return constructor;
            }
            
            // This type has no property bag associated with it. Fallback to reflection to create our type constructor.
            return s_CreateTypeConstructor
                .MakeGenericMethod(type)
                .Invoke(null, null) as ITypeConstructor; 
#else
            // Nothing we can do here. No type constructor can be provided for this type.
            var constructor = new NonConstructable();
            s_TypeConstructors[type] = constructor;
            return constructor;
#endif
        }

        /// <summary>
        /// Creates a new strongly typed <see cref="TypeConstructor{TType}"/> for the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to create a constructor for.</typeparam>
        /// <returns>A <see cref="TypeConstructor{TType}"/> for the specified type.</returns>
        static ITypeConstructor<T> CreateTypeConstructor<T>()
        {
            var constructor = new TypeConstructor<T>();
            Cache<T>.TypeConstructor = constructor;
            s_TypeConstructors[typeof(T)] = constructor;
            return constructor;
        }
        
        /// <summary>
        /// Gets the internal <see cref="ITypeConstructor"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// This method will return null if the type is not constructable on the current platform.
        /// </remarks>
        /// <param name="type">The type to get a constructor for.</param>
        /// <returns>A <see cref="ITypeConstructor"/> for the specified type.</returns>
        static ITypeConstructor GetTypeConstructor(Type type)
        {
            return s_TypeConstructors.TryGetValue(type, out var constructor) 
                ? constructor 
                : CreateTypeConstructor(type);
        }

        /// <summary>
        /// Gets the internal <see cref="ITypeConstructor"/> for the specified <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This method will return null if the type is not constructable on the current platform.
        /// </remarks>
        /// <typeparam name="T">The type to create a constructor for.</typeparam>
        /// <returns>A <see cref="ITypeConstructor{TType}"/> for the specified type.</returns>
        static ITypeConstructor<T> GetTypeConstructor<T>()
        {
            return null != Cache<T>.TypeConstructor 
                ? Cache<T>.TypeConstructor 
                : CreateTypeConstructor<T>();
        }
        
        /// <summary>
        /// Returns <see langword="true"/> if the specified type is constructable.
        /// </summary>
        /// <remarks>
        /// Constructable is defined as either having a default or implicit constructor or having a registered construction method.
        /// </remarks>
        /// <param name="type">The type to query.</param>
        /// <returns><see langword="true"/> if the given type is constructable.</returns>
        public static bool CanBeConstructed(Type type)
            => GetTypeConstructor(type).CanBeConstructed;
        
        /// <summary>
        /// Returns <see langword="true"/> if type <see cref="T"/> is constructable.
        /// </summary>
        /// <remarks>
        /// Constructable is defined as either having a default or implicit constructor or having a registered construction method.
        /// </remarks>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns><see langword="true"/> if type <see cref="T"/> is constructable.</returns>
        public static bool CanBeConstructed<T>()
            => GetTypeConstructor<T>().CanBeConstructed;

        /// <summary>
        /// Sets the explicit construction method for the <see cref="T"/>.
        /// </summary>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
        /// <returns><see langword="true"/> if the constructor was set; otherwise, <see langword="false"/>.</returns>
        public static bool TrySetExplicitConstructionMethod<T>(ConstructorMethod<T> constructor)
        {
            try
            {
                GetTypeConstructor<T>().SetExplicitConstructor(constructor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Un-sets the explicit construction method for the <see cref="T"/> type.
        /// </summary>
        /// <remarks>
        /// An explicit construction method can only be unset if it was previously set with the same instance.
        /// </remarks>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
        /// <returns><see langword="true"/> if the constructor was unset; otherwise, <see langword="false"/>.</returns>
        public static bool TryUnsetExplicitConstructionMethod<T>(ConstructorMethod<T> constructor)
        {
            try
            {
                GetTypeConstructor<T>().UnsetExplicitConstructor(constructor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets an explicit construction method for the <see cref="T"/> type.
        /// </summary>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
        public static void SetExplicitConstructionMethod<T>(ConstructorMethod<T> constructor)
            => GetTypeConstructor<T>().SetExplicitConstructor(constructor);

        /// <summary>
        /// Un-sets the explicit construction method for the <see cref="T"/> type.
        /// </summary>
        /// <remarks>
        /// An explicit construction method can only be unset if it was previously set with the same instance.
        /// </remarks>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="T">The type to set the explicit construction method.</typeparam>
        public static void UnsetExplicitConstructionMethod<T>(ConstructorMethod<T> constructor)
            => GetTypeConstructor<T>().UnsetExplicitConstructor(constructor);
        
        /// <summary>
        /// Constructs a new instance of the specified <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">The type we want to create a new instance of.</typeparam>
        /// <returns>A new instance of the <see cref="T"/>.</returns>
        /// <exception cref="InvalidOperationException">The specified <see cref="T"/> has no available constructor.</exception>
        public static T Construct<T>()
        {
            var constructor = GetTypeConstructor<T>();

            CheckCanBeConstructed(constructor);

            return constructor.Construct();
        }
        
        /// <summary>
        /// Constructs a new instance of the specified <see cref="T"/>.
        /// </summary>
        /// <param name="instance">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type to create an instance of.</typeparam>
        /// <returns><see langword="true"/> if a new instance of type <see cref="T"/> was created; otherwise, <see langword="false"/>.</returns>
        public static bool TryConstruct<T>(out T instance)
        {
            var constructor = GetTypeConstructor<T>();

            if (constructor.CanBeConstructed)
            {
                instance = constructor.Construct();
                return true;
            }

            instance = default;
            return false;
        }
        
        /// <summary>
        /// Constructs a new instance of the given type type and returns it as <see cref="T"/>.
        /// </summary>
        /// <param name="derivedType">The type we want to create a new instance of.</param>
        /// <typeparam name="T">The type we want to create a new instance of.</typeparam>
        /// <returns>a new instance of the <see cref="T"/> type.</returns>
        /// <exception cref="ArgumentException">Thrown when the given type is not assignable to <see cref="T"/>.</exception>
        public static T Construct<T>(Type derivedType)
        {
            var constructor = GetTypeConstructor(derivedType);

            CheckIsAssignableFrom(typeof(T), derivedType);
            CheckCanBeConstructed(constructor, derivedType);

            return (T) constructor.Construct();
        }
        
        /// <summary>
        /// Tries to constructs a new instance of the given type type and returns it as <see cref="T"/>.
        /// </summary>
        /// <param name="derivedType">The type we want to create a new instance of.</param>
        /// <param name="value">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type we want to create a new instance of.</typeparam>
        /// <returns><see langword="true"/> if a new instance of the given type could be created.</returns>
        public static bool TryConstruct<T>(Type derivedType, out T value)
        {
            if (!typeof(T).IsAssignableFrom(derivedType))
            {
                value = default;
                value = default;
                return false;
            }
                
            var constructor = GetTypeConstructor(derivedType);
            
            if (!constructor.CanBeConstructed)
            {
                value = default;
                return false;
            }

            value = (T) constructor.Construct();
            return true;
        }
        
        /// <summary>
        /// Construct a new instance of an array with the given count.
        /// </summary>
        /// <param name="count">The size of the array to construct.</param>
        /// <typeparam name="TArray">The array type to construct.</typeparam>
        /// <returns>The array newly constructed array.</returns>
        /// <exception cref="ArgumentException">Thrown is count is negative or if <see cref="TArray"/> is not an array type.</exception>
        public static TArray ConstructArray<TArray>(int count = 0)
        {
            if (count < 0)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array with {nameof(count)}={count}");
            }

            var properties = Internal.PropertyBagStore.GetPropertyBag<TArray>();

            if (properties is IConstructorWithCount<TArray> constructor)
            {
                return constructor.ConstructWithCount(count);
            }
            
            var type = typeof(TArray);

            if (!type.IsArray)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array, since {typeof(TArray).Name} is not an array type.");
            }

            var elementType = type.GetElementType();
            if (null == elementType)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array, since {typeof(TArray).Name}.{nameof(Type.GetElementType)}() returned null.");
            }
            
            return (TArray) (object) Array.CreateInstance(elementType, count);
        }
        
        /// <summary>
        /// Tries to construct a new instance of an array with the given count.
        /// </summary>
        /// <param name="count">The count the array should have.</param>
        /// <param name="instance">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="TType"/>.</param>
        /// <typeparam name="TArray">The array type.</typeparam>
        /// <returns><see langword="true"/> if the type was constructed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown is count is negative or if <see cref="TArray"/> is not an array type.</exception>
        public static bool TryConstructArray<TArray>(int count, out TArray instance)
        {
            if (count < 0)
            {
                instance = default;
                return false;
            }

            var properties = Internal.PropertyBagStore.GetPropertyBag<TArray>();

            if (properties is IConstructorWithCount<TArray> constructor)
            {
                try
                {
                    instance = constructor.ConstructWithCount(count);
                    return true;
                }
                catch
                {
                    // continue
                }
            }

            var type = typeof(TArray);

            if (!type.IsArray)
            {
                instance = default;
                return false;   
            }

            var elementType = type.GetElementType();
            
            if (null == elementType)
            {
                instance = default;
                return false;
            }
            
            instance = (TArray) (object) Array.CreateInstance(elementType, count);
            return true;
        }

        /// <summary>
        /// Construct a new instance of an array with the given type and given count.
        /// </summary>
        /// <param name="derivedType">The type we want to create a new instance of.</param>
        /// <param name="count">The size of the array to construct.</param>
        /// <typeparam name="TArray">The array type to construct.</typeparam>
        /// <returns>The array newly constructed array.</returns>
        /// <exception cref="ArgumentException">Thrown is count is negative or if <see cref="TArray"/> is not an array type.</exception>
        public static TArray ConstructArray<TArray>(Type derivedType, int count = 0)
        {
            if (count < 0)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array with {nameof(count)}={count}");
            }

            var properties = Internal.PropertyBagStore.GetPropertyBag(derivedType);

            if (properties is IConstructorWithCount<TArray> constructor)
            {
                return constructor.ConstructWithCount(count);
            }
            
            var type = typeof(TArray);

            if (!type.IsArray)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array, since {typeof(TArray).Name} is not an array type.");
            }

            var elementType = type.GetElementType();
            if (null == elementType)
            {
                throw new ArgumentException($"{nameof(TypeConstruction)}: Cannot construct an array, since {typeof(TArray).Name}.{nameof(Type.GetElementType)}() returned null.");
            }
            
            return (TArray) (object) Array.CreateInstance(elementType, count);
        }

        static void CheckIsAssignableFrom(Type type, Type derivedType)
        {
            if (!type.IsAssignableFrom(derivedType))
            {
#if !NET_DOTS
                throw new ArgumentException($"Could not create instance of type `{derivedType.Name}` and convert to `{type.Name}`: The given type is not assignable to target type.");
#else
                throw new ArgumentException($"The given type is not assignable to target type.");
#endif
            }
        }

        static void CheckCanBeConstructed<T>(ITypeConstructor<T> constructor)
        {
            if (!constructor.CanBeConstructed)
            {
#if !NET_DOTS
                throw new InvalidOperationException($"Type `{typeof(T).Name}` could not be constructed. A parameter-less constructor or an explicit construction method is required.");
#else
                throw new ArgumentException($"Type could not be constructed. A parameter-less constructor or an explicit construction method is required.");
#endif
            }
        }
        
        static void CheckCanBeConstructed(ITypeConstructor constructor, Type type)
        {
            if (!constructor.CanBeConstructed)
            {
#if !NET_DOTS
                throw new InvalidOperationException($"Type `{type.Name}` could not be constructed. A parameter-less constructor or an explicit construction method is required.");
#else
                throw new ArgumentException($"Type could not be constructed. A parameter-less constructor or an explicit construction method is required.");
#endif
            }
        }
    }
}