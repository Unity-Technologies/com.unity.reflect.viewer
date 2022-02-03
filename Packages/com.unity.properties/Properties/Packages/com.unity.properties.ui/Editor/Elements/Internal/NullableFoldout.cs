using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    interface IReloadableElement
    {
        void Reload();
        void Reload(IProperty property);
    }

    class NullableFoldout<TValue> : Foldout, IBinding, IReloadableElement, IContextElement
    {
        protected BindingContextElement Root { get; private set; }
        public PropertyPath Path { get; private set; }
        protected InspectorVisitor GetVisitor() => Root.GetVisitor();
        protected TValue GetValue() => Root.TryGetValue(Path, out TValue v) ? v : default;
        protected IProperty GetProperty() => Root.TryGetProperty(Path, out var property) ? property : default;

        public NullableFoldout()
        {
            binding = this;
            AddToClassList(UssClasses.NullableFoldoutElement.NullableFoldout);
            Resources.Templates.NullableFoldout.AddStyles(this);
            this.Q<Foldout>().Q<VisualElement>(className: UssClasses.Unity.ToggleInput).AddManipulator(
                new ContextualMenuManipulator(evt =>
                {
                    var property = GetProperty();
                    if (null == property)
                        return;
                    
                    var inspectorOptions = property.GetAttribute<InspectorOptionsAttribute>();
                    
                    if (property.IsReadOnly || true == inspectorOptions?.HideResetToDefault)
                    {
                        return;
                    }

                    evt.menu.AppendAction(
                        "Reset to default",
                        p => ReloadWithInstance(),
                        p => property.HasAttribute<CreateInstanceOnInspectionAttribute>()
                            ? DropdownMenuAction.Status.Disabled
                            : DropdownMenuAction.Status.Normal);
                }));
        }

        void IContextElement.SetContext(BindingContextElement root, PropertyPath path)
        {
            Root = root;
            Path = path;
            OnContextReady();
        }
        
        public virtual void OnContextReady()
        {
        }

        void IBinding.PreUpdate()
        {
        }

        void IBinding.Update()
        {
            try
            {
                if (!Root.TryGetValue<TValue>(Path, out var current))
                {
                    if (Root.IsPathValid(Path))
                    {
                        Root.ReloadAtPath(Path, this);
                    }
                    return;
                }

                if (typeof(TValue).IsClass && EqualityComparer<TValue>.Default.Equals(current, default))
                {
                    ReloadWithInstance(default);
                    return;
                }

                OnUpdate();
            }
            catch (Exception)
            {
            }
        }

        void IBinding.Release()
        {
        }

        public void Reload()
        {
            var property = GetProperty();
            if (null == property)
                return;

            text = GuiFactory.GetDisplayName(property);
            var visitor = GetVisitor();
            if (null == visitor)
                return;
            
            using (visitor.Context.MakeParentScope(this))
            {
                visitor.Context.AddToPath(Path);
                try
                {
                    Reload(property);
                }
                finally
                {
                    visitor.Context.RemoveFromPath(Path);
                }
            }
        }

        public virtual void Reload(IProperty property)
        {
        }

        protected virtual void OnUpdate()
        {
        }
        
        protected bool HasAttribute<T>() where T:Attribute
        {
            return Root.TryGetProperty(Path, out var property) && property.HasAttribute<T>();
        }
        
        protected T GetAttribute<T>() where T:Attribute
        {
            return Root.TryGetProperty(Path, out var property) ? property.GetAttribute<T>() : null;
        }

        void ReloadWithInstance(TValue defaultValue = default)
        {
            Root.SwapWithInstance(Path, this, defaultValue);
        }
    }
}