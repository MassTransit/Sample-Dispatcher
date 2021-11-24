namespace Sample.Components.Services
{
    public enum RouteDisposition
    {
        Unhandled = 0,
        Ambiguous = 1, // too many routes matched
        Destination = 2
    }
}