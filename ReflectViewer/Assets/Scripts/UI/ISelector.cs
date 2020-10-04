namespace Unity.Reflect.Viewer.UI
{
    public interface ISelector<TValue>
    {
        TValue getValueByName(string name);
    }
}
