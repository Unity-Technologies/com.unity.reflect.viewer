using System;
using System.Collections.Generic;
using Unity.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEngine.Reflect.Viewer.Example
{
    [UIStoreContextProperties(typeof(IStateFlagData), typeof(ExampleContext))]
    [UIStoreContextProperties(typeof(IStateTextData), typeof(ExampleContext))]
    class ExampleStoreContainer : UnityStoreContainer<ExampleStateData, ExampleContext>
    {
    }

    class ExampleContext : ContextBase<ExampleContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> { typeof(IStateFlagData), typeof(IStateTextData) };
    }
}
