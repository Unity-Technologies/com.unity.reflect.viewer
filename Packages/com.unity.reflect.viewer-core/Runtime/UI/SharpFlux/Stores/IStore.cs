using System;

namespace SharpFlux.Stores
{
    public interface IStore<TData>
    {
        TData Data { get; }
        string DispatchToken { get; }
        bool HasChanged { get; }
    }
}