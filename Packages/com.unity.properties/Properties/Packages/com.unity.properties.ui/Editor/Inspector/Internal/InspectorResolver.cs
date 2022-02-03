using System;
using System.Collections.Generic;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Base interface to define a constraint that needs to be verified when filtering the inspector types.
    /// </summary>
    interface IInspectorConstraint
    {
        /// <summary>
        /// Returns true if the constraint is satisfied for the provided inspector type. 
        /// </summary>
        /// <param name="inspectorType">the inspector type that must satisfy the constraint</param>
        /// <returns>true if constraint is satisfied; false otherwise</returns>
        bool Satisfy(Type inspectorType);
    }

    /// <summary>
    /// Helper class to easily build inspector contraints.
    /// </summary>
    static class InspectorConstraint
    {
        /// <summary>
        /// Helper class to invert a constraint.
        /// </summary>
        public static class Not
        {
            /// <summary>
            /// Create an instance of an inverted <see cref="AssignableToConstraint"/>. 
            /// </summary>
            /// <typeparam name="TType">The type that must not be assignable to</typeparam>
            /// <returns>The constraint instance</returns>
            public static InvertConstraint AssignableTo<TType>()
            {
                return AssignableTo(typeof(TType));
            }

            /// <summary>
            /// Create an instance of an inverted <see cref="AssignableToConstraint"/>. 
            /// </summary>
            /// <param name="inspectorType">The type that must not be assignable to</param>
            /// <returns>The constraint instance</returns>
            public static InvertConstraint AssignableTo(Type inspectorType)
            {
                return new InvertConstraint(new AssignableToConstraint(inspectorType));
            }
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssignableToConstraint"/>. 
        /// </summary>
        /// <param name="type">The type that must be assignable to</param>
        /// <returns>The constraint instance</returns>
        public static AssignableToConstraint AssignableTo(Type type)
        {
            return new AssignableToConstraint(type);
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssignableToConstraint"/>. 
        /// </summary>
        /// <typeparam name="TType">The type that must be assignable to</typeparam>
        /// <returns>The constraint instance</returns>
        public static AssignableToConstraint AssignableTo<TType>()
        {
            return AssignableTo(typeof(TType));
        }

        /// <summary>
        /// Creates a constraint that combines a constraint with other constraints.
        /// </summary>
        /// <param name="constraint">The constraint to combine</param>
        /// <param name="others">The other constraints</param>
        /// <returns>The aggregate constraint</returns>
        public static CombineConstraint Combine(IInspectorConstraint constraint, params IInspectorConstraint[] others)
        {
            return new CombineConstraint(constraint, others);
        }

        public readonly struct InvertConstraint : IInspectorConstraint
        {
            readonly AssignableToConstraint m_IsAssignableTo;

            public InvertConstraint(AssignableToConstraint isAssignableTo)
            {
                m_IsAssignableTo = isAssignableTo;
            }

            public bool Satisfy(Type inspectorType)
            {
                return !m_IsAssignableTo.Satisfy(inspectorType);
            }
        }

        public readonly struct AssignableToConstraint : IInspectorConstraint
        {
            readonly Type m_IsAssignableTo;

            public AssignableToConstraint(Type type)
            {
                m_IsAssignableTo = type;
            }

            public bool Satisfy(Type inspectorType)
            {
                return m_IsAssignableTo.IsAssignableFrom(inspectorType);
            }
        }

        public readonly struct CombineConstraint : IInspectorConstraint
        {
            readonly IInspectorConstraint m_Constraint;
            readonly IInspectorConstraint[] m_Others;

            public CombineConstraint(IInspectorConstraint constraint, params IInspectorConstraint[] others)
            {
                m_Constraint = constraint;
                m_Others = others ?? Array.Empty<IInspectorConstraint>();
            }

            public bool Satisfy(Type inspectorType)
            {
                if (!m_Constraint.Satisfy(inspectorType))
                {
                    return false;
                }

                foreach (var constraint in m_Others)
                {
                    if (!constraint.Satisfy(inspectorType))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    struct SatisfiesConstraintsEnumerator
    {
        List<Type> m_Source;
        int m_Index;
        public Type Current;
        IInspectorConstraint[] m_Constraints;

        public SatisfiesConstraintsEnumerator(List<Type> types, IInspectorConstraint[] constraints)
        {
            m_Source = types;
            m_Index = 0;
            Current = null;
            m_Constraints = constraints;
        }

        public bool MoveNext()
        {
            if (null == m_Source)
                return false;

            while (m_Index < m_Source.Count)
            {
                var inspector = m_Source[m_Index];
                ++m_Index;
                var any = false;
                foreach (var r in m_Constraints)
                {
                    if (r.Satisfy(inspector))
                        continue;

                    any = true;
                    break;
                }

                if (any)
                {
                    continue;
                }

                Current = inspector;
                return true;
            }

            Current = null;
            return false;
        }

        public void Reset()
        {
            m_Index = 0;
            Current = null;
        }
    }
}
