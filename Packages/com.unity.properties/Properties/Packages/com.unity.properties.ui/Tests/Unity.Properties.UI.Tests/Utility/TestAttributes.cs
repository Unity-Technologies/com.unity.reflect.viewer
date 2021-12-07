using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Unity.Properties.UI.Tests
{
    class TestRequires_QUICKSEARCH_2_1_0_OR_NEWER : Attribute, ITestAction
    {
        public ActionTargets Targets { get; }
    
        readonly string m_Reason;
    
        public TestRequires_QUICKSEARCH_2_1_0_OR_NEWER(string reason)
        {
            m_Reason = reason;
        }
    
        public void BeforeTest(ITest test)
        {
#if !QUICKSEARCH_2_1_0_OR_NEWER
            Assert.Ignore($"Test requires Define=[QUICKSEARCH_2_1_0_OR_NEWER] Reason=[{m_Reason}]");
#endif
        }

        public void AfterTest(ITest test)
        {
        
        }
    }
}