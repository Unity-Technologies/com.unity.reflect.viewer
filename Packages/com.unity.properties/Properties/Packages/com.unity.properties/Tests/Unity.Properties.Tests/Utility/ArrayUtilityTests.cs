using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class ArrayUtilityTests
    {
        [Test]
        public void Array_WhenRemovingAtIndex_PreservesOtherItems()
        {
            var source = new [] {"1", "2", "3", "4", "5"};
            var indexToRemove = 0;
            {
                var expected = new [] {"2", "3", "4", "5"};
                var result = ArrayUtility.RemoveAt(source, indexToRemove);
                Assert.That(result, Is.EquivalentTo(expected));
            }
            indexToRemove = 1;
            {
                var expected = new [] {"1", "3", "4", "5"};
                var result = ArrayUtility.RemoveAt(source, indexToRemove);
                Assert.That(result, Is.EquivalentTo(expected));
            }
            indexToRemove = 4;
            {
                var expected = new [] {"1", "2", "3", "4"};
                var result = ArrayUtility.RemoveAt(source, indexToRemove);
                Assert.That(result, Is.EquivalentTo(expected));
            }
        }
        
        [Test]
        public void Array_WhenInsertingAtIndex_PreservesOtherItems()
        {
            var expected = new[] {"1", "2", "3", "4", "5"};
            var indexToInsert = 0;
            var value = "1";

            {
                var source = new[] {"2", "3", "4", "5"};
                var result = ArrayUtility.InsertAt(source, indexToInsert, value);
                Assert.That(result, Is.EquivalentTo(expected));
            }

            indexToInsert = 1;
            value = "2";
            {
                var source = new[] {"1", "3", "4", "5"};
                var result = ArrayUtility.InsertAt(source, indexToInsert, value);
                Assert.That(result, Is.EquivalentTo(expected));
            }
            
            indexToInsert = 4;
            value = "5";
            {
                var source = new[] {"1", "2", "3", "4"};
                var result = ArrayUtility.InsertAt(source, indexToInsert, value);
                Assert.That(result, Is.EquivalentTo(expected));
            }
        }
    }
}
