namespace SharpFlux
{
    //Payload: The actual information or message in transmitted data
    public class Payload<TAction>
    {
        public TAction ActionType { get; }
        public object Data { get; }

        private Payload(TAction actionType, object data)
        {
            ActionType = actionType;
            Data = data;
        }

        public static Payload<TAction> From(TAction actionType, object data) 
            => new Payload<TAction>(actionType, data);
    }
}
