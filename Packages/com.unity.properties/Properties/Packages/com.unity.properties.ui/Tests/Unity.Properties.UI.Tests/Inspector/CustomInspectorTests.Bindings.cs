using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    partial class CustomInspectorsTests : WindowTestsFixtureBase
    {
        public class ComplicatedType
        {
            public float A;
            public float B;
            public NestedType Nested = new NestedType();

            public override string ToString()
            {
                return "Heyyyyy";
            }
        }

        public class NestedType
        {
            public float A;
            public float B;
            public SubNestedType Nested = new SubNestedType();
        }

        public class SubNestedType
        {
            public float A;
            public float B;
            public float Nested;
        }

        class ComplicatedTypeInspector : PropertyInspector<ComplicatedType>
        {
            public override VisualElement Build()
            {
                var root = new VisualElement();
                new Internal.UITemplate("complicated-type-test").Clone(root);
                {
                    root.Add(new ComplicatedTypeElement {name = ".", bindingPath = "."});
                    root.Add(new FloatField {name = "A", bindingPath = "A"});
                    root.Add(new FloatField {name = "B", bindingPath = "B"});
                    root.Add(new FloatField {name = "Nested.A", bindingPath = "Nested.A"});
                    root.Add(new FloatField {name = "Nested.B", bindingPath = "Nested.B"});
                    root.Add(new FloatField {name = "Nested.Nested.A", bindingPath = "Nested.Nested.A"});
                    root.Add(new FloatField {name = "Nested.Nested.B", bindingPath = "Nested.Nested.B"});
                    root.Add(new FloatField {name = "Nested.Nested.Nested", bindingPath = "Nested.Nested.Nested"});
                }

                {
                    var firstLevel = new Foldout {bindingPath = "Nested"};
                    firstLevel.Add(new FloatField {name = "Nested.A", bindingPath = "A"});
                    firstLevel.Add(new FloatField {name = "Nested.B", bindingPath = "B"});
                    firstLevel.Add(new FloatField {name = "Nested.Nested.A", bindingPath = "Nested.A"});
                    firstLevel.Add(new FloatField {name = "Nested.Nested.B", bindingPath = "Nested.B"});
                    firstLevel.Add(new FloatField {name = "Nested.Nested.Nested", bindingPath = "Nested.Nested"});
                    root.Add(firstLevel);
                }

                {
                    var firstLevel = new Foldout {bindingPath = "Nested"};
                    var secondLevel = new Foldout {bindingPath = "Nested"};
                    secondLevel.Add(new FloatField {name = "Nested.Nested.A", bindingPath = "A"});
                    secondLevel.Add(new FloatField {name = "Nested.Nested.B", bindingPath = "B"});
                    secondLevel.Add(new FloatField {name = "Nested.Nested.Nested", bindingPath = "Nested"});
                    secondLevel.Add(new ComplicatedTypeElement {name = ".", bindingPath = "."});
                    firstLevel.Add(secondLevel);
                    root.Add(firstLevel);
                }

                {
                    var firstLevel = new Foldout {bindingPath = "Nested.Nested"};
                    firstLevel.Add(new FloatField {name = "Nested.Nested.A", bindingPath = "A"});
                    firstLevel.Add(new FloatField {name = "Nested.Nested.B", bindingPath = "B"});
                    firstLevel.Add(new FloatField {name = "Nested.Nested.Nested", bindingPath = "Nested"});
                    root.Add(firstLevel);
                }

                return root;
            }
        }

        class ComplicatedTypeElement : BaseField<ComplicatedType>
        {
            public ComplicatedTypeElement() : base(string.Empty, null)
            {
            }

            public ComplicatedTypeElement(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        [TestCase("A")]
        [TestCase("B")]
        [TestCase("Nested.A")]
        [TestCase("Nested.B")]
        [TestCase("Nested.Nested.A")]
        [TestCase("Nested.Nested.B")]
        [TestCase("Nested.Nested.Nested")]
        public void Bindings_WithAbsoluteAndRelativePaths_UpdatesValuesCorrectly(string path)
        {
            var value = new ComplicatedType
            {
                A = 0, B = 1,
                Nested = new NestedType {A = 2, B = 3, Nested = new SubNestedType {A = 4, B = 5, Nested = 6}}
            }; 
            Element.SetTarget(value);
            var values = new float[] {1, 2, 3, 4, 5};
            var elements = Element.Query<FloatField>(path).ToList();
            foreach (var element in elements)
                Assert.That(element.value,
                    Is.EqualTo(PropertyContainer.GetValue<ComplicatedType, float>(ref value, path)));

            foreach (var element in elements)
            foreach (var v in values)
            {
                element.value = v;
                UpdateBindables();
                Assert.That(PropertyContainer.GetValue<ComplicatedType, float>(ref value, path), Is.EqualTo(v));
            }

            foreach (var v in values)
            {
                PropertyContainer.SetValue(ref value, path, v);
                UpdateBindables();

                foreach (var element in elements)
                {
                    Assert.That(PropertyContainer.GetValue<ComplicatedType, float>(ref value, path),
                        Is.EqualTo(element.value));
                }
            }
        }

        [Test]
        public void BindingCurrentValue_UsingADot_WorksOnEveryBindingPath()
        {
            var value = new ComplicatedType();
            Element.SetTarget(value);
            Assert.That(Element.Q<ComplicatedTypeElement>(".").value, Is.EqualTo(value));
            Assert.That(Element.Q<Label>(".").text, Is.EqualTo(value.ToString()));
        }
    }
}