using System;
using NUnit.Framework;
using UnityEditor.Graphs;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    partial class CustomInspectorsTests
    {
        static partial class Types
        {
            public class ContextBase : InspectionContext
            {
            }
            
            public class ContextA : ContextBase
            {
                public override string Name => "A";
            }
            
            public class ContextB : ContextBase
            {
                public override string Name => "B";
            }

            public class ContextTracker
            {
                public bool HasA;
                public bool HasB;
            }

            class ContextTrackerInspector : PropertyInspector<ContextTracker>
            {
                public override VisualElement Build()
                {
                    Target.HasA = HasContext<Types.ContextA>();
                    Target.HasB = HasContext<Types.ContextB>();
                    return DoDefaultGui();
                }
            }
        }
        
        [Test]
        public void CustomInspector_AddingOrRemovingContext_RebuildsTheInspector()
        {
            var value = new Types.ContextTracker();
            Element.SetTarget(value);
            Assert.That(value.HasA, Is.False);
            Assert.That(value.HasB, Is.False);

            var contextA = new Types.ContextA();
            Element.AddContext(contextA);
            Assert.That(value.HasA, Is.True);
            Assert.That(value.HasB, Is.False);
            
            var contextB = new Types.ContextB();
            Element.AddContext(contextB);
            Assert.That(value.HasA, Is.True);
            Assert.That(value.HasB, Is.True);
            
            Element.RemoveContext(contextA);
            Assert.That(value.HasA, Is.False);
            Assert.That(value.HasB, Is.True);
            
            Element.RemoveContext(contextB);
            Assert.That(value.HasA, Is.False);
            Assert.That(value.HasB, Is.False);
        }

        [Test]
        public void CustomInspector_GettingContext_ReturnsCorrectInstance()
        {
            var value = new Types.ContextTracker();
            Element.SetTarget(value);
            Assert.That(Element.GetContext<Types.ContextA>(), Is.Null);
            var contextA = new Types.ContextA();
            Element.AddContext(contextA);
            Assert.That(Element.GetContext<Types.ContextA>(), Is.Not.Null);
            Assert.That(Element.GetContext<Types.ContextA>().Name, Is.EqualTo("A"));
            
            Assert.That(Element.GetContext<Types.ContextB>(), Is.Null);
            var contextB = new Types.ContextB();
            Element.AddContext(contextB);
            Assert.That(Element.GetContext<Types.ContextB>(), Is.Not.Null);
            Assert.That(Element.GetContext<Types.ContextB>().Name, Is.EqualTo("B"));
            
            Element.RemoveContext(contextA);
            Assert.That(Element.GetContext<Types.ContextA>(), Is.Null);
            
            Element.RemoveContext(contextB);
            Assert.That(Element.GetContext<Types.ContextB>(), Is.Null);
        }
        
        [Test]
        public void CustomInspector_AddingOrRemovingContext_IsValidated()
        {
            var value = new Types.ContextTracker();
            Element.SetTarget(value);
            Assert.Throws<NullReferenceException>(() => Element.AddContext(null));
            Assert.Throws<NullReferenceException>(() => Element.RemoveContext(null));
            var contextB = new Types.ContextB();
            Assert.Throws<ArgumentException>(() => Element.RemoveContext(contextB));
        }
    }
}