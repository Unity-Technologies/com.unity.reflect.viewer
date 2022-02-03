#if !NET_DOTS
using System;
using Unity.Serialization.Binary.Adapters;

namespace Unity.Serialization.Binary
{
    abstract class BinaryPropertyVisitor : BinaryAdapter
    {
        protected const byte k_TokenNone = 0;
        protected const byte k_TokenNull = 1;
        protected const byte k_TokenPolymorphic = 2;
        protected const byte k_TokenUnityEngineObjectReference = 3;
        protected const byte k_TokenSerializedReference = 4;
        
        /// <summary>
        /// Scope used to lock the current visitor as being in use.
        /// </summary>
        internal readonly struct LockScope : IDisposable
        {
            readonly BinaryPropertyVisitor m_Visitor;

            /// <summary>
            /// Initializes a new instance of <see cref="LockScope"/>.
            /// </summary>
            /// <param name="visitor">The current visitor.</param>
            public LockScope(BinaryPropertyVisitor visitor)
            {
                m_Visitor = visitor;
                visitor.IsLocked = true;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                m_Visitor.IsLocked = false;
            }
        }
        
        /// <summary>
        /// Returns true if the reader is currently in use.
        /// </summary>
        internal bool IsLocked { get; private set; }

        /// <summary>
        /// Creates a lock scope for the visitation.
        /// </summary>
        /// <returns>A disposable scope.</returns>
        internal LockScope Lock()
            => new LockScope(this);
    }
}
#endif