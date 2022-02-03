using System;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Example
{
    [Serializable, GeneratePropertyBag]
    struct ExampleStateData: IStateFlagData, IStateTextData
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool stateFlag
        {
            get;
            set;
        }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string stateText
        {
            get;
            set;
        }
    }
}
