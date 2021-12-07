using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetEnableTextureAction: ActionBase
    {
        public static readonly int k_UseTexture = Shader.PropertyToID("_Reflect_UseTexture");
        public object Data { get; }

        SetEnableTextureAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var enableTexture = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = "enableTexture";

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, enableTexture);

            if (hasChanged)
            {
                if (enableTexture)
                    Shader.SetGlobalFloat(k_UseTexture, 1);
                else
                    Shader.SetGlobalFloat(k_UseTexture, 0);

                onStateDataChanged?.Invoke();
            }
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetEnableTextureAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SceneOptionContext.current;
        }
    }
}
