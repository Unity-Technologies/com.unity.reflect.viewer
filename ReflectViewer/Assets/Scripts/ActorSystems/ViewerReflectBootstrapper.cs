using System;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using UnityEngine;
using UnityEngine.Reflect;
using Object = UnityEngine.Object;

namespace Unity.Reflect.Viewer
{
    public class ViewerReflectBootstrapper : ReflectActorSystem
    {
#pragma warning disable 649
        [SerializeField]
        string m_ApiKey;
#pragma warning restore 649

        public event Action<BridgeActor.Proxy, bool> ActorSystemStarting;
        public event Action<BridgeActor.Proxy> ActorSystemStarted;
        public event Action<BridgeActor.Proxy> StreamingStarting;
        public event Action<BridgeActor.Proxy> StreamingStarted;

        public ViewerBridgeActor.Proxy ViewerBridge { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!string.IsNullOrEmpty(m_ApiKey))
                UnityEngine.Reflect.ProjectServer.SetAppId(m_ApiKey);
        }

        public void OpenProject(Project project, UnityUser user, AccessToken accessToken, bool isRestarting, Action<BridgeActor.Proxy> settingsOverrideAction)
        {
            if (isRestarting)
                Restart();
            else
            {
                Instantiate(project, user, accessToken,
                    bridge =>
                    {
                        var viewerBridge = Hook.GetActor<ViewerBridgeActor>();
                        ViewerBridge = new ViewerBridgeActor.Proxy(viewerBridge);
                        settingsOverrideAction(bridge);
                    },
                    runner =>
                    {
                        var bridge = runner.GetActor<ViewerBridgeActor>();
                        bridge.SetActorRunner(Hook.Systems.ActorRunner);
                    });
            }

            ActorSystemStarting?.Invoke(Bridge, isRestarting);
            StartActorSystem();
            ActorSystemStarted?.Invoke(Bridge);

            StreamingStarting?.Invoke(Bridge);
            Bridge.SendUpdateManifests();
            StreamingStarted?.Invoke(Bridge);
        }
    }
}
