using System.Collections.Generic;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.Actors
{
    [Actor("4d4fadea-8078-4c8b-ba91-da51d342b685", true)]
    public class HighlightActor
    {
        public static readonly int k_SelectedLayer = LayerMask.NameToLayer("BimFilterSelect");
        public static readonly int k_OtherLayer = LayerMask.NameToLayer("BimFilterOthers");

        readonly HashSet<DynamicGuid> m_CurrentlyHighlighted = new HashSet<DynamicGuid>();
        readonly Dictionary<DynamicGuid, (GameObject GameObject, int Layer)> m_AllObjects = new Dictionary<DynamicGuid, (GameObject, int)>();

        bool m_IsHighlighting;

        [PipeInput]
        void OnGameObjectCreating(PipeContext<GameObjectCreating> ctx)
        {
            foreach (var go in ctx.Data.GameObjectIds)
            {
                m_AllObjects.Add(go.Id, (go.GameObject, go.GameObject.layer));

                if (m_IsHighlighting)
                    go.GameObject.SetLayerRecursively(m_CurrentlyHighlighted.Contains(go.Id) ? k_SelectedLayer : k_OtherLayer);
            }

            ctx.Continue();
        }

        [PipeInput]
        void OnGameObjectDestroying(PipeContext<GameObjectDestroying> ctx)
        {
            foreach (var go in ctx.Data.GameObjectIds)
                m_AllObjects.Remove(go.Id);

            ctx.Continue();
        }

        [NetInput]
        void CancelHighlight(NetContext<CancelHighlight> ctx)
        {
            m_IsHighlighting = false;
            m_CurrentlyHighlighted.Clear();
            foreach (var obj in m_AllObjects)
                obj.Value.GameObject.SetLayerRecursively(obj.Value.Layer);
        }

        [NetInput]
        void OnAddHighlight(NetContext<AddToHighlight> ctx)
        {
            m_IsHighlighting = true;
            foreach (var id in ctx.Data.HighlightedInstances)
            {
                m_CurrentlyHighlighted.Add(id);
                m_AllObjects[id].GameObject.SetLayerRecursively(k_SelectedLayer);
            }
        }
        [NetInput]
        void OnRemoveHighlight(NetContext<RemoveFromHighlight> ctx)
        {
            m_IsHighlighting = true;
            foreach (var id in ctx.Data.OtherInstances)
            {
                if (m_CurrentlyHighlighted.Contains(id))
                    m_CurrentlyHighlighted.Remove(id);

                m_AllObjects[id].GameObject.SetLayerRecursively(k_OtherLayer);
            }
        }
        [NetInput]
        void OnSetHighlight(NetContext<SetHighlight> ctx)
        {
            m_IsHighlighting = true;
            m_CurrentlyHighlighted.Clear();
            foreach (var id in ctx.Data.HighlightedInstances)
                m_CurrentlyHighlighted.Add(id);

            foreach (var kvp in m_AllObjects)
                kvp.Value.GameObject.SetLayerRecursively(m_CurrentlyHighlighted.Contains(kvp.Key) ? k_SelectedLayer : k_OtherLayer);
        }
    }
}
