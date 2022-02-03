using NUnit.Framework;

namespace Unity.Serialization.Editor.Tests
{
    public class MockTestScript
    {
        /// <summary>
        /// This test exists to make sure we have at least one Editor test for CI.
        /// </summary>
        [Test]
        public void ScriptSimplePasses()
        {
            Assert.IsTrue(true);
        }
    }
}