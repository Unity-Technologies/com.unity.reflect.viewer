using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace UnityEngine.Reflect.Viewer.Core
{

    public interface IDeltaDNADataProvider
    {
        public DNALicenseInfo dnaLicenseInfo{ get; set; }
        public string buttonName{ get; set; }
    }
    public struct DNALicenseInfo
    {
        public TimeSpan floatingSeat { get; set; }
        public List<string> entitlements { get; set; }
    }

    public class DeltaDNAContext : ContextBase<DeltaDNAContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IDeltaDNADataProvider) };
    }
}
