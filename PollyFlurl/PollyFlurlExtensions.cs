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
    static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };


    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<IFlurlResponse> policy)
    {
        return new PollyRequest(request, policy);
    }

    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<HttpResponseMessage> policy)
    {
        return new PollyHttpResponseRequest(request, policy);
    }

    public static IFlurlRequest WithRetry(this string request) => WithRetry(new Url(request));
    public static IFlurlRequest WithRetry(this Url request) => WithRetry(new FlurlRequest(request));
    public static IFlurlRequest WithRetry(this IFlurlRequest request) =>
        WithPolicy(request,
            Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync());
}