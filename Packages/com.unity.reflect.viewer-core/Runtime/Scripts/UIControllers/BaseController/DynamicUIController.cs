using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class DynamicUIController: MonoBehaviour
    {
        protected struct HandlerData
        {
            public Action handler;
            public IPropertyValue iPropertyValue;
        }

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();
        protected List<HandlerData> m_HandlerList;

        void Start()
        {
            m_HandlerList = new List<HandlerData>();
            BindSelectors();
        }

        protected void BindSelectors(bool overwriteLabels = false)
        {
            var children = transform.GetComponentsInChildren<UISelectorComponent>();
            foreach (var childSelector in children)
            {
                if (overwriteLabels)
                {
                    var iLabel = childSelector.transform.GetComponentInChildren<IPropertyLabel>(true);
                    if (iLabel != null)
                        iLabel.label = ObjectNames.NicifyVariableName(childSelector.propertyName);
                }
                var iValue = childSelector.transform.GetComponentInChildren<IPropertyValue>(true);
                m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<object>(childSelector.context, childSelector.propertyName, (property) =>
                {
                    iValue.objectValue = property;
                }));
                Action handler = () =>
                {
                    Dispatcher.Dispatch(ModifyContextPropertyAction.From(
                        new ModifyContextPropertyActionData
                        {
                            context = childSelector.context,
                            propertyName = childSelector.propertyName,
                            propertyValue = iValue.objectValue
                        }));
                };
                iValue.AddListener(handler);
                m_HandlerList.Add(new HandlerData { handler = handler, iPropertyValue = iValue });
            }
        }

        void OnDestroy()
        {
            if (m_HandlerList != null)
            {
                foreach (var handlerData in m_HandlerList)
                {
                    handlerData.iPropertyValue.RemoveListener(handlerData.handler);
                }
                m_HandlerList.Clear();
            }

            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }
    }
}
