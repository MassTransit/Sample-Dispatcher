namespace Sample.Components.Services
{
    using System.Threading.Tasks;


    public interface IRequestRoutingCandidate
    {
        Task<RouteResult?> IsValidCandidate(RequestRoutingCriteria criteria);
    }
}