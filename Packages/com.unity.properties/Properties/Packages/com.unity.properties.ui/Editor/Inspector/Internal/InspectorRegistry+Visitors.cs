using System;
using Unity.Properties.Internal;

namespace Unity.Properties.UI.Internal
{
    interface IResetableVisitor
    {
        void Reset();
    }
    
    struct ScopedVisitor<T> : IDisposable
        where T : class, IVisitor, IResetableVisitor, new()
    {
        public static readonly Pool<T> Pool = new Pool<T>(() => new T(), v => v.Reset());

        readonly T Value;

        public T Visitor => Value;

        ScopedVisitor(T value)
        {
            Value = value;
        }

        public static ScopedVisitor<T> Make()
        {
            return new ScopedVisitor<T>(Pool.Get());
        }

        public void Dispose()
        {
            Pool.Release(Value);
        }

        public static implicit operator T(ScopedVisitor<T> scoped) => scoped.Value;
    }

    static partial class InspectorRegistry
    {
        enum InspectorTarget
        {
            None,
            Inspector,
            PropertyInspector,
            AttributeInspector
        }

        class CustomInspectorVisitor<TDeclaredValueType> : ConcreteTypeVisitor, IResetableVisitor
        {
            public InspectorTarget Target { get; set; }
            public IInspector Inspector { get; private set; }
            public BindingContextElement Root { get; set; }
            public PropertyPath PropertyPath { get; set; }
            public IProperty Property { get; set; }

            protected override void VisitContainer<TValue>(ref TValue value)
            {
                Inspector = GetInspector<TValue>() ?? GetInspector<TDeclaredValueType>();
            }

            public void Reset()
            {
                Inspector = null;
                Root = null;
                PropertyPath = null;
                Property = null;
                Target = InspectorTarget.None;
            }

            IInspector GetInspector<T>()
            {
                IInspector<T> inspector = default;
                switch (Target)
                {
                    case InspectorTarget.Inspector:
                        inspector = InspectorRegistry.GetInspector<T>();
                        break;
                    case InspectorTarget.PropertyInspector:
                        inspector = InspectorRegistry.GetPropertyInspector<T>(Property);
                        break;
                    case InspectorTarget.AttributeInspector:
                        inspector = InspectorRegistry.GetAttributeInspector<T>(Property);
                        break;
                    case InspectorTarget.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (null != inspector)
                {
                    inspector.Context = new InspectorContext<T>(
                        Root,
                        PropertyPath,
                        Property,
                        Property.GetAttributes()
                    );
                }

                return inspector;
            }
        }
    }
}
