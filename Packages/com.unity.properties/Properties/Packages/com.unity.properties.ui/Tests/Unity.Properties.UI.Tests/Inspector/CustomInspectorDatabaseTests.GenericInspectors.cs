using System;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [UI]
    partial class CustomInspectorDatabaseTests
    {
        interface IGenericType<T> {}
        class GenericType<T> : IGenericType<T> { }
        class IGenericInspector<T> : PropertyInspector<IGenericType<T>>, IExperimentalInspector { }
        class GenericInspector<T> : PropertyInspector<GenericType<T>>, IExperimentalInspector { }
        class IGenericIntInspector : PropertyInspector<IGenericType<int>>, IExperimentalInspector { }
        class GenericIntInspector : PropertyInspector<GenericType<int>>, IExperimentalInspector { }
        class TooManyArgumentsInspector<T1, T2> : PropertyInspector<GenericType<T1>>, IExperimentalInspector { }
        class TooManyArgumentsInspector<T> : PropertyInspector<GenericType<int>>, IExperimentalInspector { }
        
        interface IGenericType<T1, T2> {}
        class GenericType<T1, T2> : IGenericType<T1, T2> { }
        class IGenericInspector<T1, T2> : PropertyInspector<IGenericType<T1, T2>>, IExperimentalInspector { }
        class GenericInspector<T1, T2> : PropertyInspector<GenericType<T1, T2>>, IExperimentalInspector { }
        class IMyIdentityGenericInspector<T> : PropertyInspector<IGenericType<T, T>>, IExperimentalInspector { }
        class MyIdentityGenericInspector<T> : PropertyInspector<GenericType<T, T>>, IExperimentalInspector { }
        class IGenericStringStringInspector : PropertyInspector<IGenericType<string, string>>, IExperimentalInspector { }
        class GenericStringStringInspector : PropertyInspector<GenericType<string, string>>, IExperimentalInspector { }
        class IPartialDerivedGenericInspector<T> : IGenericInspector<int, T>, IExperimentalInspector { }
        class PartialDerivedGenericInspector<T> : GenericInspector<int, T>, IExperimentalInspector { }
        class IResolvedDerivedGenericInspector : IPartialDerivedGenericInspector<Vector2>, IExperimentalInspector { }
        class ResolvedDerivedGenericInspector : PartialDerivedGenericInspector<Vector2>, IExperimentalInspector { }

        interface IDeviousGenericType<T1, T2> { }
        class DeviousGenericType<T1, T2> : IDeviousGenericType<T1, T2> { }
        class IDeviousGenericInspector<T1, T2> : PropertyInspector<IDeviousGenericType<T2, T1>>, IExperimentalInspector { }
        class DeviousGenericInspector<T1, T2> : PropertyInspector<DeviousGenericType<T2, T1>>, IExperimentalInspector { }
        
        class UnresolvableGeneric<T1, T2> {}
        class UnresolvableGenericInspector<T> : PropertyInspector<UnresolvableGeneric<T, int>>, IExperimentalInspector { }
        class UnresolvableGenericInspector2<T> : PropertyInspector<UnresolvableGeneric<int, T>>, IExperimentalInspector { }

        class ExperimentalGeneric<T1, T2> { }
        class ExperimentalGenericInspector<T1, T2> : PropertyInspector<ExperimentalGeneric<T1, T2>> { }
        class ExperimentalGenericInspector<T> : PropertyInspector<ExperimentalGeneric<T, T>>, IExperimentalInspector { }

        class DefaultGenericInspector<T> : PropertyInspector<T>, IExperimentalInspector
        {
            public override VisualElement Build()
            {
                throw new InvalidOperationException();
            }
        }

        [Test]
        public void CustomInspector_ForGenericTypes_IsSupported()
        {
            // Fully resolved inspectors for generic types
            AssertInspectorMatchesForType<GenericType<int>, GenericIntInspector>();
            AssertInspectorMatchesForType<GenericType<string, string>, GenericStringStringInspector>();
            AssertInspectorMatchesForType<GenericType<int, Vector2>, ResolvedDerivedGenericInspector>();
            
            // Generic inspectors for generic types (arguments match in given order)
            AssertInspectorMatchesForType<GenericType<float>, GenericInspector<float>>();
            AssertInspectorMatchesForType<GenericType<string, float>, GenericInspector<string, float>>();
            AssertInspectorMatchesForType<GenericType<int, float>, GenericInspector<int, float>>();
            AssertInspectorMatchesForType<GenericType<float, int>, GenericInspector<float, int>>();
            
            // Specialized generic inspectors for generic types (reusing the same arguments)
            AssertInspectorMatchesForType<GenericType<Vector2, Vector2>, MyIdentityGenericInspector<Vector2>>();
            AssertInspectorMatchesForType<GenericType<float, float>, MyIdentityGenericInspector<float>>();
            
            // Strange generic inspectors for generic types (arguments were provided in whatever order)
            AssertInspectorMatchesForType<DeviousGenericType<string, float>, DeviousGenericInspector<float, string>>();
        }

        [Test]
        public void CustomInspector_ForGenericInterfaces_IsSupported()
        {
            // Fully resolved inspectors for generic types
            AssertInspectorMatchesForType<IGenericType<int>, IGenericIntInspector>();
            AssertInspectorMatchesForType<IGenericType<string, string>, IGenericStringStringInspector>();
            AssertInspectorMatchesForType<IGenericType<int, Vector2>, IResolvedDerivedGenericInspector>();
            
            // Generic inspectors for generic types (arguments match in given order)
            AssertInspectorMatchesForType<IGenericType<float>, IGenericInspector<float>>();
            AssertInspectorMatchesForType<IGenericType<string, float>, IGenericInspector<string, float>>();
            AssertInspectorMatchesForType<IGenericType<int, float>, IGenericInspector<int, float>>();
            AssertInspectorMatchesForType<IGenericType<float, int>, IGenericInspector<float, int>>();
            
            // Specialized generic inspectors for generic types (reusing the same arguments)
            AssertInspectorMatchesForType<IGenericType<Vector2, Vector2>, IMyIdentityGenericInspector<Vector2>>();
            AssertInspectorMatchesForType<IGenericType<float, float>, IMyIdentityGenericInspector<float>>();
            
            // Strange generic inspectors for generic types (arguments were provided in whatever order)
            AssertInspectorMatchesForType<IDeviousGenericType<string, float>, IDeviousGenericInspector<float, string>>();
        }

        [Test]
        public void PartiallyResolvedCustomInspector_ForGenericTypes_IsNotSupported()
        {
            AssertNoInspectorMatchesForType<UnresolvableGeneric<float, int>>();
            AssertNoInspectorMatchesForType<UnresolvableGeneric<int, float>>();
        }
        
        [Test]
        public void CustomInspector_ForGenericTypes_RequiresExperimentalInterface()
        {
            AssertNoInspectorMatchesForType<ExperimentalGeneric<float, int>>();
            AssertInspectorMatchesForType<ExperimentalGeneric<float, float>, ExperimentalGenericInspector<float>>();
        }

        [Test]
        public void UnsupportedCustomInspector_ForGenericTypes_AreDetected()
        {
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(TooManyArgumentsInspector<,>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.GenericArgumentsDoNotMatchInspectedType));
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(TooManyArgumentsInspector<>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(UnresolvableGenericInspector<>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(UnresolvableGenericInspector2<>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(ExperimentalGenericInspector<,>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.UnsupportedUserDefinedGenericInspector));
            // Assert.That(InspectorRegistry.GetRegistrationStatusForInspectorType(typeof(DefaultGenericInspector<>)), Is.EqualTo(InspectorRegistry.RegistrationStatus.UnsupportedGenericInspectorForNonGenericType));
        }
    }
}