using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    class TestPathVisitor : Internal.PathVisitor
    {
        public TestPathVisitor(PropertyPath path)
        {
            Path = path;
        }

        protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            
        }
    }
    
    [TestFixture]
    class PathVisitorTests : PropertiesTestFixture
    {
        [Test]
        public void PathVisitor_VisitArrayElement_ReturnVisitErrorCodeOk()
        {
            var container = new ClassWithLists
            {
                Int32List = new List<int> {1, 2, 3}
            };

            var visitor = new TestPathVisitor(new PropertyPath($"{nameof(ClassWithLists.Int32List)}[1]"));
            
            PropertyContainer.Accept(visitor, ref container);
            
            Assert.That(visitor.ErrorCode, Is.EqualTo(VisitErrorCode.Ok));
        }
        
        [Test]
        public void PathVisitor_VisitNestedContainer_ReturnVisitErrorCodeOk()
        {
            var container = new StructWithNestedStruct
            {
                Container = new StructWithPrimitives()
            };

            var visitor = new TestPathVisitor(new PropertyPath($"{nameof(StructWithNestedStruct.Container)}.{nameof(StructWithPrimitives.Float64Value)}"));
            
            PropertyContainer.Accept(visitor, ref container);
            
            Assert.That(visitor.ErrorCode, Is.EqualTo(VisitErrorCode.Ok));
        }

        [Test]
        public void PathVisitor_NullValuesOnPath_ReturnsInvalidPath()
        {
            var container = new ClassWithPolymorphicFields();

            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue")), Is.True);
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.ObjectValue")), Is.False);
            container.ObjectValue = new ClassWithPolymorphicFields();
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.ObjectValue")), Is.True);
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.ObjectValue.ObjectValue")), Is.False);
        }
    }
}