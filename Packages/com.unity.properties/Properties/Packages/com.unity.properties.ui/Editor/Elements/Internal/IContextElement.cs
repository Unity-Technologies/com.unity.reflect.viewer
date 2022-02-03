namespace Unity.Properties.UI.Internal
{
    interface IContextElement
    {
        PropertyPath Path { get; }
        
        void SetContext(BindingContextElement root, PropertyPath path);
        void OnContextReady();
    }
}