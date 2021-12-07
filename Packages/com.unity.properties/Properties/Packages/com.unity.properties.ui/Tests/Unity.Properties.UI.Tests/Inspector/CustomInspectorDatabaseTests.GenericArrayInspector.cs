using NUnit.Framework;
using Unity.Properties.UI.Internal;

namespace Unity.Properties.UI.Tests
{
    [UI]
    partial class CustomInspectorDatabaseTests
    {
        public class SomeArray{}
        public class SomeArrayInspector : PropertyInspector<SomeArray[]>{}
        
        public class SomeOtherArray {}
        public class GenericArrayInspector<T> : PropertyInspector<T[]>, IExperimentalInspector {}
        
        [Test]
        public void CustomInspectors_ForArrayTypes_AreSupported()
        {
            AssertInspectorMatchesForType<SomeArray[], SomeArrayInspector>();
        }

        [Test]
        public void GenericCustomInspectors_ForArrayTypes_AreNotSupported()
        {
            AssertNoInspectorMatchesForType<SomeOtherArray[]>();
            // TODO: Uncomment this when support for generic array inspector is added
            // AssertInspectorMatchesForType<SomeOtherArray[], GenericArrayInspector<SomeOtherArray>>();
        }
    }
}