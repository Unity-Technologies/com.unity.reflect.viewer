#pragma warning disable 649
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.Properties.UI.Internal;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    class CustomInspectorTests
    {
        static class Types
        {
            public class InspectorTestBase
            {
                public float Float;
                public int Int;
                public string String;
            }

            public class NoInspectorType : InspectorTestBase
            {
            }

            public class NullInspectorType : InspectorTestBase
            {
            }

            public class NullInspectorTypeInspector : PropertyInspector<NullInspectorType>
            {
                public override VisualElement Build()
                {
                    return null;
                }
            }

            public class DefaultInspectorType : InspectorTestBase
            {
            }

            public class DefaultInspectorTypeInspector : PropertyInspector<DefaultInspectorType>
            {
                public override VisualElement Build()
                {
                    var sneaky = new VisualElement();
                    sneaky.Add(DoDefaultGui());
                    return sneaky;
                }
            }

            public class CodeInspectorType : InspectorTestBase
            {

            }

            public class CodeInspectorTypeInspector : PropertyInspector<CodeInspectorType>
            {
                FloatField Float;
                IntegerField Int;
                TextField String;

                public override VisualElement Build()
                {
                    var root = new VisualElement();
                    root.Add(Float = new FloatField
                    {
                        bindingPath = "Float"
                    });

                    root.Add(Int = new IntegerField
                    {
                        bindingPath = "Int"
                    });

                    root.Add(String = new TextField
                    {
                        bindingPath = "String"
                    });
                    return root;
                }
            }

            public class AllDefaultInspectorType : InspectorTestBase
            {
                }

            public class AllDefaultInspectorTypeInspector : PropertyInspector<AllDefaultInspectorType>
            {
                public override VisualElement Build()
                {
                    var root = new VisualElement();
                    DoDefaultGui(root, "Float");
                    DoDefaultGui(root, "Int");
                    DoDefaultGui(root, "String");
                    return root;
                }
            }

            public class NullFieldInspector
            {
                public NullInspectorType NullInspectorType;
            }

            public class DefaultFieldInspector
            {
                public NoInspectorType NoInspectorType;
                public DefaultInspectorType DefaultInspectorType;
            }

            public enum MyEnumCustom
            {
                Zero, One
            }

            class MyEnumCustomInspector : PropertyInspector<MyEnumCustom>
            {
                public override VisualElement Build()
                {
                    return new Label(Target.ToString()){ name = Name };
                }
            }
            
            public enum MyEnumDefault
            {
                Zero, One
            }

            class MyEnumDefaultInspector : PropertyInspector<MyEnumDefault>
            {
            }
            
            public enum MyEnumNull
            {
                Zero, One
            }

            class MyEnumNullInspector : PropertyInspector<MyEnumNull>
            {
                public override VisualElement Build()
                {
                    return null;
                }
            }

            public class TypeWithEnum
            {
                public MyEnumCustom Custom;
                public MyEnumDefault Default;
                public MyEnumNull Null;
            }
            
            public class TypeWithCollections
            {
                public List<MyEnumCustom> Custom = new List<MyEnumCustom>();
                public List<MyEnumDefault> Default = new List<MyEnumDefault>();
                public List<MyEnumNull> Null = new List<MyEnumNull>();
            }
            
            class MyEnumCustomListInspector : PropertyInspector<List<MyEnumCustom>>
            {
                public override VisualElement Build()
                {
                    return new Label(Target.ToString()){ name = Name };
                }
            }
    
            class MyEnumDefaultListInspector : PropertyInspector<List<MyEnumDefault>>
            {
                public override VisualElement Build()
                {
                    return DoDefaultGui();
                }
            }
    
            class MyEnumNullListInspector : PropertyInspector<List<MyEnumNull>>
            {
                public override VisualElement Build()
                {
                    return null;
                }
            }
        }

        [Test]
        public void NullInspector_ForRootType_HasNoChildren()
        {
            var noOverrideInspector = new PropertyElement();
            noOverrideInspector.SetTarget(new Types.NullInspectorType());
            var customInspectorElements = noOverrideInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0]; 
            Assert.That(customInspectorElement.childCount, Is.EqualTo(0));
        }
        
        [Test]
        public void NullInspector_ForField_HasNoChildren()
        {
            var fieldInspector = new PropertyElement();
            fieldInspector.SetTarget(new Types.NullFieldInspector
            {
                NullInspectorType = new Types.NullInspectorType()
            });
           
            var customInspectorElements = fieldInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0];
            Assert.That(customInspectorElement.childCount, Is.EqualTo(0));
        }
        
        [Test]
        public void DefaultInspector_ForRootType_MimicsGenericInspector()
        {
            var noInspector = new PropertyElement();
            noInspector.SetTarget(new Types.NoInspectorType());
            Assert.That(noInspector.Query<CustomInspectorElement>().ToList().Count, Is.EqualTo(0));
            Assert.That(noInspector.childCount, Is.EqualTo(3));
            
            var defaultInspector = new PropertyElement();
            defaultInspector.SetTarget(new Types.DefaultInspectorType());
            var customInspectorElements = defaultInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0].Q<CustomInspectorElement.DefaultInspectorElement>(); 
            Assert.That(customInspectorElement.childCount, Is.EqualTo(3));

            for (var i = 0; i < 3; ++i)
            {
                var lhs = noInspector.ElementAt(i);
                var rhs = customInspectorElement.ElementAt(i);
                Assert.That(lhs.GetType(), Is.EqualTo(rhs.GetType()));
                Assert.That(lhs.childCount, Is.EqualTo(rhs.childCount));
                Assert.That((lhs as BindableElement)?.bindingPath, Is.EqualTo((rhs as BindableElement)?.bindingPath));
            }
        }
        
        [Test]
        public void DefaultInspector_ForField_MimicsGenericInspector()
        {
            var fieldInspector = new PropertyElement();
            fieldInspector.SetTarget(new Types.DefaultFieldInspector()
            {
                NoInspectorType = new Types.NoInspectorType(),
                DefaultInspectorType = new Types.DefaultInspectorType()
            });

            var noInspector =
                fieldInspector.Q<Foldout>(nameof(Types.DefaultFieldInspector.NoInspectorType));
            Assert.That(noInspector.Query<CustomInspectorElement>().ToList().Count, Is.EqualTo(0));
            Assert.That(noInspector.childCount, Is.EqualTo(3));
            
            var customInspectorElements = fieldInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0].Q<Foldout>(nameof(Types.DefaultFieldInspector.DefaultInspectorType));
            Assert.That(customInspectorElement.childCount, Is.EqualTo(3));

            for (var i = 0; i < 3; ++i)
            {
                var lhs = noInspector.ElementAt(i);
                var rhs = customInspectorElement.ElementAt(i);
                Assert.That(lhs.GetType(), Is.EqualTo(rhs.GetType()));
                Assert.That(lhs.childCount, Is.EqualTo(rhs.childCount));
                Assert.That((lhs as BindableElement)?.bindingPath, Is.EqualTo((rhs as BindableElement)?.bindingPath));
            }
        }
        
        [Test]
        public void DefaultInspector_ForEnumField_MimicsGenericInspector()
        {
            var fieldInspector = new PropertyElement();
            fieldInspector.SetTarget(new Types.TypeWithEnum());

            var customInspectorElements = fieldInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements[0].childCount, Is.EqualTo(1));
            Assert.That(customInspectorElements[0][0], Is.TypeOf<Label>());
            Assert.That(customInspectorElements[1].childCount, Is.EqualTo(1));
            Assert.That(customInspectorElements[1][0], Is.TypeOf<CustomInspectorElement.DefaultInspectorElement>());
            Assert.That(customInspectorElements[2].childCount, Is.EqualTo(0));
        }
        
        [Test]
        public void DefaultInspector_ForCollectionField_MimicsGenericInspector()
        {
            var fieldInspector = new PropertyElement();
            fieldInspector.SetTarget(new Types.TypeWithCollections());

            var customInspectorElements = fieldInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements[0].childCount, Is.EqualTo(1));
            Assert.That(customInspectorElements[0][0], Is.TypeOf<Label>());
            Assert.That(customInspectorElements[1].childCount, Is.EqualTo(1));
            Assert.That(customInspectorElements[1][0], Is.TypeOf<CustomInspectorElement.DefaultInspectorElement>());
            Assert.That(customInspectorElements[2].childCount, Is.EqualTo(0));
        }
        
        [Test]
        public void CustomInspector_CallingDefaultOnEachField_MimicsGenericInspector()
        {
            var noInspector = new PropertyElement();
            noInspector.SetTarget(new Types.NoInspectorType());
            Assert.That(noInspector.Query<CustomInspectorElement>().ToList().Count, Is.EqualTo(0));
            Assert.That(noInspector.childCount, Is.EqualTo(3));
            
            var allDefaultInspector = new PropertyElement();
            allDefaultInspector.SetTarget(new Types.AllDefaultInspectorType());
            var customInspectorElements = allDefaultInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0].ElementAt(0); 
            Assert.That(customInspectorElement.childCount, Is.EqualTo(3));

            for (var i = 0; i < 3; ++i)
            {
                var lhs = noInspector.ElementAt(i);
                var rhs = customInspectorElement.ElementAt(i);
                Assert.That(lhs.GetType(), Is.EqualTo(rhs.GetType()));
                Assert.That(lhs.childCount, Is.EqualTo(rhs.childCount));
                Assert.That((lhs as BindableElement)?.bindingPath, Is.EqualTo((rhs as BindableElement)?.bindingPath));
            }
        }

        [Test]
        public void CustomInspector_WithBindings_GetsValuesSet()
        {
            var fieldInspector = new PropertyElement();
            fieldInspector.SetTarget(new Types.CodeInspectorType()
            {
                Float = 25.0f,
                Int = 15,
                String = "Yup"
            });
           
            var customInspectorElements = fieldInspector.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(1));
            var customInspectorElement = customInspectorElements[0].ElementAt(0);
            Assert.That(customInspectorElement.childCount, Is.EqualTo(3));
            var floatField = customInspectorElement.ElementAt(0) as FloatField;
            Assert.That(floatField.value, Is.EqualTo(25.0f));
            var intField = customInspectorElement.ElementAt(1) as IntegerField;
            Assert.That(intField.value, Is.EqualTo(15));
            var stringField = customInspectorElement.ElementAt(2) as TextField;
            Assert.That(stringField.value, Is.EqualTo("Yup"));
        }
    }
}
#pragma warning restore 649