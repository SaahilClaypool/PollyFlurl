using System.Net;
using Microsoft.Extensions.Http;
using IFlurlHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace Polly.Flurl;
public static class PollyFlurlExtensions
{
    static readonly HashSet<int> HTTPTransientErrorStatusCodes = new()
    {
        (int)HttpStatusCode.InternalServerError,
        (int)HttpStatusCode.RequestTimeout,
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.GatewayTimeout,
        (int)HttpStatusCode.ServiceUnavailable,
    };

    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<IFlurlResponse> policy)
    {
        return new PollyRequest(request, policy);
    }

    public static IFlurlRequest WithRetry(this string request) => WithRetry(new Url(request));
    public static IFlurlRequest WithRetry(this Url request) => WithRetry(new FlurlRequest(request));
    public static IFlurlRequest WithRetry(this IFlurlRequest request) =>
        WithPolicy(request, Policy
            .Handle<FlurlHttpException>(ex => ex.StatusCode is not null && HTTPTransientErrorStatusCodes.Contains(ex.StatusCode.Value))
            .OrResult<IFlurlResponse>(r => false)
            .RetryAsync());
}