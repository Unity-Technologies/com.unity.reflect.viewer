namespace Unity.Properties.UI.Internal
{
    partial class InspectorVisitor
        : IResetableVisitor
    {
        public readonly InspectorContext Context;
        
        public InspectorVisitor(BindingContextElement root)
        {
            Context = new InspectorContext(root);
        }

        void IResetableVisitor.Reset()
        {
            Context.Reset();
        }
    }
}
