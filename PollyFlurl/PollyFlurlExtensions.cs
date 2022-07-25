using System.Net;

namespace PollyFlurl;
public static class PollyFlurlExtensions
{
    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<IFlurlResponse> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<IFlurlResponse> policy)
    {
        return new PollyRequestFlurlResponse(request, policy);
    }

    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy policy)
    {
        return new PollyRequest(request, policy);
    }

    public static IFlurlRequest WithPolicy(this string request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new Url(request), policy);
    public static IFlurlRequest WithPolicy(this Url request, IAsyncPolicy<HttpResponseMessage> policy) => WithPolicy(new FlurlRequest(request), policy);
    public static IFlurlRequest WithPolicy(this IFlurlRequest request, IAsyncPolicy<HttpResponseMessage> policy)
    {
        return new PollyHttpResponseRequest(request, policy);
    }

    static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };

    public static IFlurlRequest RetryTransientErrors(this string request) => RetryTransientErrors(new Url(request));
    public static IFlurlRequest RetryTransientErrors(this Url request) => RetryTransientErrors(new FlurlRequest(request));
    public static IFlurlRequest RetryTransientErrors(this IFlurlRequest request) =>
        WithPolicy(request,
            Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync());
}