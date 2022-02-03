using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    partial class PropertyElementTests
    {
        class NestedPropertyElementWithInterfaces 
        {
            interface ICanBeNested
            {
            }

            public class Nestable : ICanBeNested
            {
            }
    
            public class NestableInspector : PropertyInspector<Nestable>
            {
                public override void Update()
                {
                    Target = new Nestable();
                }
            }
            
            [CreateProperty] ICanBeNested m_Nested = new Nestable();

            class DataInspector : PropertyInspector<NestedPropertyElementWithInterfaces>
            {
                public override VisualElement Build()
                {
                    return new PropertyElement {bindingPath = nameof(m_Nested)};
                }

                public override void Update()
                {
                    Target = new NestedPropertyElementWithInterfaces();
                }
            }
        }

        [Test]
        public void ResettingTargets_FromCustomInspector_ShouldNotThrow()
        {
            var value = new NestedPropertyElementWithInterfaces();
            var element = new PropertyElement();
            Assert.DoesNotThrow(() => element.SetTarget(value));
            Assert.DoesNotThrow(() => element.Query<CustomInspectorElement>().ForEach(i=> (i as IBinding).Update()));
        }
    }
}