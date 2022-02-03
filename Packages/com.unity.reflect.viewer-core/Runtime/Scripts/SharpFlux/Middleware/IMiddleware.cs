
namespace SharpFlux.Middleware
{
    public interface IMiddleware<TPayload>
    {
        bool Apply(ref TPayload payload);
    }
}
