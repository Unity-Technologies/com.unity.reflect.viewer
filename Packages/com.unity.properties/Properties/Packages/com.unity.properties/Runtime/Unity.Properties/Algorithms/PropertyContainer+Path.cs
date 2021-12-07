#if !NET_DOTS
using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        class ValueAtPathVisitor : PathVisitor
        {
            public static readonly Pool<ValueAtPathVisitor> Pool = new Pool<ValueAtPathVisitor>(() => new ValueAtPathVisitor(), v => v.Reset()); 
            public IPropertyVisitor Visitor;

            public override void Reset()
            {
                base.Reset();
                Visitor = default;
                ReadonlyVisit = true;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                ((IPropertyAccept<TContainer>) property).Accept(Visitor, ref container);
            }
        }
        
        class ExistsAtPathVisitor : PathVisitor
        {
            public static readonly Pool<ExistsAtPathVisitor> Pool = new Pool<ExistsAtPathVisitor>(() => new ExistsAtPathVisitor(), v => v.Reset()); 
            public bool Exists;

            public override void Reset()
            {
                base.Reset();
                Exists = default;
                ReadonlyVisit = true;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                Exists = true;
            }
        }
        
        /// <summary>
        /// Returns <see langword="true"/> if a property exists at the specified <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <returns><see langword="true"/> if a property can be found at path.</returns>
        public static bool IsPathValid(ref object container, PropertyPath path)
            => IsPathValid<object>(ref container, path);

        /// <summary>
        /// Returns <see langword="true"/> if a property exists at the specified <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <returns><see langword="true"/> if a property can be found at path.</returns>
        public static bool IsPathValid<TContainer>(ref TContainer container, PropertyPath path)
        {
            var visitor = ExistsAtPathVisitor.Pool.Get();
            try
            {
                visitor.Path = path;
                TryAccept(visitor, ref container);
                return visitor.Exists;
            }
            finally
            {
                ExistsAtPathVisitor.Pool.Release(visitor);
            }
        }
    }
}
#endif