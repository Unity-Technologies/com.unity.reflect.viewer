using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Flags used to specify a set of exceptions.
    /// </summary>
    [Flags]
    public enum VisitExceptionType
    {
        /// <summary>
        /// Flag used to specify no exception types.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Flag used to specify internal exceptions thrown by the core visitation.
        /// </summary>
        Internal = 1 << 0,
        
        /// <summary>
        /// Use this flag to specify exceptions thrown from the visitor code itself.
        /// </summary>
        Visitor = 1 << 1,
        
        /// <summary>
        /// Use this flag to specify all exceptions.
        /// </summary>
        All = Internal | Visitor
    }
    
    /// <summary>
    /// Custom parameters to use for visitation.
    /// </summary>
    public struct VisitParameters
    {
        /// <summary>
        /// Use this options to ignore specific exceptions during visitation.
        /// </summary>
        public VisitExceptionType IgnoreExceptions { get; set; }
    }
    
    public static partial class PropertyContainer
    {
        /// <summary>
        /// Visit the specified <paramref name="container"/> using the specified <paramref name="visitor"/>. 
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="parameters">The visit parameters to use.</param>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The given container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">No property bag was found for the given container.</exception>
        public static void Accept(IPropertyBagVisitor visitor, object container, VisitParameters parameters = default)
        {
            Accept(visitor, ref container, parameters);
        }
        
        /// <summary>
        /// Visit the specified <paramref name="container"/> using the specified <paramref name="visitor"/>. 
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="parameters">The visit parameters to use.</param>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The given container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">No property bag was found for the given container.</exception>
        public static void Accept<TContainer>(IPropertyBagVisitor visitor, TContainer container, VisitParameters parameters = default)
        {
            Accept(visitor, ref container, parameters);
        }

        /// <summary>
        /// Visit the specified <paramref name="container"/> using the specified <paramref name="visitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="parameters">The visit parameters to use.</param>
        /// <typeparam name="TContainer">The declared container type.</typeparam>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The container is null.</exception>
        /// <exception cref="MissingPropertyBagException">No property bag was found for the given container.</exception>
        public static void Accept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container, VisitParameters parameters = default)
        {
            var errorCode = VisitErrorCode.Ok;
            
            try
            {
                if (TryAccept(visitor, ref container, out errorCode)) 
                    return;
            }
            catch (Exception)
            {
                if ((parameters.IgnoreExceptions & VisitExceptionType.Visitor) == 0) 
                    throw;
            }

            if ((parameters.IgnoreExceptions & VisitExceptionType.Internal) != 0) 
                return;
            
            switch (errorCode)
            {
                case VisitErrorCode.Ok:
                case VisitErrorCode.InvalidContainerType:
                    break;
                case VisitErrorCode.NullContainer:
                    throw new ArgumentException("The given container was null. Visitation only works for valid non-null containers.");
                case VisitErrorCode.MissingPropertyBag:
                    throw new MissingPropertyBagException(container.GetType());
                default:
                    throw new Exception($"Unexpected VisitErrorCode=[{errorCode}]");
            }
        }

        /// <summary>
        /// Tries to visit the specified <paramref name="container"/> by ref using the specified <paramref name="visitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <typeparam name="TContainer">The declared container type.</typeparam>
        /// <returns><see langword="true"/> if the visitation succeeded; <see langword="false"/> otherwise.</returns>
        public static bool TryAccept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container)
        {
            return TryAccept(visitor, ref container, out _);
        }
       
        /// <summary>
        /// Tries to visit the specified <paramref name="container"/> by ref using the specified <paramref name="visitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="errorCode">When this method returns, contains the error code.</param>
        /// <typeparam name="TContainer">The declared container type.</typeparam>
        /// <returns><see langword="true"/> if the visitation succeeded; <see langword="false"/> otherwise.</returns>
        public static bool TryAccept<TContainer>(IPropertyBagVisitor visitor, ref TContainer container, out VisitErrorCode errorCode)
        {
            if (!RuntimeTypeInfoCache<TContainer>.IsContainerType)
            {
                errorCode = VisitErrorCode.InvalidContainerType;
                return false;
            }
            
            // Can not visit a null container.
            if (RuntimeTypeInfoCache<TContainer>.CanBeNull)
            {
                if (EqualityComparer<TContainer>.Default.Equals(container, default))
                {
                    errorCode = VisitErrorCode.NullContainer;
                    return false;
                }
            }

            if (!RuntimeTypeInfoCache<TContainer>.IsValueType && typeof(TContainer) != container.GetType())
            {
                if (!RuntimeTypeInfoCache.IsContainerType(container.GetType()))
                {
                    errorCode = VisitErrorCode.InvalidContainerType;
                    return false;
                }
                
                var properties = PropertyBagStore.GetPropertyBag(container.GetType());
                
                if (null == properties)
                {
                    errorCode = VisitErrorCode.MissingPropertyBag;
                    return false;
                }
                
                // At this point the generic parameter is useless to us since it's not the correct type.
                // Instead we need to retrieve the untyped property bag and accept on that. Since we don't know the type
                // We need to box the container and let the property bag cast it internally.
                var boxed = (object) container;
                properties.Accept(visitor, ref boxed);
                container = (TContainer) boxed;
            }
            else
            {
                var properties = PropertyBagStore.GetPropertyBag<TContainer>();

                if (null == properties)
                {
                    errorCode = VisitErrorCode.MissingPropertyBag;
                    return false;
                }
                
                PropertyBag.AcceptWithSpecializedVisitor(properties, visitor, ref container);
            }

            errorCode = VisitErrorCode.Ok;
            return true;
        }

        /// <summary>
        /// Visit the specified <paramref name="container"/> using the specified <paramref name="visitor"/> at the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="path">The property path to visit.</param>
        /// <param name="parameters">The visit parameters to use.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The given container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">No property bag was found for the given container.</exception>
        public static void Accept<TContainer>(IPropertyVisitor visitor, ref TContainer container, PropertyPath path, VisitParameters parameters = default)
        {
            var visitAtPath = ValueAtPathVisitor.Pool.Get();
            try
            {
                visitAtPath.Path = path;
                visitAtPath.Visitor = visitor;
                
                Accept(visitAtPath, ref container, parameters);

                if ((parameters.IgnoreExceptions & VisitExceptionType.Internal) == 0)
                {
                    switch (visitAtPath.ErrorCode)
                    {
                        case VisitErrorCode.Ok:
                            break;
                        case VisitErrorCode.InvalidPath:
                            throw new InvalidPathException($"Failed to Visit at Path=[{path}]");
                        default:
                            throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{visitAtPath.ErrorCode}]");
                    }
                }
            }
            finally
            {
                ValueAtPathVisitor.Pool.Release(visitAtPath);
            }
        }
    }
}