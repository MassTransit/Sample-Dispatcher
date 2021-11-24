namespace Sample.Components.Services
{
    using System.Threading.Tasks;
    using Contracts;


    public interface IRequestRoutingService
    {
        Task<RouteResult> RouteRequest(DispatchRequest request);
    }
}