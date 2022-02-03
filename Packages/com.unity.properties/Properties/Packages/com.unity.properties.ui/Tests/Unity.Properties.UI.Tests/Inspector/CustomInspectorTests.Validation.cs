using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    partial class CustomInspectorsTests
    {
        static partial class Types
        {
            public struct InspectedType{}
            public class InspectedTypeInspector : PropertyInspector<InspectedType> {}
            
            public class ThrowsOnBuild{}
            public class ThrowsOnBuildInspector : PropertyInspector<ThrowsOnBuild>
            {
                public override VisualElement Build()
                {
                    throw new InvalidOperationException("Woups");
                }
            }
            
            public class ThrowsOnUpdate{}
            public class ThrowsOnUpdateInspector : PropertyInspector<ThrowsOnUpdate>
            {
                public override void Update()
                {
                    throw new InvalidOperationException("Nope");
                }
            }
        }
        
        [Test]
        public void CustomInspectors_WithoutProperContext_CanBeDetected()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new Types.InspectedTypeInspector().Part);
            Assert.Throws<InvalidOperationException>(() => _ = new Types.InspectedTypeInspector().Type);
            Assert.Throws<InvalidOperationException>(() => _ = new Types.InspectedTypeInspector().PropertyPath);
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().Build());
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().DoDefaultGui(null, ""));
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().DoDefaultGuiAtIndex(null, 1));
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().DoDefaultGuiAtKey(null, 1));
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().GetContext<InspectionContext>());
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().GetAttribute<Attribute>());
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().GetAttributes<Attribute>().Count());
            Assert.Throws<InvalidOperationException>(() => new Types.InspectedTypeInspector().HasAttribute<Attribute>());
        }

        [Test]
        public void CustomInspectors_UserExceptions_AreLoggedToTheConsole()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() => Element.SetTarget(new Types.ThrowsOnBuild()));
            Assert.DoesNotThrow(() => Element.SetTarget(new Types.ThrowsOnUpdate()));
            LogAssert.ignoreFailingMessages = false;
        }
    }
}