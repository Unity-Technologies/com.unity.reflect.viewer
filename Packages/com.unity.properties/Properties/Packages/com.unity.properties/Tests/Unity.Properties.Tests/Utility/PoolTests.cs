using NUnit.Framework;
using Unity.Properties.Internal;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Properties.Tests
{
    class PoolTest
    {
        class ValueSemantics
        {
            internal static readonly Pool<ValueSemantics> Pool = new Pool<ValueSemantics>(() => new ValueSemantics(), p => p.Name = string.Empty);

            public string Name;

            public override int GetHashCode()
            {
                return 0;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((ValueSemantics)obj);
            }

            public bool Equals(ValueSemantics other)
            {
                return true;
            }
        }

        [Test]
        public void Pooling_WhenGettingMultipleTimes_ReturnsDifferentInstances()
        {
            var value = ValueSemantics.Pool.Get();
            var value2 = ValueSemantics.Pool.Get();
            
            Assert.That(value, Is.Not.SameAs(value2));
            
            ValueSemantics.Pool.Release(value);   
            ValueSemantics.Pool.Release(value2);   
            LogAssert.NoUnexpectedReceived();
        }
        
        [Test]
        public void PooledInstance_WhenReturned_CanBeReused()
        {
            var value = ValueSemantics.Pool.Get();
            ValueSemantics.Pool.Release(value);
            var value2 = ValueSemantics.Pool.Get();
            Assert.That(value, Is.SameAs(value2));
            ValueSemantics.Pool.Release(value2);   
            LogAssert.NoUnexpectedReceived();
        }
        
        [Test]
        public void PooledInstance_WhenReleasedMultipleTimes_LogsAnError()
        {
            var value = ValueSemantics.Pool.Get();
            value.Name = "Harry";
            
            ValueSemantics.Pool.Release(value);   
            ValueSemantics.Pool.Release(value);   
            LogAssert.Expect(LogType.Error, Pool<ValueSemantics>.ErrorString);
        }
        
        [Test]
        public void TypesWithValueSemantics_WhenPooled_UsesReferences()
        {
            var value = ValueSemantics.Pool.Get();
            value.Name = "Harry";
            var value2 = ValueSemantics.Pool.Get();
            value2.Name = "Harry";

            ValueSemantics.Pool.Release(value2);   
            ValueSemantics.Pool.Release(value);   
            LogAssert.NoUnexpectedReceived();
        }
    }
}
