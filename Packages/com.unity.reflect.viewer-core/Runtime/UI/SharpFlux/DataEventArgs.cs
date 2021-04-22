using System;

namespace SharpFlux
{
    public class DataEventArgs<TData> : EventArgs
    {
        private DataEventArgs(TData data)
        {
            Data = data;
        }
        
        public TData Data { get; }
        public static DataEventArgs<TData> From(TData data)
            => new DataEventArgs<TData>(data);
    }
}