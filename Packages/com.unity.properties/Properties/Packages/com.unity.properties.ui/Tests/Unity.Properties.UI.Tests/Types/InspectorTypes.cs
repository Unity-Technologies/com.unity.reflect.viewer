using UnityEngine;

namespace Unity.Properties.UI.Tests
{
    public class DrawerAttribute : UnityEngine.PropertyAttribute {}
    
    public interface IUserInspectorTag {}
    public interface IAnotherUserInspectorTag {}
    
    public class NoInspectorType {}
    
    public class SingleInspectorType {}
    public class SingleInspectorTypeInspector : PropertyInspector<SingleInspectorType> {}
    
    public class MultipleInspectorsType {}
    public class MultipleInspectorsTypeInspector : PropertyInspector<MultipleInspectorsType> {}
    public class MultipleInspectorsTypeInspectorWithTag : PropertyInspector<MultipleInspectorsType>, IUserInspectorTag {}
    

    public class NoInspectorButDrawerType {}
    public class NoInspectorButDrawerTypeDrawer : PropertyInspector<NoInspectorButDrawerType, DrawerAttribute> {}
    
    public class InspectorAndDrawerType {}
    public class InspectorAndDrawerTypeInspector : PropertyInspector<InspectorAndDrawerType> {}
    
    public class InspectorAndDrawerTypeTypeDrawer : PropertyInspector<InspectorAndDrawerType, DrawerAttribute>, IAnotherUserInspectorTag {}
    
    public class InspectorAndDrawerTypeTypeDrawerWithTag : PropertyInspector<InspectorAndDrawerType, DrawerAttribute>, IUserInspectorTag {}
}