using System;
using System.Collections.Generic;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using Unity.Reflect.Collections;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    [Actor("23b4deef-deaf-4fe1-8ed3-f567bd8f87c5", true)]
    public class ViewerBridgeActor
    {
#pragma warning disable 649
        NetComponent m_Net;

        RpcOutput<SpatialPickingArguments> m_SpatialPickingOutput;
#pragma warning restore 649

        event Action<GameObjectCreating> GameObjectCreating;
        event Action<GameObjectDestroying> GameObjectDestroying;
        event Action<GameObjectEnabling> GameObjectEnabling;
        event Action<GameObjectDisabling> GameObjectDisabling;

        ActorRunner.Proxy m_Runner;

        public void SetActorRunner(ActorRunner.Proxy runner)
        {
            m_Runner = runner;
        }

        public TickResult Tick(TimeSpan endTime)
        {
            // Skip endTime and force callback processing to be instant
            return m_Net.Tick(TimeSpan.MaxValue);
        }
        
        [RpcInput]
        void OnExecuteSyncEvent(RpcContext<ExecuteSyncEvent, NullData> ctx)
        {
            switch (ctx.Data.Event)
            {
                case GameObjectCreating msg:
                    GameObjectCreating?.Invoke(msg);
                    break;
                case GameObjectDestroying msg:
                    GameObjectDestroying?.Invoke(msg);
                    break;
                case GameObjectEnabling msg:
                    GameObjectEnabling?.Invoke(msg);
                    break;
                case GameObjectDisabling msg:
                    GameObjectDisabling?.Invoke(msg);
                    break;
            }

            ctx.SendSuccess(null);
        }

        void PickFromRay(Ray ray, List<ISpatialObject> results, string[] flagsExcluded)
        {
            results.Clear();
            var pickingLogic = new PickFromRay(ray);
            var cc = new ConditionCapture<bool>(false);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, cc, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, cc, result) =>
            {
                results.AddRange(result);
                cc.Data = true;
            });
            rpc.Failure((self, ctx, cc, ex) =>
            {
                Debug.LogException(ex);
                cc.Data = true;
            });

            m_Runner.ProcessUntil(cc, c => c.Data);
        }

        void PickFromRay(Ray ray, Action<List<ISpatialObject>> callback, string[] flagsExcluded)
        {
            var pickingLogic = new PickFromRay(ray);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, (object)null, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, userCtx, result) =>
            {
                callback(result);
            });
            rpc.Failure((self, ctx, userCtx, ex) =>
            {
                Debug.LogException(ex);
            });
        }

        void PickFromSamplePoints(Vector3[] samplePoints, int count, List<ISpatialObject> results, string[] flagsExcluded)
        {
            results.Clear();
            var pickingLogic = new PickFromSamplePoints(samplePoints, count);
            var cc = new ConditionCapture<bool>(false);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, cc, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, cc, result) =>
            {
                results.AddRange(result);
                cc.Data = true;
            });
            rpc.Failure((self, ctx, cc, ex) =>
            {
                Debug.LogException(ex);
                cc.Data = true;
            });

            m_Runner.ProcessUntil(cc, c => c.Data);
        }

        void PickFromSamplePoints(Vector3[] samplePoints, int count, Action<List<ISpatialObject>> callback, string[] flagsExcluded = null)
        {
            var pickingLogic = new PickFromSamplePoints(samplePoints, count);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, (object)null, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, userCtx, result) =>
            {
                callback(result);
            });
            rpc.Failure((self, ctx, userCtx, ex) =>
            {
                Debug.LogException(ex);
            });
        }

        void PickFromDistance(Vector3 origin, float distance, List<ISpatialObject> results, string[] flagsExcluded = null)
        {
            results.Clear();
            var pickingLogic = new PickFromDistance(origin, distance);
            var cc = new ConditionCapture<bool>(false);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, cc, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, cc, result) =>
            {
                results.AddRange(result);
                cc.Data = true;
            });
            rpc.Failure((self, ctx, cc, ex) =>
            {
                Debug.LogException(ex);
                cc.Data = true;
            });

            m_Runner.ProcessUntil(cc, c => c.Data);
        }

        void PickFromDistance(Vector3 origin, float distance, Action<List<ISpatialObject>> callback, string[] flagsExcluded = null)
        {
            var pickingLogic = new PickFromDistance(origin, distance);
            var rpc = m_SpatialPickingOutput.Call((object)null, (object)null, (object)null, new SpatialPickingArguments(pickingLogic, flagsExcluded));
            rpc.Success<List<ISpatialObject>>((self, ctx, userCtx, result) =>
            {
                callback(result);
            });
            rpc.Failure((self, ctx, userCtx, ex) =>
            {
                Debug.LogException(ex);
            });
        }

        public struct Proxy : ISpatialPicker<ISpatialObject>, ISpatialPickerAsync<ISpatialObject>
        {
            ViewerBridgeActor m_Self;

            public Proxy(ViewerBridgeActor self)
            {
                m_Self = self;
            }

            public bool IsInitialized => m_Self != null;

            public event Action<GameObjectCreating> GameObjectCreating
            {
                add => m_Self.GameObjectCreating += value;
                remove => m_Self.GameObjectCreating -= value;
            }

            public event Action<GameObjectDestroying> GameObjectDestroying
            {
                add => m_Self.GameObjectDestroying += value;
                remove => m_Self.GameObjectDestroying -= value;
            }

            public event Action<GameObjectEnabling> GameObjectEnabling
            {
                add => m_Self.GameObjectEnabling += value;
                remove => m_Self.GameObjectEnabling -= value;
            }

            public event Action<GameObjectDisabling> GameObjectDisabling
            {
                add => m_Self.GameObjectDisabling += value;
                remove => m_Self.GameObjectDisabling -= value;
            }
            
            // ISpatialPicker
            [Obsolete("Please use 'Pick(Ray ray, Action<List<ISpatialObject>> callback)' instead.")]
            public void Pick(Ray ray, List<ISpatialObject> results, string[] flagsExcluded = null) => m_Self.PickFromRay(ray, results, flagsExcluded);
            [Obsolete("Please use 'Pick(Ray ray, Action<List<ISpatialObject>> callback)' instead.")]
            public void VRPick(Ray ray, List<ISpatialObject> results, string[] flagsExcluded = null) => m_Self.PickFromRay(ray, results, flagsExcluded);
            [Obsolete("Please use 'Pick(Vector3[] samplePoints, int count, Action<List<ISpatialObject>> callback)' instead.")]
            public void Pick(Vector3[] samplePoints, int count, List<ISpatialObject> results, string[] flagsExcluded = null) => m_Self.PickFromSamplePoints(samplePoints, count, results, flagsExcluded);
            [Obsolete("Please use 'Pick(Vector3 origin, float distance, Action<List<ISpatialObject>> callback)' instead.")]
            public void Pick(float distance, List<ISpatialObject> results, Transform origin, string[] flagsExcluded = null) => m_Self.PickFromDistance(origin.position, distance, results, flagsExcluded);
            
            // ISpatialPickerAsync
            public void Pick(Ray ray, Action<List<ISpatialObject>> callback, string[] flagsExcluded = null) => m_Self.PickFromRay(ray, callback, flagsExcluded);
            public void Pick(Vector3[] samplePoints, int count, Action<List<ISpatialObject>> callback, string[] flagsExcluded = null) => m_Self.PickFromSamplePoints(samplePoints, count, callback, flagsExcluded);
            public void Pick(Vector3 origin, float distance, Action<List<ISpatialObject>> callback, string[] flagsExcluded = null) => m_Self.PickFromDistance(origin, distance, callback, flagsExcluded);
        }
    }
}
