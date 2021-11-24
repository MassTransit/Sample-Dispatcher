namespace Sample.Components.Services
{
    using System.Threading.Tasks;


    public interface IRequestRoutingService
    {
        Task<RouteResult> RouteRequest(string? routingKey);
    }
}
